using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using RobotBarApp.BLL.Interfaces;

public class RobotScriptRunner
{
    private readonly RoboComms _robo;
    private readonly ILogLogic _log;

    private readonly ConcurrentQueue<string> _queue = new();
    private readonly object _lock = new();
    private bool _workerRunning;

    public RobotScriptRunner(RoboComms roboComms, ILogLogic logLogic)
    {
        _robo = roboComms;
        _log = logLogic;
    }

    public void EnqueueScript(string scriptName)
    {
        if (!string.IsNullOrWhiteSpace(scriptName))
        {
            _queue.Enqueue(scriptName);
            _log.AddLog($"Queued script '{scriptName}'", "RobotInfo");
            StartWorkerIfNeeded();
        }
    }

    public void EnqueueScripts(IEnumerable<string> scriptNames)
    {
        foreach (var script in scriptNames)
        {
            if (!string.IsNullOrWhiteSpace(script))
            {
                _queue.Enqueue(script);
                _log.AddLog($"Queued script '{script}'", "RobotInfo");
            }
        }

        StartWorkerIfNeeded();
    }

    private void StartWorkerIfNeeded()
    {
        lock (_lock)
        {
            if (_workerRunning) return;
            _workerRunning = true;
            _ = Task.Run(RunQueueAsync);
        }
    }

    private async Task RunQueueAsync()
    {
        try
        {
            while (_queue.TryDequeue(out var scriptName))
            {
                await RunSingleScript(scriptName);
            }
        }
        finally
        {
            lock (_lock)
            {
                _workerRunning = false;
            }
        }
    }

    private async Task RunSingleScript(string scriptName)
    {
        try
        {
            _log.AddLog($"Running robot script '{scriptName}'", "RobotInfo");

            await _robo.LoadProgramAsync(scriptName);
            await _robo.PlayAsync();

            await WaitForProgramToFinish(scriptName);

            _log.AddLog($"Script '{scriptName}' completed.", "RobotInfo");
        }
        catch (Exception ex)
        {
            _log.AddLog($"Error while executing '{scriptName}': {ex.Message}", "RobotError");
        }
    }

    private async Task WaitForProgramToFinish(string scriptName)
    {
        while (true)
        {
            await Task.Delay(500);

            string state;

            try
            {
                state = await _robo.GetProgramStateAsync();
            }
            catch
            {
                _log.AddLog($"Lost program state while waiting for '{scriptName}'.", "RobotError");
                return;
            }

            string lower = state.ToLower();

            if (lower.Contains("running") || lower.Contains("playing"))
                continue;

            // STOPPED, IDLE, READY, etc.
            return;
        }
    }
}
