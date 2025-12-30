namespace RobotBarApp.BLL.Interfaces;

public interface IRobotLogic
{
    void RunRobotScripts(IEnumerable<string> scripts);
    void RunIngredientScript(List<Guid> ingredientIds);
    void RunDrinkScripts(Guid drinkId);
    event Action DrinkFinished;
}