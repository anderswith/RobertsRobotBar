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

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        _client = new TcpClient();

        await _client.ConnectAsync(_robotIp, DASHBOARD_PORT);
        _stream = _client.GetStream();

        // Dashboard server sends a welcome message immediately
        _ = Task.Run(() => ListenLoop(_cts.Token));
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

    private void ProcessMessage(string msg)
    {
        // These messages come straight from UR Dashboard Server
        if (msg.Contains("Protective stop") ||
            msg.Contains("emergency") ||
            msg.Contains("fault") ||
            msg.Contains("not ready") ||
            msg.Contains("Violation"))
        {
            _log.AddLog(msg, "RobotError");
        }
        else
        {
            _log.AddLog(msg, "RobotMessage");
        }
    }
}
