using RobotBarApp.BLL.Interfaces;

namespace RobotBarApp.Services.Robot.Interfaces;

public interface IRobotDashboardStreamReader
{

    event Action<string>? OnRobotError;
    event Action? ProgramFinished;
    event Action<string>? OnRobotMessage;

    Task StartAsync();
}