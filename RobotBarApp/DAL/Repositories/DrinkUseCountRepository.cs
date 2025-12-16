using Microsoft.EntityFrameworkCore;
using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;


namespace RobotBarApp.DAL.Repositories;

public class DrinkUseCountRepository : IDrinkUseCountRepository
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
    
    public (List<Drink> Drinks, List<DrinkUseCount> DrinkUses)
        GetAllDrinksUseCountForEvent(Guid eventId)
    {
        // 1) Fetch all usecounts for this event
        var drinkUses = _context.DrinkUseCounts
            .Where(duc => duc.EventId == eventId)
            .ToList();

        if (!drinkUses.Any())
            return (new(), new());

        // 2) Fetch drinks that match those usecounts
        var drinkIds = drinkUses.Select(uc => uc.DrinkId).Distinct().ToList();

        var drinks = _context.Drinks
            .Where(d => drinkIds.Contains(d.DrinkId))
            .ToList();

        return (drinks, drinkUses);
    }

    
    
}