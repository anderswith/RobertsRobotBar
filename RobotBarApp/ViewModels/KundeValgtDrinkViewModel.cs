using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RobotBarApp.BE;
using RobotBarApp.BLL;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels;

public class KundeValgtDrinkViewModel : ViewModelBase
{
    private readonly IRobotLogic _robotLogic;
    public string DrinkName { get; }
    public string DrinkImagePath { get; }
    public string IngredientsText { get; }
    public Drink SelectedDrink { get; }
    public ICommand OrderCommand { get; }
    public ICommand BackCommand { get; }
    private readonly INavigationService _navigationService;

    public KundeValgtDrinkViewModel(IRobotLogic robotLogic, Drink drink, INavigationService navigation)
    {
        SelectedDrink = drink;
        _robotLogic = robotLogic;
        DrinkName = drink.Name;
        _navigationService = navigation;

        IngredientsText = drink.DrinkContents != null && drink.DrinkContents.Any()
            ? string.Join(
                Environment.NewLine,
                drink.DrinkContents.Select(dc => dc.Ingredient.Name))
            : "Ingen ingredienser";
        
        DrinkImagePath = drink.Image;
        BackCommand = new RelayCommand(_ => GoBack());
        OrderCommand = new RelayCommand(_ => Order());
    }
    
    private void Order()
    {
       _robotLogic.RunDrinkScripts(SelectedDrink.DrinkId);
    }
    private void GoBack()
    {
        _navigationService.NavigateTo<KundeMenuViewModel>();
            
    }
}