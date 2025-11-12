using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IIngredientUseCountRepository
{
    public void AddIngredientUseCount(IngredientUseCount ingredientUseCount);
    public IEnumerable<IngredientUseCount> GetAllIngredientUseCounts();
    public IEnumerable<IngredientUseCount> GetAllIngredientUseCountByTimeFrame(DateTime start, DateTime end);
    
}