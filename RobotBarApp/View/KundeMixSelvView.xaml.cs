using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View
{
    public partial class KundeMixSelvView
    {
        public event EventHandler? BackRequested;

        public KundeMixSelvView()
        {
            InitializeComponent();

            // Prefer DI/parent-provided DataContext.
            // Fallback to AppHost if nothing is set (keeps designer/runtime robust).
            if (DataContext == null)
            {
                DataContext = App.AppHost?.Services.GetService(typeof(KundeMixSelvViewModel)) as KundeMixSelvViewModel
                              ?? new KundeMixSelvViewModel();
            }

            if (DataContext is KundeMixSelvViewModel vm)
                vm.BackRequested += (_, _) => BackRequested?.Invoke(this, EventArgs.Empty);

            Loaded += KundeMixSelvView_Loaded;
        }

        private void KundeMixSelvView_Loaded(object sender, RoutedEventArgs e)
        {
            LiquidLayer.Clip = CreateBowlClipGeometry();
        }

        private static Geometry CreateBowlClipGeometry()
        {
            // Trapezoid clip approximating the inner walls of the glass (normalized 520x520).
            // This is used to keep the liquid inside the glass bowl.
            var topY = 180.0;
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

        private void OverlayScrollLeft_Click(object sender, RoutedEventArgs e)
            => OverlayScroll.ScrollToHorizontalOffset(OverlayScroll.HorizontalOffset - 700);

        private void OverlayScrollRight_Click(object sender, RoutedEventArgs e)
            => OverlayScroll.ScrollToHorizontalOffset(OverlayScroll.HorizontalOffset + 700);

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("[KundeMixSelv] Back_Click invoked");
            BackRequested?.Invoke(this, EventArgs.Empty);
        }

        private void OverlayBackground_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (DataContext is KundeMixSelvViewModel vm)
                vm.IsOverlayOpen = false;

            e.Handled = true;
        }
    }
}
