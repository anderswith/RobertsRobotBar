using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RobotBarApp.Converters
{
    public sealed class BooleanToVisibilityConverter : IValueConverter
    {
        public bool Invert { get; set; }

        public object Convert(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            var flag = value is bool b && b;
            if (Invert) flag = !flag;
            return flag ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, System.Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not Visibility v) return DependencyProperty.UnsetValue;
            var flag = v == Visibility.Visible;
            return Invert ? !flag : flag;
        }
    }
}
