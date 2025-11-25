using System.Net.Sockets;
using System.Text;
using RobotBarApp.BLL.Interfaces;

public class RobotLogMonitor
{
    private readonly string _robotIp;
    private readonly ILogLogic _log;

    private TcpClient? _client;
    private NetworkStream? _stream;
    private CancellationTokenSource? _cts;

    private const int DASHBOARD_PORT = 29999;

    public event Action? ProgramFinished;  //  <-- Script finished event
    public event Action<string>? OnRobotMessage;
    public event Action<string>? OnRobotError;

    public RobotLogMonitor(string robotIp, ILogLogic logLogic)
    {
        _robotIp = robotIp;
        _log = logLogic;
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();

        try
        {
            _client = new TcpClient();
            var connectTask = _client.ConnectAsync(_robotIp, DASHBOARD_PORT);
            var timeoutTask = Task.Delay(2000);

            var result = await Task.WhenAny(connectTask, timeoutTask);
            if (result == timeoutTask)
            {
                _log.AddLog("Robot dashboard timeout — starting without connection.", "RobotWarning");
                return;
            }

            await connectTask;

            _stream = _client.GetStream();
            _log.AddLog("Connected to robot dashboard.", "RobotInfo");

            _ = Task.Run(() => ListenLoop(_cts.Token));
        }
        catch (Exception ex)
        {
            _log.AddLog($"Error connecting to robot: {ex.Message}", "RobotWarning");
        }
    }

    private async Task ListenLoop(CancellationToken token)
    {
        byte[] buffer = new byte[4096];

        try
        {
            while (!token.IsCancellationRequested)
            {
                int read = await _stream!.ReadAsync(buffer, 0, buffer.Length, token);
                if (read == 0)
                    continue;

                string msg = Encoding.ASCII.GetString(buffer, 0, read).Trim();
                if (string.IsNullOrWhiteSpace(msg))
                    continue;

                Console.WriteLine($"[ROBOT STREAM] {msg}");

                ProcessMessage(msg);
            }
        }
        catch (Exception ex)
        {
            _log.AddLog($"Dashboard connection lost: {ex.Message}", "RobotError");
        }
    }

    private void ProcessMessage(string msg)
    {
        if (msg.Contains("Program finished"))
        {
            ProgramFinished?.Invoke();
            _log.AddLog(msg, "RobotInfo");
            return;
        }

        if (msg.Contains("Protective stop") ||
            msg.Contains("emergency") ||
            msg.Contains("fault"))
        {
            _log.AddLog(msg, "RobotError");
            OnRobotError?.Invoke(msg);
            return;
        }

        _log.AddLog(msg, "RobotMessage");
        OnRobotMessage?.Invoke(msg);
    }
}
