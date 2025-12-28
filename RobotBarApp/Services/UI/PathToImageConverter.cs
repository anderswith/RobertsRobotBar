using System;
using System.Diagnostics;
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
            if (string.IsNullOrWhiteSpace(path))
                return null;

            path = path.TrimStart('/');

            // 🔑 Resolve PROJECT ROOT (not bin)
            var projectRoot =
                Directory.GetParent(AppContext.BaseDirectory)!  // net9.0-windows
                    .Parent!                                   // Debug
                    .Parent!                                   // bin
                    .Parent!.FullName;                          // RobotBarApp

            var full = Path.Combine(projectRoot, path);

            if (!File.Exists(full))
                return null;

            try
            {
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
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