namespace RobotBarApp.Services.Robot.Interfaces;

public interface IRobotComms
{
    Task ConnectAsync();
    Task LoadProgramAsync(string programName);
    Task PlayAsync();
    Task StopAsync();
}