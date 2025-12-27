using System;
using System.IO;

namespace RobotBarApp.Services.UI
{
    /// <summary>
    /// Copies user-chosen drink images into Resources/DrinkPics and returns the relative path
    /// that should be stored in the database (e.g. "Resources/DrinkPics/mydrink_20251227.png").
    /// </summary>
    public static class DrinkImageStorage
    {
        public static string SaveToDrinkPics(string sourceFilePath)
        {
            if (string.IsNullOrWhiteSpace(sourceFilePath))
                throw new ArgumentException("Image path cannot be empty.", nameof(sourceFilePath));

            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("Image file not found.", sourceFilePath);

            var ext = Path.GetExtension(sourceFilePath);
            if (string.IsNullOrWhiteSpace(ext))
                ext = ".png";

            // Locate project root from runtime base dir.
            // AppContext.BaseDirectory -> bin/Debug/.../netX
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
                throw new InvalidOperationException("Could not resolve project root directory.");

            var targetDir = Path.Combine(projectRoot, "Resources", "DrinkPics");
            Directory.CreateDirectory(targetDir);

            var fileNameNoExt = Path.GetFileNameWithoutExtension(sourceFilePath);
            fileNameNoExt = SanitizeFileName(fileNameNoExt);

            var uniqueName = $"{fileNameNoExt}_{DateTime.Now:yyyyMMddHHmmssfff}{ext}";
            var destFullPath = Path.Combine(targetDir, uniqueName);

            File.Copy(sourceFilePath, destFullPath, overwrite: false);

            // Return a relative path compatible with PathToImageConverter.
            return Path.Combine("Resources", "DrinkPics", uniqueName).Replace('\\', '/');
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');

            return string.IsNullOrWhiteSpace(name) ? "drink" : name;
        }
    }
}

