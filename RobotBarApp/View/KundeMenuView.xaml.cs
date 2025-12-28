using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View
{
    public partial class KundeMenuView : UserControl
    {
        private const double ScrollStep = 380; // roughly 1 card + margin

        public KundeMenuView()
        {
            InitializeComponent();
        }
        
        private void ScrollLeft_Click(object sender, RoutedEventArgs e)
            => DrinksScroll.ScrollToHorizontalOffset(DrinksScroll.HorizontalOffset - ScrollStep);

        private void ScrollRight_Click(object sender, RoutedEventArgs e)
            => DrinksScroll.ScrollToHorizontalOffset(DrinksScroll.HorizontalOffset + ScrollStep);


    }


}