using System;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

public class RoboComms
{
    private readonly string _robotIp;
    private const int DASHBOARD_PORT = 29999;

    public RoboComms(string robotIp)
    {
        _robotIp = robotIp;
    }

    public async Task<string> SendDashboardCommand(string command)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_robotIp, DASHBOARD_PORT);

        using var stream = client.GetStream();

        byte[] cmdBytes = Encoding.ASCII.GetBytes(command + "\n");
        await stream.WriteAsync(cmdBytes, 0, cmdBytes.Length);

        var buffer = new byte[4096];
        int read = await stream.ReadAsync(buffer, 0, buffer.Length);
        if (read <= 0) return string.Empty;

        return Encoding.ASCII.GetString(buffer, 0, read).Trim();
    }

    public Task<string> LoadProgramAsync(string programName)
        => SendDashboardCommand($"load {programName}");

    public Task<string> PlayAsync()
        => SendDashboardCommand("play");

    public Task<string> StopAsync()
        => SendDashboardCommand("stop");

    public Task<string> GetProgramStateAsync()
        => SendDashboardCommand("programState");
}