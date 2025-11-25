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

    public RobotLogMonitor(string robotIp, ILogLogic logLogic)
    {
        _robotIp = robotIp;
        _log = logLogic;
    }
    
    public async Task<bool> TryConnectWithTimeout(string ip, int port, int timeoutMs)
    {
        using var client = new TcpClient();

        var connectTask = client.ConnectAsync(ip, port);
        var timeoutTask = Task.Delay(timeoutMs);

        var finished = await Task.WhenAny(connectTask, timeoutTask);

        if (finished == timeoutTask)
            return false; // timeout

        return true; // connected
    }
    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        _client = new TcpClient();

        try
        {
            // gives 2000ms to connect, if not then log timeout and continue without connection
            bool connected = await TryConnectWithTimeout(_robotIp, DASHBOARD_PORT, 2000);

            if (!connected)
            {
                _log.AddLog("Robot did not respond (timeout). Starting without connection.", "RobotWarning");
                return; 
            }

            await _client.ConnectAsync(_robotIp, DASHBOARD_PORT); 
            _stream = _client.GetStream();

            _ = Task.Run(() => ListenLoop(_cts.Token));
            _log.AddLog("Connected to robot dashboard server.", "RobotInfo");
        }
        catch (Exception ex)
        {
            _log.AddLog($"Error while connecting to robot: {ex.Message}", "RobotWarning");
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
                if (read == 0) continue;

                string msg = Encoding.ASCII.GetString(buffer, 0, read).Trim();

                if (string.IsNullOrWhiteSpace(msg))
                    continue;

                ProcessMessage(msg);
            }
        }
        catch (Exception ex)
        {
            _log.AddLog($"Dashboard connection error: {ex.Message}", "RobotError");
        }
    }
    public event Action<string>? OnRobotMessage;
    public event Action<string>? OnRobotError;
    public event Action? ProgramFinished;

    private void ProcessMessage(string msg)
    {
        if (msg.Contains("Program finished"))
        {
            ProgramFinished?.Invoke();
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
