using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.View;


namespace RobotBarApp.ViewModels;

public class KundeMenuViewModel : ViewModelBase
{
    public ObservableCollection<Drink> Drinks { get; }
    public ICommand SelectDrinkCommand { get; }
    public ICommand BackCommand { get; }
    
    private readonly INavigationService _navigation;
    
    public KundeMenuViewModel(
        IMenuLogic menuLogic,
        INavigationService navigation)
    {
        _navigation = navigation;

        Drinks = new ObservableCollection<Drink>(
            menuLogic.GetMenuWithDrinksAndIngredients());
        
        SelectDrinkCommand = new RelayCommand(SelectDrink);
        BackCommand = new RelayCommand(_ => GoBack());
    }

    private void SelectDrink(object? parameter)
    {
        if (parameter is not Drink drink)
            return;
        _navigation.NavigateTo<KundeValgtDrinkViewModel>(drink);
        
    }
    private void GoBack()
    {
        _navigation.NavigateTo<KundeStartViewModel>();
    }
    
}
