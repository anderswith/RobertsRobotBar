using RobotBarApp.BE;

namespace RobotBarApp.DAL.Repositories.Interfaces;

public interface IIngredientRepository
{
    void AddIngredient(Ingredient ingredient);
    IEnumerable<Ingredient> GetAllIngredients();
    Ingredient? GetIngredientById(Guid ingredientId);
    void DeleteIngredient(Ingredient ingredient);
    void UpdateIngredient(Ingredient ingredient);
}