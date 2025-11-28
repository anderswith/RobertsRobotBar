using System.IO;
using System.Net.Sockets;
using System.Text;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

public class RobotDashboardStreamReader : IRobotDashboardStreamReader
{
    private readonly ILogLogic _log;

    public event Action<string>? OnRobotError;
    public event Action? ProgramFinished;
    public event Action<string>? OnRobotMessage;

    private TcpClient? _client;
    private CancellationTokenSource? _cts;

    public RobotDashboardStreamReader(ILogLogic log)
    {
        _log = log;
    }
    
    public async Task StartAsync(string robotIp)
    {
        _cts = new CancellationTokenSource();

        try
        {
            _client = new TcpClient();
            
            var connectTask = _client.ConnectAsync(robotIp, 30001);
            var timeoutTask = Task.Delay(2000);

            var result = await Task.WhenAny(connectTask, timeoutTask);

            if (result == timeoutTask)
            {
                Console.WriteLine("Robot dashboard timeout — starting without connection.");
                _log.AddLog("Robot dashboard timeout — starting without connection.", "RobotWarning");
                return;
            }

            await connectTask;
            Console.WriteLine("Connected to Primary Interface (30001)");
            _ = Task.Run(() => Listen(_cts.Token));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error connecting to robot: {ex.Message}");
            _log.AddLog($"Error connecting to robot: {ex.Message}", "RobotWarning");
        }
    }

    private async Task Listen(CancellationToken token)
    {
        var stream = _client!.GetStream();
        byte[] buffer = new byte[4096];

        while (!token.IsCancellationRequested)
        {
            if (!stream.DataAvailable)
            {
                await Task.Delay(1);
                continue;
            }

            int bytes = await stream.ReadAsync(buffer, 0, buffer.Length, token);
            if (bytes == 0)
                continue;

            string data = Encoding.ASCII.GetString(buffer, 0, bytes);

            foreach (var line in data.Split('\n'))
            {
                var trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                //Console.WriteLine("30001 << " + trimmed);

                if (trimmed.Contains("textmsg", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("30001 << " + trimmed);
                    OnRobotMessage?.Invoke(trimmed);
                }

                if (trimmed.Contains("protect", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("fault", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("emergency", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("30002 << " + trimmed);
                    _log.AddLog($"Error: {trimmed}", "RobotError");
                    OnRobotError?.Invoke(trimmed);
                    
                }

                if (trimmed.Contains("finished", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("stopped", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("30002 << " + trimmed);
                    ProgramFinished?.Invoke();
                }
            }
        }
    }
}
