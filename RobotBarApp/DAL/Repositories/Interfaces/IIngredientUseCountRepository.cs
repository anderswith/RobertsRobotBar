using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IIngredientUseCountRepository
{
    public void AddIngredientUseCount(IngredientUseCount ingredientUseCount);
    public IEnumerable<IngredientUseCount> GetAllIngredientUseCounts();

    (List<Ingredient> Ingredients, List<IngredientUseCount> IngredientUses)
        GetIngredientUseCountForEvent(Guid eventId);

}