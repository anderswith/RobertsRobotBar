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
    public event Action? DrinkFinished;
    public event Action<int, int>? ScriptFinished;
    public event Action<int>? ScriptsStarted;
    
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
        
        _scriptRunner.ScriptsStarted += total =>
            ScriptsStarted?.Invoke(total);
        scriptRunner.DrinkFinished += () => DrinkFinished?.Invoke();
        _scriptRunner.ScriptFinished += (f, t) =>
            ScriptFinished?.Invoke(f, t);
    }
    
    public void RunRobotScripts(IEnumerable<string> scripts)
    {
        _scriptRunner.QueueScripts(scripts);
    }

    public void RunMixSelvScripts(List<(Guid IngredientId, int Cl)> order)
    {
        if (order == null || order.Count == 0)
        {
            throw new ArgumentException("Order cannot be null or empty.");
        }
            
        
        var ingredientIds = order.Select(o => o.IngredientId).Distinct().ToList();

        var ingredients = _ingredientLogic
            .GetIngredientsWithScripts(ingredientIds)
            .ToDictionary(i => i.IngredientId);

        var scripts = new List<string>();
        var eventId = _eventSession.CurrentEventId!.Value;

        foreach (var (ingredientId, cl) in order)
        {
            if (!ingredients.TryGetValue(ingredientId, out var ingredient))
            {
                throw new InvalidOperationException($"Ingredient not found: {ingredientId}");
            }
                

            _ingredientUseCountLogic.AddIngredientUseCount(ingredientId, eventId);

            if (cl == 2 || cl == 20)
            {
                foreach (var script in ingredient.SingleScripts.OrderBy(s => s.Number))
                    scripts.Add(script.UrScript);
            }
            else if (cl == 4)
            {
                foreach (var script in ingredient.DoubleScripts.OrderBy(s => s.Number))
                    scripts.Add(script.UrScript);
            }
            else
            {
                throw new InvalidOperationException(
                    $"Unsupported cl value {cl} for ingredient {ingredient.Name}"
                );
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
        
        foreach (var content in drink.DrinkContents)
        {
            int count =
                content.Dose == "double" ? 2 : 1;

            for (int i = 0; i < count; i++)
            {
                _ingredientUseCountLogic.AddIngredientUseCount(
                    content.IngredientId,
                    eventId);
            }
        }
        var scripts = new List<string>();

        foreach (var script in drink.DrinkScripts.OrderBy(s => s.Number))
        {
            var scriptName = script.UrScript.Trim();
            scripts.Add(scriptName);
        }
        _scriptRunner.QueueScripts(scripts);
    }

    public void RunMixSelvScripts(IEnumerable<(Guid IngredientId, int Cl)> ingredients)
    {
        if (ingredients == null)
            throw new ArgumentNullException(nameof(ingredients));

        var items = ingredients
            .Where(x => x.IngredientId != Guid.Empty)
            .ToList();

        if (items.Count == 0)
            throw new ArgumentException("No ingredients provided.", nameof(ingredients));

        // Fetch full ingredient data (including scripts)
        var ids = items.Select(i => i.IngredientId).Distinct().ToList();
        var ingredientsWithScripts = _ingredientLogic.GetIngredientsWithScripts(ids)
            .ToDictionary(i => i.IngredientId);

        var scripts = new List<string>();

        var eventId = _eventSession.CurrentEventId!.Value;

        foreach (var (ingredientId, cl) in items)
        {
            if (!ingredientsWithScripts.TryGetValue(ingredientId, out var ing))
                throw new ArgumentException($"Ingredient not found for id {ingredientId}.");

            // Dose selection rule:
            // 2cl => single
            // 4cl or more => double
            var useDouble = cl >= 4;

            // Use-count logic mirrors drink logic: double counts as 2 uses.
            var useCount = useDouble ? 2 : 1;
            for (var i = 0; i < useCount; i++)
                _ingredientUseCountLogic.AddIngredientUseCount(ingredientId, eventId);

            if (useDouble)
            {
                foreach (var script in ing.DoubleScripts.OrderBy(s => s.Number))
                    scripts.Add(script.UrScript);
            }
            else
            {
                foreach (var script in ing.SingleScripts.OrderBy(s => s.Number))
                    scripts.Add(script.UrScript);
            }
        }

        _scriptRunner.QueueScripts(scripts);
    }

        
    
}
    
