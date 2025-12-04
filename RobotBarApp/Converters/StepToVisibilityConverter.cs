using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace RobotBarApp.Converters
{
    public class StepToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int step = (int)value;
            int target = int.Parse(parameter.ToString());

            return step == target ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => null;
    }
}