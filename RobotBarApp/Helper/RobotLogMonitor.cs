using System.Net.Sockets;
using System.Text;
using System.Diagnostics;
using RobotBarApp.BLL.Interfaces;

namespace RobotBarApp.Helper
{
    public class RobotLogMonitor
    {
        private readonly string _robotIp;
        private readonly ILogLogic _log;
        private TcpClient? _client;
        private NetworkStream? _stream;
        private CancellationTokenSource? _cts;

        private const int DashboardPort = 29999;

        public RobotLogMonitor(string robotIp, ILogLogic logLogic)
        {
            _robotIp = robotIp;
            _log = logLogic;
        }

        public async Task StartAsync()
        {
            _cts = new CancellationTokenSource();
            _client = new TcpClient();

            try
            {
                await _client.ConnectAsync(_robotIp, DashboardPort);
            }
            catch (Exception ex)
            {
                // Log the failure and return; do not let the exception bubble to the UI thread.
                try
                {
                    _log.AddLog($"Failed to connect to robot dashboard {_robotIp}:{DashboardPort} - {ex.Message}", "RobotError");
                }
                catch (Exception logEx)
                {
                    // If logging fails, write to debug output so it's not completely suppressed.
                    Debug.WriteLine($"Failed to log robot connection failure: {logEx.Message}");
                }

                return;
            }

            // If connected, get the stream and start listening
            try
            {
                _stream = _client.GetStream();
                // Dashboard server sends a welcome message immediately
                _ = Task.Run(() => ListenLoop(_cts.Token));
            }
            catch (Exception ex)
            {
                try
                {
                    _log.AddLog($"Failed to start dashboard listen loop: {ex.Message}", "RobotError");
                }
                catch (Exception logEx)
                {
                    Debug.WriteLine($"Failed to log dashboard listen loop error: {logEx.Message}");
                }
            }
        }

        private async Task ListenLoop(CancellationToken token)
        {
            byte[] buffer = new byte[4096];

            try
            {
                while (!token.IsCancellationRequested)
                {
                    int read = await _stream!.ReadAsync(buffer, 0, buffer.Length, token);
                    if (read == 0) continue;

                    string msg = Encoding.ASCII.GetString(buffer, 0, read).Trim();

                    if (string.IsNullOrWhiteSpace(msg))
                        continue;

                    ProcessMessage(msg);
                }
            }
            catch (Exception ex)
            {
                try
                {
                    _log.AddLog($"Dashboard connection error: {ex.Message}", "RobotError");
                }
                catch (Exception logEx)
                {
                    Debug.WriteLine($"Failed to log dashboard connection error: {logEx.Message}");
                }
            }
        }

        private void ProcessMessage(string msg)
        {
            // These messages come straight from UR Dashboard Server
            if (msg.Contains("Protective stop") ||
                msg.Contains("emergency") ||
                msg.Contains("fault") ||
                msg.Contains("not ready") ||
                msg.Contains("Violation"))
            {
                _log.AddLog(msg, "RobotError");
            }
            else
            {
                _log.AddLog(msg, "RobotMessage");
            }
        }
    }
}
