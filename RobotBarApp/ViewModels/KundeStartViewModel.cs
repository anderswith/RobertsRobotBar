using System.Windows.Input;

namespace RobotBarApp.ViewModels;

public class KundeStartViewModel
{
    public ICommand OpenMenuCommand { get; }
    public ICommand OpenMixSelvCommand { get; }

    public KundeStartViewModel()
    {
        OpenMenuCommand = new RelayCommand(_ => OpenMenu());
        OpenMixSelvCommand = new RelayCommand(_ => OpenMixSelv());
    }

    private void OpenMenu()
    {
        // TODO: navigate to customer menu screen
    }

    private void OpenMixSelv()
    {
        // TODO: navigate to mix-selv flow
    }
}