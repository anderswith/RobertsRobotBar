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

    public IEnumerable<(string IngredientName, int TotalUseCount)> GetAllIngredientUseCountByTimeFrame(DateTime start, DateTime end)
    {
        if(start >= end)
        {
            throw new ArgumentException("Start time must be earlier than end time.");
        }
        if(start == default || end == default)
        {
            throw new ArgumentException("Start time and end time must be valid dates.");
        }
        
        var list = _ingredientUseCountRepository.GetAllIngredientUseCountByTimeFrame(start, end);
        if(list == null || !list.Any())
        {
            throw new InvalidOperationException("No ingredient use counts found in the specified time frame.");
        }
        return list
            .GroupBy(iuc => iuc.IngredientId)
            .Select(g => 
                (
                    IngredientName: g.First().Ingredient.Name, 
                    TotalUseCount: g.Count() 
                )
            )
            .OrderByDescending(x => x.TotalUseCount)
            .ToList();
    }
}