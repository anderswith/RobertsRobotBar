using Microsoft.EntityFrameworkCore;
using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.DAL.Repositories;

public class MenuRepository : IMenuRepository
{
    private readonly RobotBarContext _context;
    
    public MenuRepository(RobotBarContext context)
    {
        _context = context;
    }


    public void UpdateMenu(Menu menu)
    {
        _context.Menus.Update(menu);
        _context.SaveChanges();
    }
    
   
    public Menu? GetMenuWithContentByEventId(Guid eventId)
    {
        var menuId = _context.Events
            .Where(e => e.EventId == eventId)
            .Select(e => e.MenuId)
            .FirstOrDefault();

        if (menuId == Guid.Empty)
            return null;

        return _context.Menus
            .Include(m => m.MenuContents)   
            .FirstOrDefault(m => m.MenuId == menuId);
    }
    public void AddDrinksToMenu(Guid menuId, IEnumerable<Guid> drinkIds)
    {
        // Optional: prevent duplicates by checking existing rows
        var existingDrinkIds = _context.MenuContents
            .Where(mc => mc.MenuId == menuId)
            .Select(mc => mc.DrinkId)
            .ToHashSet();

        var newRows = drinkIds
            .Where(did => !existingDrinkIds.Contains(did)) // skip already-added drinks
            .Select(did => new MenuContent
            {
                MenuContentId = Guid.NewGuid(),
                MenuId = menuId,
                DrinkId = did
            })
            .ToList();

        if (!newRows.Any())
            return;

        _context.MenuContents.AddRange(newRows);
        _context.SaveChanges();
    }
    
    public Menu? GetMenuWithDrinksAndIngredientsByEventId(Guid eventId)
    {
        return _context.Menus
            .Where(m => m.Event.EventId == eventId)
            .Include(m => m.MenuContents)
            .ThenInclude(mc => mc.Drink)
            .ThenInclude(d => d.DrinkContents)
            .ThenInclude(dc => dc.Ingredient)
            .FirstOrDefault();
        
        
    }
    
    
}