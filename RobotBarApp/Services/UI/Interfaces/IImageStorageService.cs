namespace RobotBarApp.Services.Interfaces;

public interface IImageStorageService
{
    string SaveImage(
        string sourceFilePath,
        string categoryFolder,
        string? baseName = null);
}