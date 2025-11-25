using System.Collections.Concurrent;

public class RobotScriptRunner
{
    private readonly RoboComms _comms;
    private readonly RobotLogMonitor _monitor;

    private readonly ConcurrentQueue<string> _queue = new();
    private bool _isRunning = false;

    public RobotScriptRunner(RoboComms comms, RobotLogMonitor monitor)
    {
        _comms = comms;
        _monitor = monitor;

        _monitor.OnRobotMessage += HandleRobotMessage;
    }

    public void EnqueueScripts(IEnumerable<string> scripts)
    {
        foreach (var s in scripts)
            _queue.Enqueue(s);

        TryRunNext();
    }

    private async void TryRunNext()
    {
        if (_isRunning) return;
        if (!_queue.TryDequeue(out var script)) return;

        _isRunning = true;

        await _comms.LoadProgram(script);
        await Task.Delay(200); // small delay to ensure load completed
        await _comms.Play();
    }

    private void HandleRobotMessage(string msg)
    {
        if (msg.Contains("Program finished") ||
            msg.Contains("Stopped") ||
            msg.Contains("not running"))
        {
            _isRunning = false;
            TryRunNext();
        }
    }
}