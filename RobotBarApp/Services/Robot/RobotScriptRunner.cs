using System.Collections.Concurrent;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

public class RobotScriptRunner : IRobotScriptRunner
{
    private readonly IRobotComms _comms;
    private readonly ILogLogic _log;
    private readonly IRobotDashboardStreamReader _reader;
    private readonly ConcurrentQueue<string> _queue = new();
    private int _finishedScriptCount;
    private int _totalScriptCount;
    public event Action? DrinkFinished;
    public event Action<int, int>? ScriptFinished;
    public event Action<int>? ScriptsStarted;

    private bool _isRunning = false;
    private readonly object _lock = new();

    public RobotScriptRunner(IRobotComms comms, IRobotDashboardStreamReader reader, ILogLogic logLogic)
    {
        _comms = comms;
        _reader = reader;
        _log = logLogic;
        
        _reader.ProgramFinished += OnProgramFinished;
    }

    public void QueueScripts(IEnumerable<string> scripts)
    {
        var scriptList = scripts
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .ToList();
        _totalScriptCount = scriptList.Count;
        _finishedScriptCount = 0;
        ScriptsStarted?.Invoke(_totalScriptCount);

        TriggerScriptFinished(scriptList);
        
        foreach (var s in scripts)
            if (!string.IsNullOrWhiteSpace(s))
                _queue.Enqueue(s);

        TryStartNext();
    }

    public void TriggerScriptFinished(IEnumerable<string> scripts)
    {
        _totalScriptCount = scripts.Count();
        _finishedScriptCount = 0;
    }

    private void TryStartNext()
    {
        lock (_lock)
        {
            if (_isRunning) return;
            if (!_queue.TryDequeue(out var next)) return;

            _isRunning = true;
            _ = RunScript(next);
        }
    }

    private async Task RunScript(string scriptName)
    {
        try
        {
            _log.AddLog($"Loading script: {scriptName}", "RobotInfo");
            await _comms.LoadProgramAsync(scriptName);

            _log.AddLog($"Playing script: {scriptName}", "RobotInfo");
            await _comms.PlayAsync();
        }
        catch (Exception ex)
        {
            _log.AddLog($"Script run error: {ex.Message}", "RobotError");

            lock (_lock)
            {
                _isRunning = false;
            }

            TryStartNext();
        }
    }

    private void OnProgramFinished()
    {
        lock (_lock)
        {
            _isRunning = false;
        }
        _finishedScriptCount++;
        
        ScriptFinished?.Invoke(_finishedScriptCount, _totalScriptCount);

        Console.WriteLine(
            $"Script færdig ({_finishedScriptCount}/{_totalScriptCount})");

        if (_finishedScriptCount == _totalScriptCount)
        {
            Console.WriteLine("alle scripts er færdige");
            DrinkFinished?.Invoke();
        }

        TryStartNext();
    }
}