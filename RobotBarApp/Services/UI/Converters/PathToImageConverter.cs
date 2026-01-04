using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using RobotBarApp.Services.UI;

namespace RobotBarApp.Converters
{
    public class PathToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Parameter can be: Ingredient / Drink / Event
            var kind = (parameter as string)?.Trim();

            var path = value as string;
            if (string.IsNullOrWhiteSpace(path))
                return TryLoad(kind, GetDefaultPath(kind));

            path = path.TrimStart('/');

            // Resolve PROJECT ROOT (not bin)
            var projectRoot =
                Directory.GetParent(AppContext.BaseDirectory)!  // net9.0-windows
                    .Parent!                                   // Debug
                    .Parent!                                   // bin
                    .Parent!.FullName;                          // RobotBarApp

            var full = Path.Combine(projectRoot, path);

            // Missing file -> fallback
            if (!File.Exists(full))
                return TryLoad(kind, GetDefaultPath(kind));

            try
            {
                return LoadBitmap(full);
            }
            catch
            {
                // Corrupt/unreadable -> fallback
                return TryLoad(kind, GetDefaultPath(kind));
            }
        }

        private static object TryLoad(string? kind, string defaultPath)
        {
            try
            {
                // defaultPath is stored like "Resources/..."; resolve the same way as normal paths.
                var projectRoot =
                    Directory.GetParent(AppContext.BaseDirectory)!
                        .Parent!
                        .Parent!
                        .Parent!.FullName;

                var full = Path.Combine(projectRoot, defaultPath.Replace('/', Path.DirectorySeparatorChar));
                if (!File.Exists(full))
                    return null; // if default is missing, don't crash the UI

                return LoadBitmap(full);
            }
            catch
            {
                return null;
            }
        }

        private static string GetDefaultPath(string? kind)
        {
            return kind?.Equals("Drink", StringComparison.OrdinalIgnoreCase) == true
                ? DefaultImagePaths.Drink
                : kind?.Equals("Event", StringComparison.OrdinalIgnoreCase) == true
                    ? DefaultImagePaths.Event
                    : DefaultImagePaths.Ingredient;
        }

        private static BitmapImage LoadBitmap(string absolutePath)
        {
            var bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.CacheOption = BitmapCacheOption.OnLoad;
            bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            bmp.UriSource = new Uri(absolutePath, UriKind.Absolute);
            bmp.EndInit();
            bmp.Freeze();
            return bmp;
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}