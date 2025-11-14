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
    
}