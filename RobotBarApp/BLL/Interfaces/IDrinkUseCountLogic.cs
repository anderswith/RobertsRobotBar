using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IDrinkUseCountLogic
{

    void AddDrinkUseCount(Guid drinkId);
    IEnumerable<DrinkUseCount> GetAllDrinkUseCounts();

    IEnumerable<(string DrinkName, int TotalUseCount)> GetAllDrinkUseCountByTimeFrame(
        DateTime start, DateTime end);

}
