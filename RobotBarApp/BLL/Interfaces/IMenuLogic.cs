using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IMenuLogic
{
    void AddMenuWithDrinks(string name, List<Guid> drinkIds);
    IEnumerable<Menu> GetAllMenus();
    Menu? GetMenuById(Guid menuId);
    void DeleteMenu(Guid menuId);
    void UpdateMenu(Guid menuId, string name, List<Guid> drinkIds);
    Menu GetMenuForEvent(Guid eventId);

}