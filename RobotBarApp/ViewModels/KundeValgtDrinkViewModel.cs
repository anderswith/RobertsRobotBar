using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using RobotBarApp.BE;
using RobotBarApp.BLL;
using RobotBarApp.BLL.Interfaces;

namespace RobotBarApp.ViewModels;

public class KundeValgtDrinkViewModel : ViewModelBase
{
    private readonly IRobotLogic _robotLogic;
    public string DrinkName { get; }
    public string DrinkImagePath { get; }
    public string IngredientsText { get; }
    public Drink SelectedDrink { get; }
    public ICommand BestilCommand { get; }

    public KundeValgtDrinkViewModel(IRobotLogic robotLogic, Drink drink)
    {
        SelectedDrink = drink;
        _robotLogic = robotLogic;
        DrinkName = drink.Name;

        IngredientsText = drink.DrinkContents != null && drink.DrinkContents.Any()
            ? string.Join(
                Environment.NewLine,
                drink.DrinkContents.Select(dc => dc.Ingredient.Name))
            : "Ingen ingredienser";
        
        DrinkImagePath = drink.Image;
        
        BestilCommand = new RelayCommand(_ => Bestil());
    }
    
    private void Bestil()
    {
        MessageBox.Show($"Din drink er bestilt! Vent venligst mens robotten mixer din drink.{SelectedDrink.Name}::{SelectedDrink.DrinkId}");
        _robotLogic.RunDrinkScripts(SelectedDrink.DrinkId);
    }
}