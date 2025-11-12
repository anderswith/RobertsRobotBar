using RobotBarApp.BE;


namespace RobotBarApp.DAL.Repositories;

public class DrinkUseCountRepository
{
    private readonly RobotBarContext _context;
    public DrinkUseCountRepository(RobotBarContext context)
    {
        _context = context;
    }
    
    public void AddDrinkUseCount(DrinkUseCount drinkUseCount)
    {
        _context.DrinkUseCounts.Add(drinkUseCount);
        _context.SaveChanges();
    }
    
    public IEnumerable<DrinkUseCount> GetAllDrinkUseCounts()
    {
        return _context.DrinkUseCounts.ToList();
    }
    
    public IEnumerable<DrinkUseCount> GetAllDrinkUseCountByTimeFrame(DateTime start, DateTime end)
    {
        return _context.DrinkUseCounts
            .Where(duc => duc.TimeStamp >= start && duc.TimeStamp <= end)
            .ToList();
    }
    
}