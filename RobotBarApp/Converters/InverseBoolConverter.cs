using System;
using System.Globalization;
using System.Windows.Data;

namespace RobotBarApp.Converters
{
    public sealed class InverseBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var mode = parameter as string;

            if (string.Equals(mode, "opacity", StringComparison.OrdinalIgnoreCase))
            {
                if (value is bool b)
                    return b ? 1.0 : 0.35;
                return 1.0;
            }

            if (value is bool bb) return !bb;
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
