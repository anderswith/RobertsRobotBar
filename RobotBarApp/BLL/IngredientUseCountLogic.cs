using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class IngredientUseCountLogic : IIngredientUseCountLogic
{
    private readonly IIngredientUseCountRepository _ingredientUseCountRepository;
    public IngredientUseCountLogic(IIngredientUseCountRepository ingredientUseCountRepository)
    {
        _ingredientUseCountRepository = ingredientUseCountRepository;
    }
    
    public void AddIngredientUseCount(Guid ingredientId)
    {
        var ingredientUseCount = new IngredientUseCount
        {
            UseCountId = Guid.NewGuid(),
            TimeStamp = DateTime.Now,
            IngredientId = ingredientId,
        };
        
        _ingredientUseCountRepository.AddIngredientUseCount(ingredientUseCount);

    }

    public IEnumerable<IngredientUseCount> GetAllIngredientUseCounts()
    {
        return _ingredientUseCountRepository.GetAllIngredientUseCounts();
    }
    public IEnumerable<(string IngredientName, int TotalUseCount)>
        GetAllIngredientsUseCountForEvent(Guid eventId)
    {
        var (ingredients, uses) = _ingredientUseCountRepository
            .GetIngredientUseCountForEvent(eventId);

        return uses
            .GroupBy(u => u.IngredientId)
            .Select(g =>
            {
                var name = ingredients.First(i => i.IngredientId == g.Key).Name;
                return (IngredientName: name, TotalUseCount: g.Count());
            })
            .OrderByDescending(x => x.TotalUseCount)
            .ToList();
    }

    public IEnumerable<(string IngredientName, int TotalUseCount)>
        GetIngredientUseCountByTimeFrame(Guid eventId, DateTime start, DateTime end)
    {
        var (ingredients, uses) = _ingredientUseCountRepository
            .GetIngredientUseCountForEvent(eventId);

        var filtered = uses
            .Where(u => u.TimeStamp >= start && u.TimeStamp <= end)
            .ToList();

        return filtered
            .GroupBy(u => u.IngredientId)
            .Select(g =>
            {
                var name = ingredients.First(i => i.IngredientId == g.Key).Name;
                return (IngredientName: name, TotalUseCount: g.Count());
            })
            .OrderByDescending(x => x.TotalUseCount)
            .ToList();
    }
}