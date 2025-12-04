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
            .FirstOrDefault(d => d.DrinkId == drinkId);
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
    
}