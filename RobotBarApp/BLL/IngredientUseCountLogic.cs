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
    
    public void AddIngredientUseCount(Guid ingredientId, Guid eventId)
    {
        if (Guid.Empty == ingredientId)
        {
            throw new ArgumentException("Ingredient ID must be a valid GUID.");
        }
        var ingredientUseCount = new IngredientUseCount
        {
            UseCountId = Guid.NewGuid(),
            TimeStamp = DateTime.Now,
            IngredientId = ingredientId,
            EventId = eventId
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
        if (Guid.Empty == eventId)
        {
            throw new ArgumentException("Event ID must be a valid GUID.");
        }
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
        if (Guid.Empty == eventId)
        {
            throw new ArgumentException("Event ID must be a valid GUID.");
        }

        if (start>= end)
        {
            throw new ArgumentException("Start time must be earlier than end time.");
        }
        if (start == default || end == default)
        {
            throw new ArgumentException("Start time and end time must be valid dates.");
        }

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