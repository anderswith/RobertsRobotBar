using System.Windows.Input;

namespace RobotBarApp.ViewModels;

public class KundeStartViewModel
{
    public ICommand OpenMenuCommand { get; }
    public ICommand OpenMixSelvCommand { get; }

    public KundeStartViewModel()
    {
        // Navigation is handled by KundeStartView swapping HostContent.
        // Keep commands so existing XAML bindings remain valid.
        OpenMenuCommand = new RelayCommand(_ => { });
        OpenMixSelvCommand = new RelayCommand(_ => { });
    }
}