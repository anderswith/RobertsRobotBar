using System;
using System.Globalization;
using System.Windows.Data;

namespace RobotBarApp.Converters
{
    /// Converts a segment index (0..14) to a Canvas.Top coordinate in the 520x520 glass coordinate space.
    /// Segment 0 is the bottom-most filled segment.
    public sealed class SegmentIndexToCanvasTopConverter : IValueConverter
    {
        // These values match the bowl clip geometry in KundeMixSelvView.xaml.cs
        private const double BowlBottomY = 455.0;
        private const double SegmentHeight = 16.0;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not int index)
                return BowlBottomY;

            // Place the rectangle so it stacks upward from BowlBottomY.
            // index=0 => top = bottom - 1*height
            // index=1 => top = bottom - 2*height, etc.
            return BowlBottomY - (index + 1) * SegmentHeight;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}

