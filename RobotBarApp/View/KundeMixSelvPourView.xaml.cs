using System.Windows;
using System.Windows.Media;

namespace RobotBarApp.View
{
    public partial class KundeMixSelvPourView
    {
        public KundeMixSelvPourView()
        {
            InitializeComponent();
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Keep the same clip shape as MixSelv so liquid stays inside the glass.
            LiquidLayer.Clip = CreateBowlClipGeometry();
        }

        private static Geometry CreateBowlClipGeometry()
        {
            var topY = 210.0;
            var bottomY = 455.0;

            var topLeftX = 200.0;
            var topRightX = 315.0;

            var bottomLeftX = 235.0;
            var bottomRightX = 285.0;

            var g = new StreamGeometry();
            using (var ctx = g.Open())
            {
                ctx.BeginFigure(new Point(topLeftX, topY), isFilled: true, isClosed: true);
                ctx.LineTo(new Point(topRightX, topY), true, false);
                ctx.LineTo(new Point(bottomRightX, bottomY), true, false);
                ctx.LineTo(new Point(bottomLeftX, bottomY), true, false);
            }

            g.Freeze();
            return g;
        }
    }
}

