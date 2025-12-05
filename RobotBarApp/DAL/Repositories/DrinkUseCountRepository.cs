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
        // 1) Find menu-id
        var menuId = _context.Events
            .Where(e => e.EventId == eventId)
            .Select(e => e.MenuId)
            .FirstOrDefault();

        if (menuId == Guid.Empty)
            return (new(), new());

        // 2) Find alle drinks i menuen
        var drinkIds = _context.MenuContents
            .Where(mc => mc.MenuId == menuId)
            .Select(mc => mc.DrinkId)
            .ToList();

        // 3) Hent drinks
        var drinks = _context.Drinks
            .Where(d => drinkIds.Contains(d.DrinkId))
            .ToList();

        // 4) Hent ALLE DrinkUseCounts for disse drinks
        var drinkUses = _context.DrinkUseCounts
            .Where(duc => drinkIds.Contains(duc.DrinkId))
            .ToList();

        return (drinks, drinkUses);
    }

    
    
}