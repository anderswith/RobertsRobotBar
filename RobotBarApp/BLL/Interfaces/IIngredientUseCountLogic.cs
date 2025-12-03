using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IIngredientUseCountLogic
{
    public void AddIngredientUseCount(Guid ingredientId);
    public IEnumerable<IngredientUseCount> GetAllIngredientUseCounts();
    IEnumerable<(string IngredientName, int TotalUseCount)> GetAllIngredientUseCountByTimeFrame(
        DateTime start, DateTime end);
}