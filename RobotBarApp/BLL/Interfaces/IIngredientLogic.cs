using RobotBarApp.BE;

namespace RobotBarApp.BLL.Interfaces;

public interface IIngredientLogic
{
    void AddIngredient(string name, string type, string image, string color, int positionNumber,
        List<string> singleScriptNames, List<string> doubleScriptNames);
    IEnumerable<Ingredient> GetAllIngredients();
    Ingredient? GetIngredientById(Guid ingredientId);
    void DeleteIngredient(Guid ingredientId);
    void UpdateIngredient(Guid ingredientId, string name, string type, string image, string color, int positionNumber,
        List<string> singleScriptNames, List<string> doubleScriptNames);
    IEnumerable<Ingredient> GetAlcohol(Guid eventId);
    IEnumerable<Ingredient> GetSyrups(Guid eventId);
    IEnumerable<Ingredient> GetSoda(Guid eventId);
    IEnumerable<Ingredient> getMockohol(Guid eventId);
    IEnumerable<Ingredient> GetIngredientsWithScripts(List<Guid> ingredientIds);
    IEnumerable<Ingredient> GetIngredientsForPositions();
}