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
    private readonly IRobotDashboardStreamReader _robotDashboardStreamReader;
    
    public event Action? DrinkFinished;
    public event Action<int, int>? ScriptFinished;
    public event Action<int>? ScriptsStarted;
    public event Action? ConnectionFailed;
    private bool _connectionFailed;
    public bool ConnectionFailedAlready => _connectionFailed;
    
    public RobotLogic(IRobotScriptRunner scriptRunner,
        IRobotDashboardStreamReader dashboardReader, 
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
        dashboardReader.ConnectionFailed += () =>
        {
            _connectionFailed = true;
            Console.WriteLine("Connection Failed Received in RobotLogic");
            ConnectionFailed?.Invoke();
        };
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
    
    
}
    
