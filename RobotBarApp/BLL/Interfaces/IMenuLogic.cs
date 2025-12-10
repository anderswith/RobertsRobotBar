using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IMenuLogic
{
    void AddDrinksToMenu(List<Guid> drinkIds, Guid eventId);
    IEnumerable<Menu> GetAllMenus();
    Menu? GetMenuById(Guid menuId);
    void DeleteMenu(Guid menuId);
    void UpdateMenu(Guid menuId, string name, List<Guid> drinkIds);
    IEnumerable<Drink> GetDrinksForMenu(Guid eventId);
    void RemoveDrinkFromMenu(Guid eventId, Guid drinkId);

}