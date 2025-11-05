using System;
using System.Net.Sockets;
using System.Text;

public class RoboComms
{
    private readonly string robotIP;
    private const int DASHBOARD_PORT = 29999;
    private const int SECONDARY_PORT = 30002;

    public RoboComms(string robotIp)
    {
        robotIP = robotIp;
    }

    public void LoadAndRunProgram(string programName)
    {
        try
        {
            using (TcpClient client = new TcpClient())
            {
                client.ReceiveTimeout = 5000;
                client.SendTimeout = 5000;

                client.Connect(robotIP, DASHBOARD_PORT);
                using (NetworkStream stream = client.GetStream())
                {
                    byte[] buffer = new byte[1024];

                    // Read the initial connection message
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim());

                    // Load program
                    string loadCmd = $"load {programName}\n";
                    byte[] loadBytes = Encoding.UTF8.GetBytes(loadCmd);
                    stream.Write(loadBytes, 0, loadBytes.Length);

                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim());

                    // Play program
                    string playCmd = "play\n";
                    byte[] playBytes = Encoding.UTF8.GetBytes(playCmd);
                    stream.Write(playBytes, 0, playBytes.Length);

                    bytesRead = stream.Read(buffer, 0, buffer.Length);
                    Console.WriteLine(Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim());

                    Console.WriteLine($"{programName} loaded and running!");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Error] {ex.Message}");
        }
    }
}