namespace RobotBarApp.BLL.Interfaces;

public interface IRobotLogic
{
    void RunRobotScripts(IEnumerable<string> scripts);
    int RunMixSelvScripts(List<(Guid IngredientId, int Cl)> order);
    void RunDrinkScripts(Guid drinkId);

    event Action DrinkFinished;

    event Action? ScriptFinished;
    event Action? ConnectionFailed;
    bool ConnectionFailedAlready { get; }
}