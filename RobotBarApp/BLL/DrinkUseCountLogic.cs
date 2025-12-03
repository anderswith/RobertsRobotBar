using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class DrinkUseCountLogic : IDrinkUseCountLogic
{
    private readonly IDrinkUseCountRepository _drinkUseCountRepository;
    public DrinkUseCountLogic(IDrinkUseCountRepository drinkUseCountRepository)
    {
        _drinkUseCountRepository = drinkUseCountRepository;
    }
    public void AddDrinkUseCount(Guid drinkId)
    {
        var drinkUseCount = new DrinkUseCount
        {
            UseCountId = Guid.NewGuid(),
            TimeStamp = DateTime.Now,
            DrinkId = drinkId
        };
        _drinkUseCountRepository.AddDrinkUseCount(drinkUseCount);
    }

    public IEnumerable<DrinkUseCount> GetAllDrinkUseCounts()
    {
        return _drinkUseCountRepository.GetAllDrinkUseCounts();
    }

    public IEnumerable<(string DrinkName, int TotalUseCount)> GetAllDrinkUseCountByTimeFrame(
        DateTime start, DateTime end)
    {
        if(start >= end)
        {
            throw new ArgumentException("Start time must be earlier than end time.");
        }
        if(start == default || end == default)
        {
            throw new ArgumentException("Start time and end time must be valid dates.");
        }
        
        var list = _drinkUseCountRepository.GetAllDrinkUseCountByTimeFrame(start, end);
        if(list == null || !list.Any()) 
        {
            throw new InvalidOperationException("No drink use counts found in the specified time frame.");
        }
        
        var stats = list
            .GroupBy(uc => uc.DrinkId)     
            .Select(g => 
                (
                    DrinkName: g.First().drink.Name,    
                    TotalUseCount: g.Count()            
                )
            )
            .OrderByDescending(x => x.TotalUseCount)
            .ToList();

        return stats;
    }
}