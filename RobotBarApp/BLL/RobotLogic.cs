using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

namespace RobotBarApp.BLL;

public class RobotLogic : IRobotLogic
{
    private readonly IRobotScriptRunner _scriptRunner;
    private readonly IIngredientLogic _ingredientLogic;
    private readonly IDrinkLogic _drinkLogic;
    
    public RobotLogic(IRobotScriptRunner scriptRunner, IIngredientLogic ingredientLogic, IDrinkLogic drinkLogic)
    {
        _scriptRunner = scriptRunner;
        _ingredientLogic = ingredientLogic;
        _drinkLogic = drinkLogic;
    }
    
    public void RunRobotScripts(IEnumerable<string> scripts)
    {
        _scriptRunner.QueueScripts(scripts);
    }

    public void RunIngredientScript(List<Guid> ingredientIds)
    {
        var ingredients = _ingredientLogic.GetIngredientsWithScripts(ingredientIds);

        var scripts = new List<string>();

        foreach (var ing in ingredients)
        {
            foreach (var script in ing.IngredientScripts.OrderBy(s => s.Number))
            {
                scripts.Add(script.UrScript);
            }
        }

        _scriptRunner.QueueScripts(scripts);
    }

    public void RunDrinkScripts(Guid drinkId)
    {
        var drink = _drinkLogic.GetDrinksWithScripts(drinkId);
        if (drink == null)
        {
            throw new ArgumentException("Drink not found.");
        }

        var scripts = new List<string>();

        foreach (var script in drink.DrinkScripts.OrderBy(s => s.Number))
        {
            scripts.Add(script.UrScript);
        }

        _scriptRunner.QueueScripts(scripts);
    }
}
    
