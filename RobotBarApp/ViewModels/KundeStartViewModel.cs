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
    private readonly INavigationService _navigation;


    public KundeStartViewModel(
        INavigationService navigation
        )
    {
        _navigation = navigation;
        OpenMenuCommand = new RelayCommand(_ => OpenMenu());
        OpenMixSelvCommand = new RelayCommand(_ => OpenMixSelv());
    }

    private void OpenMenu()
    {
        _navigation.NavigateTo<KundeMenuViewModel>();
    }

    private void OpenMixSelv()
    {
        _navigation.NavigateTo<KundeMixSelvViewModel>();
    }


}