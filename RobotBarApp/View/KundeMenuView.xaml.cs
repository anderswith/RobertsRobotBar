using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View
{
    public partial class KundeMenuView : Window
    {
        private const double ScrollStep = 380; // roughly 1 card + margin
        private readonly IMenuLogic _menuLogic;
        private readonly INavigationService _navigation;

        public KundeMenuView(IMenuLogic menuLogic, INavigationService navigation)
        {
            _navigation = navigation;
            _menuLogic = menuLogic;
            InitializeComponent();
            // DataContext = new KundeMenuViewModel(); // recommended
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Return to KundeStartView, similar to MixSelv's back behavior.
            var startView = App.AppHost.Services.GetRequiredService<KundeStartView>();
            startView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            startView.WindowState = WindowState.Maximized;
            startView.WindowStyle = WindowStyle.None;

            // Ensure proper VM is used (DI).
            startView.DataContext = App.AppHost.Services.GetRequiredService<ViewModels.KundeStartViewModel>();

            startView.Show();
            startView.Activate();

            Close();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
            => DrinksScroll.ScrollToHorizontalOffset(DrinksScroll.HorizontalOffset - ScrollStep);

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
            => DrinksScroll.ScrollToHorizontalOffset(DrinksScroll.HorizontalOffset + ScrollStep);

        private void DrinkCard_Click(object sender, RoutedEventArgs e)
        {
            // sender is KundeMenuItemCard, whose DataContext is the drink
            // Hook up order flow later.
        }
    }

    // Example VM (replace with your real model)
    public class DrinkVm
    {
        public string Name { get; set; }
        public string IngredientsText { get; set; }
        public string ImagePath { get; set; } // or ImageSource
    }
}