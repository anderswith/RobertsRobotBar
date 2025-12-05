using System.Net.Sockets;
using System.Text;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

public class AllPortsAndRtdeTest
{
    private readonly ILogLogic _log;

    public event Action<string>? OnRobotError;
    public event Action? ProgramFinished;
    public event Action<string>? OnRobotMessage;

    private CancellationTokenSource? _cts;

    // TEXT STREAM PORTS
    private readonly int[] _textPorts = { 29999, 30001, 30002, 30003 };
    private readonly Dictionary<int, TcpClient> _clients = new();

    // RTDE PORT 30004
    private TcpClient? _rtdeClient;

    public AllPortsAndRtdeTest(ILogLogic log)
    {
        _log = log;
    }

    public async Task StartAsync(string robotIp)
    {
        _cts = new CancellationTokenSource();
        
        foreach (var port in _textPorts)
        {
            try
            {
                var client = new TcpClient();
                var connectTask = client.ConnectAsync(robotIp, port);
                if (await Task.WhenAny(connectTask, Task.Delay(2000)) != connectTask)
                {
                    _log.AddLog($"Timeout connecting to port {port}", "RobotWarning");
                    continue;
                }

                _clients[port] = client;
                Console.WriteLine($"Connected to port {port}");

                // Start text stream reader
                _ = Task.Run(() => ListenToTextPort(port, client, _cts.Token));
            }
            catch (Exception ex)
            {
                _log.AddLog($"Error connecting to port {port}: {ex.Message}", "RobotWarning");
            }
        }

        // CONNECT RTDE (30004)
        try
        {
            _rtdeClient = new TcpClient();
            var rtdeTask = _rtdeClient.ConnectAsync(robotIp, 30004);
            if (await Task.WhenAny(rtdeTask, Task.Delay(2000)) != rtdeTask)
            {
                _log.AddLog("RTDE timeout — safety mode unavailable", "RobotWarning");
            }
            else
            {
                Console.WriteLine("Connected to RTDE (30004)");
                _ = Task.Run(() => ListenRtde(_cts.Token));
            }
        }
        catch (Exception ex)
        {
            _log.AddLog("RTDE error: " + ex.Message, "RobotWarning");
        }
    }


    // TEXT STREAM LISTENER (29999–30003)
    private async Task ListenToTextPort(int port, TcpClient client, CancellationToken token)
    {
        var stream = client.GetStream();
        var buffer = new byte[4096];

        while (!token.IsCancellationRequested)
        {
            int bytes = 0;
            try
            {
                bytes = await stream.ReadAsync(buffer, 0, buffer.Length, token);
            }
            catch
            {
                await Task.Delay(10);
                continue;
            }

            if (bytes <= 0)
                continue;

            string raw = Encoding.ASCII.GetString(buffer, 0, bytes);
            string[] lines = raw.Split('\n');

            foreach (var line in lines)
            {
                var msg = line.Trim();
                if (msg.Length == 0) continue;

                Console.WriteLine($"{port} << {msg}");

                HandleTextMessage(port, msg);
            }
        }
    }

    private void HandleTextMessage(int port, string msg)
    {
        // 1) textmsg()
        if (msg.Contains("textmsg", StringComparison.OrdinalIgnoreCase))
        {
            OnRobotMessage?.Invoke(msg);
        }

        // 2) Script finished
        if (msg.Contains("finished", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("stopped", StringComparison.OrdinalIgnoreCase))
        {
            ProgramFinished?.Invoke();
        }

        // 3) Error keywords (best effort)
        if (ContainsErrorKeyword(msg))
        {
            ForwardError($"[{port}] {msg}");
        }
    }

    private bool ContainsErrorKeyword(string msg)
    {
        string[] keywords =
        {
            "protect",
            "fault",
            "emergency",
            "violation",
            "safeguard",
            "error",
            "stop",
            "collision",
            "runtime",
            "system"
        };

        return keywords.Any(k =>
            msg.Contains(k, StringComparison.OrdinalIgnoreCase));
    }

    // RTDE SAFETY LISTENER (30004)
    private async Task ListenRtde(CancellationToken token)
    {
        var stream = _rtdeClient!.GetStream();
        var buffer = new byte[8192];

        while (!token.IsCancellationRequested)
        {
            int count = 0;
            try
            {
                count = await stream.ReadAsync(buffer, 0, buffer.Length, token);
            }
            catch
            {
                await Task.Delay(10);
                continue;
            }

            if (count < 7)
                continue;

            byte packageType = buffer[4];
            if (packageType != 2) continue; // State package only

            byte robotMode = buffer[5];
            byte safetyMode = buffer[6];

            // Interpret UR safety modes
            if (safetyMode == 3 || robotMode == 7)
                ForwardError("Protective Stop");

            if (safetyMode == 5 || robotMode == 8)
                ForwardError("Emergency Stop");

            if (safetyMode == 4)
                ForwardError("Safeguard Stop");

            if (robotMode == 9)
                ForwardError("Robot Fault");
        }
    }

    private void ForwardError(string msg)
    {
        _log.AddLog(msg, "RobotError");
        OnRobotError?.Invoke(msg);
    }
}
