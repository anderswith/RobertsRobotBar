using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
using System.Windows;
using System.IO;

namespace RobotBarApp.ViewModels
{
    public class TilfoejEventViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IIngredientLogic _ingredientLogic;
        private readonly IEventLogic _eventLogic;
        private readonly IDrinkLogic _drinkLogic;
        private readonly IMenuLogic _menuLogic;

        public TilfoejEventViewModel(INavigationService navigationService,
                                     IIngredientLogic ingredientLogic,
                                     IEventLogic eventLogic,
                                     IDrinkLogic drinkLogic,
                                     IMenuLogic menuLogic)
        {
            _navigationService = navigationService;
            _ingredientLogic = ingredientLogic;
            _eventLogic = eventLogic;
            _drinkLogic = drinkLogic;
            _menuLogic = menuLogic;

            Step = 1;

            Ingredients = new ObservableCollection<Ingredient>(_ingredientLogic.GetAllIngredients());
            FilteredIngredients = new ObservableCollection<Ingredient>(Ingredients);

            RackItems = new ObservableCollection<RackSlot>();
            MenuDrinks = new ObservableCollection<string>();
            SelectedDrinkIds = new ObservableCollection<Guid>();

            ChooseImageCommand = new RelayCommand(_ => ChooseImage());
            NextCommand = new RelayCommand(_ => Step = 2);
            CancelCommand = new RelayCommand(_ => _navigationService.NavigateTo<EventListViewModel>());
            AddIngredientCommand = new RelayCommand(AddIngredientToRack);
            SaveCommand = new RelayCommand(_ => SaveEvent());
        }

        // -------------------------
        // STEP MANAGEMENT
        // -------------------------
        private int _step;
        public int Step
        {
            get => _step;
            set => SetProperty(ref _step, value);
        }

        // -------------------------
        // STEP 1 — NAME + IMAGE
        // -------------------------

        private string _eventName = string.Empty;
        public string EventName
        {
            get => _eventName;
            set => SetProperty(ref _eventName, value);
        }

        private string _imagePath = string.Empty;
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        private BitmapImage? _eventImage;
        public BitmapImage? EventImage
        {
            get => _eventImage;
            set => SetProperty(ref _eventImage, value);
        }

        public RelayCommand ChooseImageCommand { get; }

        private void ChooseImage()
        {
            var dialog = new OpenFileDialog();
            dialog.Filter = "Image files|*.jpg;*.png;*.jpeg;*.bmp";

            if (dialog.ShowDialog() == true)
            {
                ImagePath = dialog.FileName;
                EventImage = new BitmapImage(new Uri(ImagePath));
            }
        }

        public RelayCommand NextCommand { get; }
        public RelayCommand CancelCommand { get; }

        // -------------------------
        // STEP 2 — INGREDIENTS + RACK + MENU
        // -------------------------

        public ObservableCollection<Ingredient> Ingredients { get; }
        public ObservableCollection<Ingredient> FilteredIngredients { get; }

        private string _ingredientSearch = string.Empty;
        public string IngredientSearch
        {
            get => _ingredientSearch;
            set
            {
                if (SetProperty(ref _ingredientSearch, value))
                {
                    FilterIngredients();
                }
            }
        }

        private void FilterIngredients()
        {
            FilteredIngredients.Clear();

            foreach (var ing in Ingredients.Where(i => string.IsNullOrEmpty(IngredientSearch) ||
                                                       i.Name.Contains(IngredientSearch, StringComparison.OrdinalIgnoreCase)))
            {
                FilteredIngredients.Add(ing);
            }
        }

        public ObservableCollection<RackSlot> RackItems { get; }
        public ObservableCollection<string> MenuDrinks { get; }
        public ObservableCollection<Guid> SelectedDrinkIds { get; }

        public RelayCommand AddIngredientCommand { get; }

        private void AddIngredientToRack(object? param)
        {
            if (param is not Ingredient ing) return;

            RackItems.Add(new RackSlot
            {
                Ingredient = ing,
                ImageSource = ing.Image
            });

            // Show menu after 1+ ingredient
            HasMenu = true;

            // Suggest drinks based on selected ingredients
            MenuDrinks.Clear();
            SelectedDrinkIds.Clear();
            foreach (var drink in _drinkLogic.GetAllDrinks())
            {
                if (drink.DrinkContents != null &&
                    drink.DrinkContents.Any(dc => RackItems.Any(r => r.Ingredient.IngredientId == dc.IngredientId)))
                {
                    MenuDrinks.Add(drink.Name);
                    if (!SelectedDrinkIds.Contains(drink.DrinkId))
                    {
                        SelectedDrinkIds.Add(drink.DrinkId);
                    }
                }
            }
        }

        private bool _hasMenu;
        public bool HasMenu
        {
            get => _hasMenu;
            set => SetProperty(ref _hasMenu, value);
        }

        // -------------------------
        // SAVE EVENT
        // -------------------------

        public RelayCommand SaveCommand { get; }

        private void SaveEvent()
        {
            if (string.IsNullOrWhiteSpace(EventName))
                return;

            if (string.IsNullOrWhiteSpace(ImagePath) || !File.Exists(ImagePath))
            {
                MessageBox.Show("Vælg et billede til eventet, før du gemmer.");
                return;
            }

            if (!SelectedDrinkIds.Any())
            {
                MessageBox.Show("Tilføj mindst én ingrediens, så der kan vælges drinks til menuen.");
                return;
            }

            var storedEventImagePath = CopyEventImageToResources();

            var menuName = string.IsNullOrWhiteSpace(EventName) ? "Menu" : $"{EventName} Menu";
            _menuLogic.AddMenuWithDrinks(menuName, SelectedDrinkIds.ToList());
            var menuId = _menuLogic.GetAllMenus()
                .Where(m => m.Name == menuName)
                .OrderByDescending(m => m.MenuId)
                .Select(m => m.MenuId)
                .FirstOrDefault();

            if (menuId == Guid.Empty)
            {
                MessageBox.Show("Kunne ikke oprette menukortet. Prøv igen.");
                return;
            }

            _eventLogic.AddEvent(EventName, storedEventImagePath, menuId);

            _navigationService.NavigateTo<EventListViewModel>();
        }

        private string CopyEventImageToResources()
        {
            var destinationFolder = GetEventPicsDirectory();
            Directory.CreateDirectory(destinationFolder);

            var extension = Path.GetExtension(ImagePath);
            var safeName = new string((EventName ?? "event").Select(ch =>
                Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch).ToArray());
            if (string.IsNullOrWhiteSpace(safeName))
                safeName = "event";

            var fileName = $"{safeName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            var destinationPath = Path.Combine(destinationFolder, fileName);

            File.Copy(ImagePath, destinationPath, overwrite: true);

            return Path.GetRelativePath(AppContext.BaseDirectory, destinationPath);
        }

        private string GetEventPicsDirectory()
        {
            var baseDir = AppContext.BaseDirectory;
            var projectResourcePath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Resources", "EventPics"));

            try
            {
                Directory.CreateDirectory(projectResourcePath);
                return projectResourcePath;
            }
            catch
            {
                return Path.Combine(baseDir, "Resources", "EventPics");
            }
        }
    }

    // Helper model for rack position
    public class RackSlot
    {
        public Ingredient Ingredient { get; set; } = null!;
        public string? ImageSource { get; set; }
    }
}
