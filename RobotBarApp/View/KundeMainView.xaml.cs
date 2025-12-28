using System.Windows;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View;

public partial class KundeMainView : Window
{
    private KundeMainViewModel ViewModel => (KundeMainViewModel)DataContext;

    public KundeMainView(KundeMainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}