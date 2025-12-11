using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Application.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

namespace RobotBarApp.BLL;

public class RobotLogic : IRobotLogic
{
    private readonly IRobotScriptRunner _scriptRunner;
    private readonly IIngredientLogic _ingredientLogic;
    private readonly IDrinkLogic _drinkLogic;
    private readonly IDrinkUseCountLogic _drinkUseCountLogic;
    private readonly IIngredientUseCountLogic _ingredientUseCountLogic;
    private readonly IEventSessionService _eventSession;
    
    public RobotLogic(IRobotScriptRunner scriptRunner, 
        IIngredientLogic ingredientLogic, 
        IDrinkLogic drinkLogic,
        IIngredientUseCountLogic ingredientUseCountLogic, 
        IDrinkUseCountLogic drinkUseCountLogic, 
        IEventSessionService eventSessionService)
    {
        _scriptRunner = scriptRunner;
        _ingredientLogic = ingredientLogic;
        _drinkLogic = drinkLogic;
        _ingredientUseCountLogic = ingredientUseCountLogic;
        _drinkUseCountLogic = drinkUseCountLogic;
        _eventSession = eventSessionService;
    }
    
    public void RunRobotScripts(IEnumerable<string> scripts)
    {
        _scriptRunner.QueueScripts(scripts);
    }

    public void RunIngredientScript(List<Guid> ingredientIds)
    {
        if(ingredientIds == null || ingredientIds.Count == 0)
        {
            throw new ArgumentException("Ingredient IDs cannot be null or empty.");
        }
        
        var ingredients = _ingredientLogic.GetIngredientsWithScripts(ingredientIds);
        if(ingredients == null || ingredients.Count() == 0)
        {
            throw new ArgumentException("No ingredients found with the provided IDs.");
        }

        var eventId = _eventSession.CurrentEventId!.Value;
        foreach(var ingredientId in ingredientIds)
        {
            _ingredientUseCountLogic.AddIngredientUseCount(ingredientId, eventId);
        }

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
        var eventId = _eventSession.CurrentEventId!.Value;
        if(drinkId == Guid.Empty)
        {
            throw new ArgumentException("Drink ID cannot be empty.");
        }
        
        var drink = _drinkLogic.GetDrinksWithScripts(drinkId);
        if (drink == null)
        {
            throw new ArgumentException("Drink not found.");
        }
        _drinkUseCountLogic.AddDrinkUseCount(drinkId, eventId);
        
        var ingredientIds = drink.DrinkContents
            .Select(dc => dc.IngredientId)
            .ToList();

        foreach (var ingredientId in ingredientIds)
        {
            _ingredientUseCountLogic.AddIngredientUseCount(ingredientId, eventId);
        }

        var scripts = new List<string>();

        foreach (var script in drink.DrinkScripts.OrderBy(s => s.Number))
        {
            scripts.Add(script.UrScript);
        }

        _scriptRunner.QueueScripts(scripts);
    }
}
    
