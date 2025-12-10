using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class MenuLogic : IMenuLogic
{
    private readonly IMenuRepository _menuRepository;
    private readonly IDrinkRepository _drinkRepository;
    private readonly IEventRepository _eventRepository;
    
    public MenuLogic(IMenuRepository menuRepository, IDrinkRepository drinkRepository, IEventRepository eventRepository)
    {
        _menuRepository = menuRepository;
        _drinkRepository = drinkRepository;
        _eventRepository = eventRepository;
    }
    
    public void AddDrinksToMenu(List<Guid> drinkIds, Guid eventId)
    {
        Console.WriteLine("Adding drinks to menu for event: " + eventId);

        if (drinkIds == null || drinkIds.Count == 0)
        {
            throw new ArgumentException("Drink IDs cannot be null or empty.");
        }
            
        Console.WriteLine($"Drink ID to add: {string.Join(", ", drinkIds)}");
        // 1. Resolve menuId from the event
        var menu = _menuRepository.GetMenuWithContentByEventId(eventId);
        var menuId = menu.MenuId;
        Console.WriteLine("Resolved menu ID: " + menuId);
        if (menuId == null || menuId == Guid.Empty)
        {
            throw new KeyNotFoundException("Menu not found for the event.");
        }
        
        // 2. Let repository do the insert
        _menuRepository.AddDrinksToMenu(menuId, drinkIds);

        Console.WriteLine("Added " + drinkIds.Count + " drinks to menu.");
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
    public IEnumerable<Drink> GetDrinksForMenu(Guid eventId)
    {
        // 1. Load Menu + MenuContents (child)
        var menu = _menuRepository.GetMenuWithContentByEventId(eventId);

        if (menu == null)
        {
            throw new KeyNotFoundException("Menu not found for this event.");
        }
        
        // 2. Extract DrinkIds from MenuContents
        var drinkIds = menu.MenuContents
            .Select(mc => mc.DrinkId)
            .ToList();

        if (!drinkIds.Any())
            return Enumerable.Empty<Drink>();

        // 3. Query drinks using DrinkRepository
        return _drinkRepository.GetDrinksByIds(drinkIds);
    }
    
    public void RemoveDrinkFromMenu(Guid eventId, Guid drinkId)
    {
        // 1. Load menu with children
        var menu = _menuRepository.GetMenuWithContentByEventId(eventId);
        if (menu == null)
        {
            throw new KeyNotFoundException("Menu not found for this event.");
        }

        // 2. Find matching MenuContent item
        var entry = menu.MenuContents
            .FirstOrDefault(mc => mc.DrinkId == drinkId);

        if (entry == null)
            throw new KeyNotFoundException("Drink is not on the menu.");

        // 3. Remove it from the child list
        menu.MenuContents.Remove(entry);

        // 4. Save changes
        _menuRepository.UpdateMenu(menu);
    }


 
  
}