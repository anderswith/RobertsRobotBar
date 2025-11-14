using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IDrinkUseCountRepository
{
    void AddDrinkUseCount(DrinkUseCount drinkUseCount);
    IEnumerable<DrinkUseCount> GetAllDrinkUseCounts();
    IEnumerable<DrinkUseCount> GetAllDrinkUseCountByTimeFrame(DateTime start, DateTime end);
}