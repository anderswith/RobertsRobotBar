using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.Services.UI;

namespace RobotBarApp.ViewModels;

public class KatalogViewModel : ViewModelBase
{
    private readonly IDrinkLogic _drinkLogic;
    private readonly IIngredientLogic _ingredientLogic;
    private readonly IEventLogic _eventLogic;
    private readonly Guid? _ingredientId;
    private readonly INavigationService _navigationService;
    
    // Master list (never filtered)
    private readonly List<KatalogItemViewModel> _allItems = new();

    // Currently active filter
    private KatalogItemType? _activeFilter = null;
    
    //filter options
    private bool _isIngredientsSelected;
    private bool _isDrinksSelected;
    private bool _isEventsSelected;
    private string _searchText = string.Empty;

    public ObservableCollection<KatalogItemViewModel> CatalogItems { get; }
        = new();

    public ICommand FilterCommand { get; }

    public KatalogViewModel(
        IDrinkLogic drinkLogic,
        IIngredientLogic ingredientLogic,
        IEventLogic eventLogic,
        INavigationService navigation)
    {
        _drinkLogic = drinkLogic;
        _ingredientLogic = ingredientLogic;
        _eventLogic = eventLogic;
        _navigationService = navigation;

        FilterCommand = new RelayCommand(ChangeFilter);

        LoadAll();
    }
    

    private void LoadAll()
    {
        CatalogItems.Clear();
        _allItems.Clear();

        LoadDrinks();
        LoadIngredients();
        LoadEvents();

        ApplyFilter();
    }

    private void LoadDrinks()
    {
        foreach (var drink in _drinkLogic.GetAllDrinks())
        {
            _allItems.Add(
                CreateItem(
                    drink.DrinkId,
                    drink.Name,
                    drink.Image,
                    KatalogItemType.Drink));
        }
    }

    private void LoadIngredients()
    {
        foreach (var ingredient in _ingredientLogic.GetAllIngredients())
        {
            _allItems.Add(
                CreateItem(
                    ingredient.IngredientId,
                    ingredient.Name,
                    ingredient.Image,
                    KatalogItemType.Ingredient));
        }
    }

    private void LoadEvents()
    {
        foreach (var ev in _eventLogic.GetAllEvents())
        {
            _allItems.Add(
                CreateItem(
                    ev.EventId,
                    ev.Name,
                    ev.Image,
                    KatalogItemType.Event));
        }
    }



    private KatalogItemViewModel CreateItem(
        Guid id,
        string name,
        string imagePath,
        KatalogItemType type)
    {
        return new KatalogItemViewModel(
            id,
            name,
            LoadImageOrDefault(imagePath, type),
            type,
            DeleteItem,
            EditItem);
    }


    public bool IsIngredientsSelected
    {
        get => _isIngredientsSelected;
        set
        {
            if (_isIngredientsSelected == value) return;
            _isIngredientsSelected = value;
            OnPropertyChanged();
        }
    }
    public bool IsEventsSelected
    {
        get => _isEventsSelected;
        set
        {
            if (_isEventsSelected == value) return;
            _isEventsSelected = value;
            OnPropertyChanged();
        }
    }
    public bool IsDrinksSelected
    {
        get => _isDrinksSelected;
        set
        {
            if (_isDrinksSelected == value) return;
            _isDrinksSelected = value;
            OnPropertyChanged();
        }
    }
    
    private void ChangeFilter(object? parameter)
    {
        if (parameter is not string filter)
            return;

        KatalogItemType? clickedFilter = filter switch
        {
            "Ingredients" => KatalogItemType.Ingredient,
            "Drinks"      => KatalogItemType.Drink,
            "Events"      => KatalogItemType.Event,
            _             => null
        };

        // Toggle behavior
        _activeFilter = _activeFilter == clickedFilter
            ? null
            : clickedFilter;

        // 🔑 Sync visual state with actual filter
        IsIngredientsSelected = _activeFilter == KatalogItemType.Ingredient;
        IsDrinksSelected      = _activeFilter == KatalogItemType.Drink;
        IsEventsSelected      = _activeFilter == KatalogItemType.Event;

        ApplyFilter();
    }

    private void DeleteItem(Guid id, KatalogItemType type)
    {
        var result = MessageBox.Show(
            "Er du sikker på, at du vil slette denne?",
            "Bekræft sletning",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        try
        {
            switch (type)
            {
                case KatalogItemType.Drink:
                    _drinkLogic.DeleteDrink(id);
                    break;

                case KatalogItemType.Ingredient:
                    _ingredientLogic.DeleteIngredient(id);
                    break;

                case KatalogItemType.Event:
                    _eventLogic.DeleteEvent(id);
                    break;
            }

            RemoveItem(id, type);
        }
        catch (InvalidOperationException ex)
        {
            // Forretningsregel-fejl (fx "ingredient is used in bar setup")
            MessageBox.Show(
                ex.Message,
                "Kan ikke slette",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }
        catch (Exception ex)
        {
            // Uventede fejl – stadig bedre end crash
            MessageBox.Show(
                "Der opstod en uventet fejl under sletning.\n\n" + ex.Message,
                "Fejl",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }


    private void EditItem(Guid id, KatalogItemType type)
    {
        switch (type)
        {
            case KatalogItemType.Ingredient:
                _navigationService.NavigateTo<TilfoejIngrediensViewModel>(id);
                break;

            case KatalogItemType.Drink:
                _navigationService.NavigateTo<TilfoejDrinkViewModel>(id);
                break;

            case KatalogItemType.Event:
                _navigationService.NavigateTo<TilfoejEventViewModel>(id);
                break;
        }
    }

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (_searchText == value)
                return;

            _searchText = value;
            OnPropertyChanged();
            ApplyFilter(); // realtime filtering
        }
    }

    private void ApplyFilter()
    {
        CatalogItems.Clear();

        IEnumerable<KatalogItemViewModel> items = _allItems;

        if (_activeFilter.HasValue)
        {
            items = items.Where(i => i.ItemType == _activeFilter.Value);
        }
        if (!string.IsNullOrWhiteSpace(SearchText))
        {
            items = items.Where(i =>
                i.Name.StartsWith(
                    SearchText,
                    StringComparison.OrdinalIgnoreCase));
        }

        foreach (var item in items)
            CatalogItems.Add(item);
    }



    
    private void RemoveItem(Guid id, KatalogItemType type)
    {
        var item = _allItems.FirstOrDefault(i => i.Id == id && i.ItemType == type);
        if (item != null)
        {
            _allItems.Remove(item);
            CatalogItems.Remove(item);
        }
    }

    private ImageSource LoadImageOrDefault(string path, KatalogItemType type)
    {
        var img = LoadImage(path);
        if (img != null)
            return img;

        var fallbackPath = type switch
        {
            KatalogItemType.Drink      => DefaultImagePaths.Drink,
            KatalogItemType.Event      => DefaultImagePaths.Event,
            KatalogItemType.Ingredient => DefaultImagePaths.Ingredient,
            _                          => DefaultImagePaths.Ingredient
        };

        return LoadImage(fallbackPath);
    }

    private ImageSource LoadImage(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        string absolutePath = Path.IsPathRooted(path)
            ? path
            : Path.Combine(
                Directory.GetParent(AppContext.BaseDirectory)!.Parent!.Parent!.Parent!.FullName,
                path.Replace("/", Path.DirectorySeparatorChar.ToString())
            );

        if (!File.Exists(absolutePath))
            return null;

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(absolutePath, UriKind.Absolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            image.Freeze();

            return image;
        }
        catch
        {
            return null;
        }
    }


}
