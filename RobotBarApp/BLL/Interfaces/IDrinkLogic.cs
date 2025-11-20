using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IDrinkLogic
{
    IEnumerable<Drink> GetAllDrinks();
    Drink? GetDrinkById(Guid drinkId);

    void AddDrink(string name, string image, bool IsMocktail, List<Guid> ingredientIds, List<string> scriptNames);
    void DeleteDrink(Guid drinkId);

    void UpdateDrink(Guid drinkId, string name, string image, bool isMocktail, List<Guid> ingredientIds,
        List<string> scriptNames);

}