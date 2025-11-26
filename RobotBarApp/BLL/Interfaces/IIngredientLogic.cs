using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IIngredientLogic
{
    void AddIngredient(string name, string type, string image, double size, string dose, int positionNumber, List<string> scriptNames);
    IEnumerable<Ingredient> GetAllIngredients();
    Ingredient? GetIngredientById(Guid ingredientId);
    void DeleteIngredient(Guid ingredientId);
    void UpdateIngredient(Guid ingredientId, string name, string type, string image, double size, string dose, int positionNumber, List<string> scriptNames);
    IEnumerable<Ingredient> GetAlcohol();
    IEnumerable<Ingredient> GetSyrups();
    IEnumerable<Ingredient> GetSoda();
    IEnumerable<Ingredient> GetIngredientsWithScripts(List<Guid> ingredientIds);

}