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

    // Bound by KundeValgtDrinkView.xaml
    public ICommand BestilCommand { get; }

    private bool _isOrdering;
    public bool IsOrdering
    {
        get => _isOrdering;
        set => SetProperty(ref _isOrdering, value);
    }

    public KundeValgtDrinkViewModel(IRobotLogic robotLogic, Drink drink, INavigationService navigation)
    {
        SelectedDrink = drink;
        _robotLogic = robotLogic;
        DrinkName = drink.Name;
        _navigationService = navigation;
        
        _robotLogic.DrinkFinished += OnDrinkFinished;

        IngredientsText = drink.DrinkContents != null && drink.DrinkContents.Any()
            ? string.Join(
                Environment.NewLine,
                drink.DrinkContents.Select(dc => dc.Ingredient.Name))
            : "Ingen ingredienser";
        
        DrinkImagePath = drink.Image;
        BackCommand = new RelayCommand(_ => GoBack());
        OrderCommand = new RelayCommand(_ => _ = OrderAsync());
        BestilCommand = OrderCommand;
    }
    
    private void OnDrinkFinished()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            IsOrdering = false;

            //_navigationService.NavigateTo<VærsgoViewModel>();

            // unsubscribe
            _robotLogic.DrinkFinished -= OnDrinkFinished;
        });
    }

    private async Task OrderAsync()
    {
        if (IsOrdering) return;

        IsOrdering = true;
        try
        {
            await Task.Run(() => _robotLogic.RunDrinkScripts(SelectedDrink.DrinkId));
            
        }
        catch (Exception ex)
        {
            IsOrdering = false;
            MessageBox.Show(ex.Message, "Order failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GoBack()
    {
        // Allow Back to stop the spinner and return.
        IsOrdering = false;
        _navigationService.NavigateTo<KundeMenuViewModel>();
    }
}