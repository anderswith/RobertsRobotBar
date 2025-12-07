using System.Net.Sockets;
using System.Text;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Robot.Interfaces;

public class RobotDashboardStreamReaderTest : IRobotDashboardStreamReader
{
    private readonly ILogLogic _log;

    public event Action<string>? OnRobotError;
    public event Action? ProgramFinished;
    public event Action<string>? OnRobotMessage;
    private readonly string _robotIp;

    private TcpClient? _primaryClient;     // 30001
    private TcpClient? _dashboardClient;   // 29999
    private TcpClient? _rtdeClient;        // 30004
    private readonly string _ip;

    private CancellationTokenSource? _cts;

    public RobotDashboardStreamReaderTest(string robotIp, ILogLogic log)
    {
        _robotIp = robotIp;
        _log = log;
    }

    public async Task StartAsync()
    {
        _cts = new CancellationTokenSource();
        
        // CONNECT PRIMARY (30001)
        try
        {
            _primaryClient = new TcpClient();
            var connectPrimary = _primaryClient.ConnectAsync(_robotIp, 30001);
            if (await Task.WhenAny(connectPrimary, Task.Delay(2000)) != connectPrimary)
            {
                _log.AddLog("Primary interface timeout — running without 30001", "RobotWarning");
            }
            else
            {
                Console.WriteLine("Connected to Primary Interface (30001)");
                _ = Task.Run(() => ListenPrimary(_cts.Token));
            }
        }
        catch (Exception ex)
        {
            _log.AddLog("Primary interface error: " + ex.Message, "RobotWarning");
        }


        // CONNECT DASHBOARD (29999)
        try
        {
            _dashboardClient = new TcpClient();
            var connectDash = _dashboardClient.ConnectAsync(_robotIp, 29999);
            if (await Task.WhenAny(connectDash, Task.Delay(2000)) != connectDash)
            {
                _log.AddLog("Dashboard timeout — running without 29999", "RobotWarning");
            }
            else
            {
                Console.WriteLine("Connected to Dashboard Server (29999)");
                _ = Task.Run(() => ListenDashboard(_cts.Token));
            }
        }
        catch (Exception ex)
        {
            _log.AddLog("Dashboard server error: " + ex.Message, "RobotWarning");
        }


        // CONNECT RTDE SAFETY (30004)
        try
        {
            _rtdeClient = new TcpClient();
            var connectRtde = _rtdeClient.ConnectAsync(_robotIp, 30004);
            if (await Task.WhenAny(connectRtde, Task.Delay(2000)) != connectRtde)
            {
                _log.AddLog("RTDE timeout — running without safety monitor", "RobotWarning");
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
    
    // PRIMARY INTERFACE (30001)
    private async Task ListenPrimary(CancellationToken token)
    {
        var stream = _primaryClient!.GetStream();
        byte[] buffer = new byte[4096];

        while (!token.IsCancellationRequested)
        {
            if (!stream.DataAvailable)
            {
                await Task.Delay(1, token);
                continue;
            }

            int bytes = await stream.ReadAsync(buffer, 0, buffer.Length, token);
            if (bytes == 0) continue;

            string data = Encoding.ASCII.GetString(buffer, 0, bytes);

            foreach (var line in data.Split('\n'))
            {
                string trimmed = line.Trim();
                if (trimmed.Length == 0) continue;

                if (trimmed.Contains("textmsg", StringComparison.OrdinalIgnoreCase))
                {
                    OnRobotMessage?.Invoke(trimmed);
                }

                if (trimmed.Contains("finished", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.Contains("stopped", StringComparison.OrdinalIgnoreCase))
                {
                    ProgramFinished?.Invoke();
                }
            }
        }
    }


    // DASHBOARD SERVER (29999)
    private async Task ListenDashboard(CancellationToken token)
    {
        var stream = _dashboardClient!.GetStream();
        byte[] buffer = new byte[4096];

        while (!token.IsCancellationRequested)
        {
            int bytes = await stream.ReadAsync(buffer, 0, buffer.Length, token);
            if (bytes == 0)
            {
                await Task.Delay(2, token);
                continue;
            }

            string msg = Encoding.ASCII.GetString(buffer, 0, bytes).Trim();
            if (msg.Length == 0) continue;

            Console.WriteLine("29999 << " + msg);
            
            if (msg.Contains("Protect", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("Fault", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("Emergency", StringComparison.OrdinalIgnoreCase) ||
                msg.Contains("Stop", StringComparison.OrdinalIgnoreCase))
            {
                _log.AddLog("Robot error: " + msg, "RobotError");
                OnRobotError?.Invoke(msg);
            }
        }
    }


    // RTDE SAFETY MONITOR (30004)
    private async Task ListenRtde(CancellationToken token)
    {
        var stream = _rtdeClient!.GetStream();
        byte[] buffer = new byte[8192];

        while (!token.IsCancellationRequested)
        {
            int count = await stream.ReadAsync(buffer, 0, buffer.Length, token);
            if (count < 7) continue;
            
            byte packageType = buffer[4];
            if (packageType != 2) continue; // state message only

            byte robotMode = buffer[5];
            byte safetyMode = buffer[6];


            if (safetyMode == 3 || robotMode == 7) // Protective Stop
                FireSafetyEvent("Protective Stop");

            if (safetyMode == 5 || robotMode == 8) // Emergency Stop
                FireSafetyEvent("Emergency Stop");

            if (safetyMode == 4) // Safeguard stop
                FireSafetyEvent("Safeguard Stop");

            if (robotMode == 9) // Fault
                FireSafetyEvent("Robot Fault");
        }
    }

    private void FireSafetyEvent(string msg)
    {
        _log.AddLog(msg, "RobotError");
        OnRobotError?.Invoke(msg);
    }
}
