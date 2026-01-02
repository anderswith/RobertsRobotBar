using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IDrinkUseCountLogic
{

    void AddDrinkUseCount(Guid drinkId, Guid eventId);
 

    IEnumerable<(string DrinkName, int TotalUseCount)> GetAllDrinkUseCountByTimeFrame(Guid eventId,
        DateTime start, DateTime end);

    IEnumerable<(String DrinkName, int TotalUseCount)> GetAllDrinksUseCountForEvent(Guid eventId);
}
