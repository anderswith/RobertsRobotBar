using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IIngredientUseCountLogic
{
    public void AddIngredientUseCount(Guid ingredientId, Guid eventId);
    public IEnumerable<IngredientUseCount> GetAllIngredientUseCounts();

    IEnumerable<(string IngredientName, int TotalUseCount)>
        GetIngredientUseCountByTimeFrame(Guid eventId, DateTime start, DateTime end);

    public IEnumerable<(string IngredientName, int TotalUseCount)>
        GetAllIngredientsUseCountForEvent(Guid eventId);
    
}