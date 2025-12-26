using System.Windows;
using System.Windows.Controls;
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

       /* private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Re-open a fresh KundeStartView when going back
            var startView = new KundeStartView
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = new KundeStartViewModel(_menuLogic, _navigation)
            };

            startView.Show();
            startView.Activate();

            // Close this menu window
            Close();
        }*/

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