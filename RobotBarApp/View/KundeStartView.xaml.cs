using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RobotBarApp.Services.UI;

namespace RobotBarApp.View
{
    public partial class KundeStartView : Window
    {
        public KundeStartView()
        {
            InitializeComponent();
            Loaded += KundeStartView_Loaded;
        }

        private void KundeStartView_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded -= KundeStartView_Loaded;

            // Initialize the shared sizing instance used by XAML bindings.
            var settings = CarouselSizingSettings.Instance;
            if (settings.IsInitialized)
                return;

            // Kiosk assumption: use primary screen size.
            settings.InitializeFromScreenSize(SystemParameters.PrimaryScreenWidth, SystemParameters.PrimaryScreenHeight);
        }
    }
}