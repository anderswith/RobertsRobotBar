using RobotBarApp.BE;
using RobotBarApp.DAL.Repositories.Interfaces;

namespace RobotBarApp.BLL;

public class IngredientLogic
{
    private readonly IIngredientRepository _ingredientRepository;
    public IngredientLogic(IIngredientRepository ingredientRepository)
    {
        _ingredientRepository = ingredientRepository;
    }
    public void AddIngredient(Ingredient ingredient)
    {
        _ingredientRepository.AddIngredient(ingredient);
    }
    public IEnumerable<Ingredient> GetAllIngredients()
    {
        return _ingredientRepository.GetAllIngredients();
    }
    public Ingredient? GetIngredientById(Guid ingredientId)
    {
        return _ingredientRepository.GetIngredientById(ingredientId);
    }
    public void DeleteIngredient(Ingredient ingredient)
    {
        _ingredientRepository.DeleteIngredient(ingredient);
    }
    public void UpdateIngredient(Ingredient ingredient)
    {
        _ingredientRepository.UpdateIngredient(ingredient);
    }
}