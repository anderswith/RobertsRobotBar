namespace RobotBarApp.Services.Robot.Interfaces;

public interface IRobotScriptRunner
{
    void QueueScripts(IEnumerable<string> scripts);
    event Action? DrinkFinished;
    event Action<int, int>? ScriptFinished;
    event Action<int>? ScriptsStarted;
    
}