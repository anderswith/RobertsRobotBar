using System.Collections.Concurrent;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

public class RobotScriptRunner : IRobotScriptRunner
{
    private readonly RobotComms _comms;
    private readonly ILogLogic _log;
    private readonly ConcurrentQueue<string> _queue = new();

    private bool _isRunning = false;
    private readonly object _lock = new();

    public RobotScriptRunner(RobotComms comms, RobotDashboardStreamReader monitor, ILogLogic logLogic)
    {
        _comms = comms;
        _log = logLogic;

        monitor.ProgramFinished += OnScriptFinished;   // <--- KEY PART
    }

    public void QueueScripts(IEnumerable<string> scripts)
    {
        foreach (var s in scripts)
            if (!string.IsNullOrWhiteSpace(s))
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

    private void OnScriptFinished()
    {
        lock (_lock)
        {
            _isRunning = false;
        }

        TryStartNext();
    }
}