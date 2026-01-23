using System.IO;
using System.Net.Sockets;
using System.Text;
using RobotBarApp.Services.Robot.Interfaces;

public class RobotComms : IRobotComms
{
    private readonly string _ip;

    public RobotComms(string ip)
    {
        _ip = ip;
    }
    
    public Task LoadProgramAsync(string programName)
        => SendDashboardCommand($"load {programName}");

    public Task PlayAsync()
        => SendDashboardCommand("play");

    public Task StopAsync()
        => SendDashboardCommand("stop");

    private async Task SendDashboardCommand(string cmd)
    {
        // tcp implements IDisposable, automatically closes the connection
        using var client = new TcpClient();
        await client.ConnectAsync(_ip, 29999);

        var stream = client.GetStream();
        var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

        await writer.WriteLineAsync(cmd);
    }
}