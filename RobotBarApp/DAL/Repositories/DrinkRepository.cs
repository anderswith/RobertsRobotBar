using Microsoft.EntityFrameworkCore;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.DAL.Repositories;

public class DrinkRepository : IDrinkRepository
{
    private readonly RobotBarContext _context;
    
    public DrinkRepository(RobotBarContext context)
    {
        _context = context;
    }
    
    public void AddDrink(Drink drink)
    {
        _context.Drinks.Add(drink);
        _context.SaveChanges();
    }
    public IEnumerable<Drink> GetAllDrinks()
    {
        return _context.Drinks
            .Include(d => d.DrinkContents)
            .ThenInclude(dc => dc.Ingredient)
            .ToList();
    }
    public Drink? GetDrinkById(Guid drinkId)
    {
        return _context.Drinks
            .Include(d => d.DrinkContents)
            .ThenInclude(dc => dc.Ingredient)
            .Include(d => d.DrinkScripts)
            .FirstOrDefault(d => d.DrinkId == drinkId);
    }
    

    public IEnumerable<Ingredient> GetAvailableIngredientsForDrink(Guid drinkId)
    {
        // Get eventId via menu
        var eventId =
            _context.MenuContents
                .Where(mc => mc.DrinkId == drinkId)
                .Join(_context.Menus,
                    mc => mc.MenuId,
                    m => m.MenuId,
                    (mc, m) => m)
                .Join(_context.Events,
                    m => m.MenuId,
                    e => e.MenuId,
                    (m, e) => e.EventId)
                .FirstOrDefault();

        if (eventId == Guid.Empty)
            return new List<Ingredient>();

        // Get ingredients from BarSetup
        return _context.BarSetups
            .Where(bs => bs.EventId == eventId)
            .Select(bs => bs.Ingredient)
            .Distinct()
            .ToList();
    }
    
    public IEnumerable<Drink> GetDrinksByIds(IEnumerable<Guid> drinkIds)
    {
        return _context.Drinks
            .Where(d => drinkIds.Contains(d.DrinkId))
            .ToList();
    }
    public void DeleteDrink(Drink drink)
    {
        _context.Drinks.Remove(drink);
        _context.SaveChanges();
    }
    public void UpdateDrink(Drink drink)
    {
        _context.Drinks.Update(drink);
        _context.SaveChanges();
    }
    public void RemoveDrinkContent(DrinkContent content)
    {
        _context.DrinkContents.Remove(content);
    }
    public Drink? GetDrinkWithScripts(Guid drinkId)
    {
        return _context.Drinks
            .Include(d => d.DrinkScripts)
            .FirstOrDefault(d => d.DrinkId == drinkId);
    }

    public IEnumerable<Drink> GetAllDrinksWithContent()
    {
        return _context.Drinks
            .Include(d => d.DrinkContents)
            .ToList();
    }
    public bool Exists(Guid drinkId)
    {
        return _context.Drinks.Any(d => d.DrinkId == drinkId);
    }

}