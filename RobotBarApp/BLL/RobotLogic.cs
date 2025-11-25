namespace RobotBarApp.BLL;

public class RobotLogic
{
    private readonly RobotScriptRunner _scriptRunner;
    
    public RobotLogic(RobotScriptRunner scriptRunner)
    {
        _scriptRunner = scriptRunner;
    }
    
    public void RunRobotScripts(IEnumerable<string> scripts)
    {
        _scriptRunner.EnqueueScripts(scripts);
    }
    
}