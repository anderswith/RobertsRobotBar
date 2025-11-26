namespace RobotBarApp.Services.Robot.Interfaces;

public interface IRobotDashboardStreamReader
{

    event Action ProgramFinished;

    event Action<string> OnRobotMessage;

    event Action<string> OnRobotError;

    Task StartAsync();
}