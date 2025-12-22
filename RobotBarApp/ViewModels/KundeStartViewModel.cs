using System.Linq;
using System.Windows;
using System.Windows.Input;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.View;

namespace RobotBarApp.ViewModels;

public class KundeStartViewModel
{
    public ICommand OpenMenuCommand { get; }
    public ICommand OpenMixSelvCommand { get; }
    private readonly IMenuLogic _menuLogic;

    public KundeStartViewModel(IMenuLogic menuLogic)
    {
        _menuLogic = menuLogic;
        OpenMenuCommand = new RelayCommand(_ => OpenMenu());
        OpenMixSelvCommand = new RelayCommand(_ => OpenMixSelv());
    }

    private void OpenMenu()
    {
        var vm = new KundeMenuViewModel(_menuLogic);

        var menuWindow = new KundeMenuView(_menuLogic)
        {
            DataContext = vm,
            WindowStartupLocation = WindowStartupLocation.CenterScreen
        };

        // Show the menu window and bring it to front
        menuWindow.Show();
        menuWindow.Activate();

        // Close the start window if it is open
        CloseKundeStartWindow();
    }

    private void OpenMixSelv()
    {
        // TODO: replace with real MixSelv view when available
        var mixSelvWindow = new Window
        {
            Title = "Mix Selv (placeholder)",
            WindowState = WindowState.Maximized,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.Black
        };

        mixSelvWindow.Show();
        mixSelvWindow.Activate();

        CloseKundeStartWindow();
    }

    private static void CloseKundeStartWindow()
    {
        // Find any open KundeStartView window and close it
        var startWindow = Application.Current.Windows
            .OfType<KundeStartView>()
            .FirstOrDefault();

        startWindow?.Close();
    }
}