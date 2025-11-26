using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using RobotBarApp.Services.Robot.Interfaces;

public class RobotComms : IRobotComms
{
    private readonly string _robotIp;
    private const int DASHBOARD_PORT = 29999;

    public RobotComms(string robotIp)
    {
        _robotIp = robotIp;
    }

    private async Task SendAsync(string command)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_robotIp, DASHBOARD_PORT);

        using var stream = client.GetStream();

        byte[] data = Encoding.ASCII.GetBytes(command + "\n");
        await stream.WriteAsync(data, 0, data.Length);
    }

    public Task LoadProgramAsync(string programName)
        => SendAsync($"load {programName}");

    public Task PlayAsync()
        => SendAsync("play");

    public Task StopAsync()
        => SendAsync("stop");
}