using System.Net.Sockets;
using System.Text;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

public class AllportReaderTest 
{
    private readonly ILogLogic _log;

    public event Action<string>? OnRobotError;
    public event Action? ProgramFinished;
    public event Action<string>? OnRobotMessage;

    private CancellationTokenSource? _cts;

    // MULTI-PORT CLIENTS
    private readonly Dictionary<int, TcpClient> _clients = new();
    private readonly int[] _ports = { 29999, 30001, 30002, 30003 };

    public AllportReaderTest(ILogLogic log)
    {
        _log = log;
    }

    public async Task StartAsync(string robotIp)
    {
        _cts = new CancellationTokenSource();

        foreach (var port in _ports)
        {
            try
            {
                var client = new TcpClient();
                var connectTask = client.ConnectAsync(robotIp, port);
                var timeout = Task.Delay(2000);

                if (await Task.WhenAny(connectTask, timeout) != connectTask)
                {
                    _log.AddLog($"Timeout connecting to port {port}.", "RobotWarning");
                    continue;
                }

                await connectTask;
                _clients[port] = client;

                Console.WriteLine($"Connected to robot port {port}");

                _ = Task.Run(() => ListenToPort(port, client, _cts.Token));
            }
            catch (Exception ex)
            {
                _log.AddLog($"Error connecting to port {port}: {ex.Message}", "RobotWarning");
            }
        }
    }

    private async Task ListenToPort(int port, TcpClient client, CancellationToken token)
    {
        var stream = client.GetStream();
        byte[] buffer = new byte[4096];

        while (!token.IsCancellationRequested)
        {
            try
            {
                int bytes = await stream.ReadAsync(buffer, 0, buffer.Length, token);
                if (bytes <= 0)
                    continue;

                string raw = Encoding.ASCII.GetString(buffer, 0, bytes);
                string[] lines = raw.Split('\n');

                foreach (var line in lines)
                {
                    var trimmed = line.Trim();
                    if (trimmed.Length == 0) continue;

                    Console.WriteLine($"{port} << {trimmed}");

                    HandlePortMessage(port, trimmed);
                }
            }
            catch (Exception)
            {
                await Task.Delay(50);
            }
        }
    }

    private void HandlePortMessage(int port, string msg)
    {
        // BASIC TEXTMSG
        if (msg.Contains("textmsg", StringComparison.OrdinalIgnoreCase))
            OnRobotMessage?.Invoke(msg);

        // PROGRAM FINISHED (ONLY FROM 30001 & 30002)
        if (msg.Contains("finished", StringComparison.OrdinalIgnoreCase) ||
            msg.Contains("stopped", StringComparison.OrdinalIgnoreCase))
        {
            ProgramFinished?.Invoke();
        }

        // ERROR DETECTION — fallback for ANY of the ports
        if (ContainsErrorKeyword(msg))
        {
            _log.AddLog($"Robot error ({port}): {msg}", "RobotError");
            OnRobotError?.Invoke(msg);
        }
    }

    private bool ContainsErrorKeyword(string msg)
    {
        string[] keywords =
        {
            "protect",
            "fault",
            "emergency",
            "stop",
            "violation",
            "safeguard",
            "error",
            "runtime",
            "system",
            "collision" 
        };

        return keywords.Any(k =>
            msg.Contains(k, StringComparison.OrdinalIgnoreCase));
    }
}
