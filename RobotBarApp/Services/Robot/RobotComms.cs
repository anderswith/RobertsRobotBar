using System.IO;
using System.Net.Sockets;
using System.Text;
using RobotBarApp.Services.Robot.Interfaces;

public class RobotComms : IRobotComms
{
    private readonly IRobotDashboardStreamReader _reader;
    private readonly string _ip;

    public RobotComms(string ip, IRobotDashboardStreamReader reader)
    {
        _ip = ip;
        _reader = reader;
    }

    public async Task ConnectAsync()
    {
        // Start Primary Interface Reader (30001)
        await _reader.StartAsync(_ip);

        Console.WriteLine("RobotComms connected (reader running on 30001)");
    }

    public Task LoadProgramAsync(string programName)
        => SendDashboardCommand($"load {programName}");

    public Task PlayAsync()
        => SendDashboardCommand("play");

    public Task StopAsync()
        => SendDashboardCommand("stop");

    private async Task SendDashboardCommand(string cmd)
    {
        using var client = new TcpClient();
        await client.ConnectAsync(_ip, 29999);

        var stream = client.GetStream();
        var writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

        await writer.WriteLineAsync(cmd);

        Console.WriteLine($">> {cmd}");
    }
}