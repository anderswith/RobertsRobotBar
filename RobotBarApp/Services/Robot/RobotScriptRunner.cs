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

    private bool _isRunning = false;
    private readonly object _lock = new();

    public event Action? ScriptFinished;
    public event Action? DrinkFinished;

    public RobotScriptRunner(
        IRobotComms comms,
        IRobotDashboardStreamReader reader,
        ILogLogic logLogic)
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

        _queue.Clear();
        _totalScriptCount = scriptList.Count;
        _finishedScriptCount = 0;

        foreach (var s in scriptList)
            _queue.Enqueue(s);

        TryStartNext();
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

        // 🔹 one script completed
        ScriptFinished?.Invoke();

        Console.WriteLine(
            $"Script færdig ({_finishedScriptCount}/{_totalScriptCount})");

        // 🔹 all scripts completed
        if (_finishedScriptCount >= _totalScriptCount)
        {
            Console.WriteLine("Alle scripts er færdige");
            DrinkFinished?.Invoke();
            return;
        }

        TryStartNext();
    }
}
