using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.DAL.Repositories.Interfaces;
using RobotBarApp.Services.Application.Interfaces;

namespace RobotBarApp.BLL;

public class MenuLogic : IMenuLogic
{
    private readonly IMenuRepository _menuRepository;
    private readonly IDrinkRepository _drinkRepository;
    private readonly IEventRepository _eventRepository;
    private readonly IEventSessionService _eventSessionService;
    
    public MenuLogic(IMenuRepository menuRepository, IDrinkRepository drinkRepository, IEventRepository eventRepository, IEventSessionService eventSessionService)
    {
        _menuRepository = menuRepository;
        _drinkRepository = drinkRepository;
        _eventRepository = eventRepository;
        _eventSessionService = eventSessionService;
    }
    
    public void AddDrinksToMenu(List<Guid> drinkIds, Guid eventId)
    {
        if (drinkIds == null || drinkIds.Count == 0)
        {
            throw new ArgumentException("Drink IDs cannot be null or empty.");
        }
        
        var menu = _menuRepository.GetMenuWithContentByEventId(eventId);
        var menuId = menu.MenuId;
        Console.WriteLine("Resolved menu ID: " + menuId);
        if (menuId == null || menuId == Guid.Empty)
        {
            throw new KeyNotFoundException("Menu not found for the event.");
        }
        _menuRepository.AddDrinksToMenu(menuId, drinkIds);

        Console.WriteLine("Added " + drinkIds.Count + " drinks to menu.");
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

    public IEnumerable<Drink> GetMenuWithDrinksAndIngredients()
    {
        if (!_eventSessionService.HasActiveEvent)
            throw new InvalidOperationException("No active event");

        var eventId = _eventSessionService.CurrentEventId.Value;
        var menu = _menuRepository.GetMenuWithDrinksAndIngredientsByEventId(eventId);
        if (menu == null)
        {
            throw new KeyNotFoundException("Menu not found for active event");
        }

        return menu.MenuContents
            .Select(mc => mc.Drink)
            .ToList();
    }


 
  
}