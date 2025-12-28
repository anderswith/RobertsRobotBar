using System;
using System.IO;
using System.Linq;
using System.Windows;
using RobotBarApp.Services.Interfaces;

public class ImageStorageService : IImageStorageService
{
    public string SaveImage(
        string sourcePath,
        string categoryFolder,
        string? baseName)
    {
        
        if (string.IsNullOrWhiteSpace(sourcePath))
            throw new ArgumentException("Image path cannot be empty.", nameof(sourcePath));

        if (!File.Exists(sourcePath))
            throw new FileNotFoundException("Image file not found.", sourcePath);

        // Resolve project root (same logic you already use)
        var projectRoot = Directory
            .GetParent(AppContext.BaseDirectory)!
            .Parent!.Parent!.Parent!.FullName;

        var destinationFolder =
            Path.Combine(projectRoot, "Resources", categoryFolder);

        Directory.CreateDirectory(destinationFolder);

        var extension = Path.GetExtension(sourcePath);

        var safeName = new string(
            (baseName ?? "image")
            .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch)
            .ToArray());

        var fileName =
            $"{safeName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";

        var destinationPath =
            Path.Combine(destinationFolder, fileName);

        // This is the behavior you explicitly want
        File.Copy(sourcePath, destinationPath, overwrite: true);

        return Path
            .Combine("Resources", categoryFolder, fileName)
            .Replace("\\", "/");
    }
}