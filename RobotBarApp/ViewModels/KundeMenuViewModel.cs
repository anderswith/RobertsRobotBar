using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;


namespace RobotBarApp.ViewModels;

public class KundeMenuViewModel : ViewModelBase
{
    public ObservableCollection<Drink> Drinks { get; }

    public KundeMenuViewModel(IMenuLogic menuLogic)
    {
        var drinks = menuLogic.GetMenuWithDrinksAndIngredients();
        Drinks = new ObservableCollection<Drink>(drinks);
    }
    

    
}