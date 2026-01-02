using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IDrinkUseCountRepository
{
    void AddDrinkUseCount(DrinkUseCount drinkUseCount);

    (List<Drink> Drinks, List<DrinkUseCount> DrinkUses)
        GetAllDrinksUseCountForEvent(Guid eventId);
}


