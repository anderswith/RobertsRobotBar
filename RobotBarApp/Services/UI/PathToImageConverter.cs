using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace RobotBarApp.Converters
{
    public class PathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var path = value as string;
            if (string.IsNullOrWhiteSpace(path)) return null;

            // Accept both "Resources/..." and "/Resources/...".
            // DB often stores "Resources/..."; some viewmodels prepend '/'.
            path = path.Trim();
            if (path.StartsWith("/"))
                path = path.TrimStart('/');

            // Locate project root from runtime base dir.
            // AppContext.BaseDirectory -> bin/Debug/.../netX
            var baseDir = AppContext.BaseDirectory;
            var root = Directory.GetParent(baseDir);
            for (var i = 0; i < 5 && root != null; i++)
            {
                // Heuristic: project root contains the solution file or the Resources folder.
                if (Directory.Exists(Path.Combine(root.FullName, "Resources")) ||
                    File.Exists(Path.Combine(root.FullName, "RobertsRobotBar.sln")))
                    break;

                root = root.Parent;
            }

            var projectRoot = root?.FullName ?? Directory.GetParent(baseDir)?.FullName;
            if (string.IsNullOrWhiteSpace(projectRoot))
                return null;

            var full = Path.Combine(projectRoot, path);
            if (!File.Exists(full)) return null;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(full, UriKind.Absolute);
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}