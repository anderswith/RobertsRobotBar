using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IMenuLogic
{
    void AddDrinksToMenu(List<Guid> drinkIds, Guid eventId);

    IEnumerable<Drink> GetDrinksForMenu(Guid eventId);
    void RemoveDrinkFromMenu(Guid eventId, Guid drinkId);

    IEnumerable<Drink> GetMenuWithDrinksAndIngredients();
}