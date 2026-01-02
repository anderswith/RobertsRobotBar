namespace RobotBarApp.BLL.Interfaces;

public interface IRobotLogic
{
    void RunRobotScripts(IEnumerable<string> scripts);
    void RunMixSelvScripts(List<(Guid IngredientId, int Cl)> order);
    void RunDrinkScripts(Guid drinkId);

    event Action DrinkFinished;

    event Action<int, int>? ScriptFinished;
    event Action<int>? ScriptsStarted;
    event Action? ConnectionFailed;
    bool ConnectionFailedAlready { get; }
}