using RobotBarApp.BLL.Interfaces;

namespace RobotBarApp.Services.Robot.Interfaces;

public interface IRobotDashboardStreamReader
{


    event Action? ProgramFinished;

    event Action? ConnectionFailed;

    Task StartAsync();
}