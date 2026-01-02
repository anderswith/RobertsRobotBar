using System.Net.Sockets;
using System.Text;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

public class RobotDashboardStreamReader : IRobotDashboardStreamReader
{
    private readonly ILogLogic _log;
    private readonly string _robotIp;

    public event Action<string>? OnRobotError;
    public event Action? ProgramFinished;
    public event Action? ConnectionFailed;
    public event Action<string>? OnRobotMessage;

    private TcpClient? _client;
    private CancellationTokenSource? _cts;
    

    public RobotDashboardStreamReader(string robotIp, ILogLogic log)
    {
        _robotIp = robotIp;
        _log = log;
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();

        try
        {
            _client = new TcpClient();

            var connectTask = _client.ConnectAsync(_robotIp, 30001);
            var timeoutTask = Task.Delay(2000);

            var result = await Task.WhenAny(connectTask, timeoutTask);

            if (result == timeoutTask)
            {

                _log.AddLog("Robot dashboard timeout — starting without connection.", "RobotWarning");
                ConnectionFailed?.Invoke();
                return;
            }

            await connectTask;
            _log.AddLog("Connected to Primary Interface (30001)", "RobotMessage");

            _ = Task.Run(() => ListenAsync(_cts.Token));
        }
        catch (Exception ex)
        {
            _log.AddLog($"Error connecting to robot: {ex.Message}", "RobotError");
        }
    }

    private async Task ListenAsync(CancellationToken token)
    {
        NetworkStream stream;

        try
        {
            stream = _client!.GetStream();
        }
        catch
        {
            return;
        }

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
                if (trimmed.Length == 0) 
                    continue;

                HandleMessage(trimmed);
            }
        }
    }

    private void HandleMessage(string trimmed)
    {
        if (trimmed.Contains("textmsg", StringComparison.OrdinalIgnoreCase))
        {
            OnRobotMessage?.Invoke(trimmed);
        }

        if (trimmed.Contains("protect", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("fault", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("emergency", StringComparison.OrdinalIgnoreCase))
        {
            _log.AddEventLog($"Error: {trimmed}", "RobotError");
            OnRobotError?.Invoke(trimmed);
        }

        if (trimmed.Contains("finished", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Contains("stopped", StringComparison.OrdinalIgnoreCase))
        {
            ProgramFinished?.Invoke();
        }
    }
}
