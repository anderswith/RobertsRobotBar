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
    private readonly IServiceProvider _provider;

    public KundeStartViewModel(
        IMenuLogic menuLogic,
        IServiceProvider provider)
    {
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
        // Show MixSelv inside the existing KundeStartView (HostContent) instead of opening a new window.
        var startWindow = Application.Current.Windows
            .OfType<KundeStartView>()
            .FirstOrDefault();

        if (startWindow == null)
            return;

        var mixSelvView = ActivatorUtilities.CreateInstance<KundeMixSelvView>(_provider);

        // Inject ingredient logic so MixSelv can populate the overlay carousel from DB.
        var ingredientLogic = _provider.GetRequiredService<IIngredientLogic>();
        mixSelvView.DataContext = new KundeMixSelvViewModel(ingredientLogic);

        mixSelvView.BackRequested += (_, _) =>
        {
            // Clear the hosted content and show the start screen again.
            startWindow.HostContent.Content = null;
            startWindow.StartRoot.Visibility = Visibility.Visible;
            startWindow.Activate();
            startWindow.Focus();
        };

        startWindow.HostContent.Content = mixSelvView;
        startWindow.StartRoot.Visibility = Visibility.Collapsed;
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