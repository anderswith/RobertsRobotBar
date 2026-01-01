namespace RobotBarApp.BLL.Interfaces;

public interface IRobotLogic
{
    void RunRobotScripts(IEnumerable<string> scripts);
    void RunMixSelvScripts(List<(Guid IngredientId, int Cl)> order);
    void RunDrinkScripts(Guid drinkId);

    /// <summary>
    /// Runs scripts for a MixSelv order. For each ingredient, RobotLogic decides whether to use
    /// single or double scripts based on the cl amount (2cl => single, 4cl+ => double).
    /// The order of items is preserved.
    /// </summary>
    void RunMixSelvScripts(IEnumerable<(Guid IngredientId, int Cl)> ingredients);

    event Action DrinkFinished;

    event Action<int, int>? ScriptFinished;
    event Action<int>? ScriptsStarted;
}