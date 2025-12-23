using System;
using System.Windows;
using System.Windows.Controls;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View
{
    public partial class KundeMenuView : UserControl
    {
        private const double ScrollStep = 380; // roughly 1 card + margin
        private readonly IMenuLogic _menuLogic;

        public event EventHandler? BackRequested;

        public KundeMenuView(IMenuLogic menuLogic)
        {
            _menuLogic = menuLogic;
            InitializeComponent();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackRequested?.Invoke(this, EventArgs.Empty);
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