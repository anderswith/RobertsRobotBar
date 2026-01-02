using System.Xml.Linq;
using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IMenuRepository
{
    void UpdateMenu(Menu menu);
    Menu GetMenuWithContentByEventId(Guid eventId);
    void AddDrinksToMenu(Guid menuId, IEnumerable<Guid> drinkIds);
    Menu? GetMenuWithDrinksAndIngredientsByEventId(Guid eventId);
}