namespace RobotBarApp.Services.Robot.Interfaces;

public interface IRobotComms
{
    Task LoadProgramAsync(string programName);
    Task PlayAsync();
    Task StopAsync();
}