using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class MenuLogic
{
    private readonly IMenuRepository _menuRepository;
    private readonly IDrinkRepository _drinkRepository;
    
    public MenuLogic(IMenuRepository menuRepository, IDrinkRepository drinkRepository)
    {
        _menuRepository = menuRepository;
        _drinkRepository = drinkRepository;
    }
    
   public void AddMenuWithDrinks(string name, List<Guid> drinkIds)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Menu name cannot be null or empty.");
        }
        var drinks = _drinkRepository.GetDrinksByIds(drinkIds).ToList();
        if (drinkIds == null || !drinkIds.Any())
        {
            throw new InvalidOperationException("No valid drinks found for the provided IDs.");
        }

        var menu = new Menu
        {
            MenuId = Guid.NewGuid(),
            Name = name,
            MenuContents = new List<MenuContent>(),
        };
        
        foreach (var drink in drinks)
        {
            var content = new MenuContent
            {
                MenuContentId = Guid.NewGuid(),
                Menu = menu,
                Drink = drink
                
            };
            menu.MenuContents.Add(content);
        }
        
        _menuRepository.AddMenu(menu);
    }
   public IEnumerable<Menu> GetAllMenus()
    {
        return _menuRepository.GetAllMenus();
    }
   
   public Menu? GetMenuById(Guid menuId)
    {
        return _menuRepository.GetMenuById(menuId);
    }

    public void DeleteMenu(Guid menuId)
    {
        var menuToDelete = _menuRepository.GetMenuById(menuId);
        if (menuToDelete == null)
        {
            throw new KeyNotFoundException($"Menu not found.");
        }

        _menuRepository.DeleteMenu(menuToDelete);
    }

    public void UpdateMenu(Guid menuId, string name, List<Guid> drinkIds)
    {
        if (string.IsNullOrEmpty(name))
        {
            throw new ArgumentException("Menu name cannot be null or empty.");
        }

        if (drinkIds == null || !drinkIds.Any())
        {
            throw new ArgumentException("Menu must contain at least one drink.");
        }

        var existingMenu = _menuRepository.GetMenuById(menuId);
        if (existingMenu == null)
        {
            throw new KeyNotFoundException($"Menu not found.");
        }

        var drinks = _drinkRepository.GetDrinksByIds(drinkIds).ToList();
        if (!drinks.Any())
        {
            throw new InvalidOperationException("No valid drinks found for the provided IDs.");
        }
        
        existingMenu.Name = name;
        existingMenu.MenuContents.Clear();
        
        foreach (var drink in drinks)
        {
            existingMenu.MenuContents.Add(new MenuContent
            {
                MenuContentId = Guid.NewGuid(),
                MenuId = existingMenu.MenuId,
                DrinkId = drink.DrinkId
            });
        }
        
        _menuRepository.UpdateMenu(existingMenu);
    }

 
  
}