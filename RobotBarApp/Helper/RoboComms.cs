using System.Net.Sockets;
using System.Text;

public class RoboComms
{
    private readonly string _robotIp;
    private TcpClient? _client;
    private NetworkStream? _stream;

    private const int DASHBOARD_PORT = 29999;

    public RoboComms(string robotIp)
    {
        _robotIp = robotIp;
    }

    public async Task<bool> ConnectAsync()
    {
        try
        {
            _client = new TcpClient();
            await _client.ConnectAsync(_robotIp, DASHBOARD_PORT);
            _stream = _client.GetStream();
            return true;
        }
        catch
        {
            return false;
        }
    }

    public async Task SendCommand(string cmd)
    {
        if (_stream == null) return;

        byte[] data = Encoding.ASCII.GetBytes(cmd + "\n");
        await _stream.WriteAsync(data, 0, data.Length);
    }

    public async Task LoadProgram(string program)
    {
        await SendCommand($"load {program}");
    }

    public async Task Play()
    {
        await SendCommand("play");
    }

    public async Task Stop()
    {
        await SendCommand("stop");
    }
}
