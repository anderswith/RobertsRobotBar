using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.View;

namespace RobotBarApp.ViewModels;

public class KundeStartViewModel : ViewModelBase
{
    public ICommand OpenMenuCommand { get; }
    public ICommand OpenMixSelvCommand { get; }
    private readonly IMenuLogic _menuLogic;
    private readonly IServiceProvider _provider;

    public KundeStartViewModel(
        IMenuLogic menuLogic,
        IServiceProvider provider)
    {
        _menuLogic = menuLogic;
        _provider = provider;

        OpenMenuCommand = new RelayCommand(_ => OpenMenu());
        OpenMixSelvCommand = new RelayCommand(_ => OpenMixSelv());
    }

    private void OpenMenu()
    {
        var menuWindow =
            ActivatorUtilities.CreateInstance<KundeMenuView>(_provider);

        menuWindow.DataContext =
            ActivatorUtilities.CreateInstance<KundeMenuViewModel>(_provider);

        menuWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        menuWindow.WindowState = WindowState.Maximized;
        menuWindow.WindowStyle = WindowStyle.None;

        menuWindow.Show();
        menuWindow.Activate();

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