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

    public void AddMenu(Menu menu)
    {
        _context.Add(menu);
        _context.SaveChanges();
    }
    
    public IEnumerable<Menu> GetAllMenus()
    {
        return _context.Menus.ToList();
    }
    
    public Menu? GetMenuById(Guid menuId)
    {
        return _context.Menus.FirstOrDefault(m => m.MenuId == menuId);
    }
    
    public void DeleteMenu(Menu menu)
    {
        _context.Menus.Remove(menu);
        _context.SaveChanges();
    }
    public void UpdateMenu(Menu menu)
    {
        _context.Menus.Update(menu);
        _context.SaveChanges();
    }
    
    public Menu? GetMenuWithDrinksAndIngredients(Guid menuId)
    {
        return _context.Menus
            .Where(m => m.MenuId == menuId)
            .Include(m => m.MenuContents)
            .ThenInclude(mc => mc.Drink)
            .ThenInclude(d => d.DrinkContents)
            .ThenInclude(dc => dc.Ingredient)
            .FirstOrDefault();
    }

    public Menu? GetMenuByEventId(Guid eventId)
    {
        var menuId = _context.Events
            .Where(e => e.EventId == eventId)
            .Select(e => e.MenuId)
            .FirstOrDefault();

        if (menuId == Guid.Empty)
            return null;

        return _context.Menus
            .FirstOrDefault(m => m.MenuId == menuId);
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
    
}