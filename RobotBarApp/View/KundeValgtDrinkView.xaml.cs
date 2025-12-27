using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View;

public partial class KundeValgtDrinkView : Window
{
    public KundeValgtDrinkView()
    {
        InitializeComponent();
    }

    private void Back_Click(object sender, RoutedEventArgs e)
    {
        // Re-open the menu window and close this detail view.
        var menuView = App.AppHost.Services.GetRequiredService<KundeMenuView>();
        menuView.DataContext = App.AppHost.Services.GetRequiredService<KundeMenuViewModel>();

        menuView.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        menuView.WindowState = WindowState.Maximized;
        menuView.WindowStyle = WindowStyle.None;

        menuView.Show();
        menuView.Activate();

        Close();
    }
}