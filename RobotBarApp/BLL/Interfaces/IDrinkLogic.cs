using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IDrinkLogic
{
    IEnumerable<Drink> GetAllDrinks();
    Drink? GetDrinkById(Guid drinkId);
    
    void AddDrink(string name, string image, bool IsMocktail, List<Guid> ingredientIds);
    void DeleteDrink(Guid drinkId);
    void UpdateDrink(Drink drink);
    
}