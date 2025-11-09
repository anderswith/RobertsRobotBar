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
    
   /* public void AddMenuWithDrinks(string name, List<Guid> drinkIds)
    {
        // Fetch drinks from DB to ensure they exist
        var drinks = _drinkRepository.GetDrinksByIds(drinkIds).ToList();
        if (!drinks.Any())
            throw new InvalidOperationException("No valid drinks provided for the menu.");

        // Create the new menu
        var menu = new Menu
        {
            MenuId = Guid.NewGuid(),
            Name = name,
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
    */
  
}