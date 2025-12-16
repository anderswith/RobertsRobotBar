using System.Windows;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View
{
    public partial class KundeMenuView : Window
    {
        private const double ScrollStep = 380; // roughly 1 card + margin

        public KundeMenuView()
        {
            InitializeComponent();
            // DataContext = new KundeMenuViewModel(); // recommended
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // Re-open a fresh KundeStartView when going back
            var startView = new KundeStartView
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
                DataContext = new KundeStartViewModel()
            };

            startView.Show();
            startView.Activate();

            // Close this menu window
            Close();
        }

        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
            => DrinksScroll.ScrollToHorizontalOffset(DrinksScroll.HorizontalOffset - ScrollStep);

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
            => DrinksScroll.ScrollToHorizontalOffset(DrinksScroll.HorizontalOffset + ScrollStep);

        private void DrinkCard_Click(object sender, RoutedEventArgs e)
        {
            // sender is KundeMenuItemCard, whose DataContext is the drink VM
            if (sender is FrameworkElement fe && fe.DataContext is DrinkVm drink)
            {
                // Navigate to drink details / confirmation
                // new DrinkDetailsView(drink).Show();
                // Close();
            }
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