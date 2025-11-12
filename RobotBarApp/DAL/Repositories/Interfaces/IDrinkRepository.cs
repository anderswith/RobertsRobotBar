using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IDrinkRepository
{
    
    void AddDrink(Drink drink);
    IEnumerable<Drink> GetAllDrinks();
    Drink? GetDrinkById(Guid drinkId);
    void DeleteDrink(Drink drink);
    void UpdateDrink(Drink drink);
    IEnumerable<Drink> GetDrinksByIds(IEnumerable<Guid> drinkIds);
}