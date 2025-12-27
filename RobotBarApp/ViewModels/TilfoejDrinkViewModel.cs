using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.Services.UI;

namespace RobotBarApp.ViewModels
{
    public class TilfoejDrinkViewModel : ViewModelBase
    {
       
        private readonly IIngredientLogic _ingredientLogic;
        private readonly IDrinkLogic _drinkLogic;
        private readonly INavigationService _navigation;
        private readonly IEventLogic _eventLogic;
        private readonly Guid _eventId;
        
        private readonly Guid? _drinkId;
        public bool IsEditMode => _drinkId.HasValue;
        public TilfoejDrinkViewModel(
            IIngredientLogic ingredientLogic,
            IDrinkLogic drinkLogic,
            INavigationService navigation,
            IEventLogic eventLogic,
            Guid contextId)
            
        {
            _ingredientLogic = ingredientLogic;
            _drinkLogic = drinkLogic;
            _navigation = navigation;
            _eventLogic = eventLogic;
            
            if (_drinkLogic.Exists(contextId))
            {
                // EDIT MODE
                _drinkId = contextId;
                _eventId = _eventLogic.GetEventIdForDrink(contextId);

                if (_eventId == Guid.Empty)
                {
                    
                    MessageBox.Show(
                        "Denne drink er ikke tilknyttet et event.\n\n" +
                        "Tilføj drinken til en menu, før du redigerer ingredienserne.",
                        "Kan ikke redigere drink"
                        );
                    
                    Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _navigation.NavigateTo<KatalogViewModel>();
                    }));
                }
            }
            else
            {
                // CREATE MODE
                _drinkId = null;
                _eventId = contextId;
            }

            ChooseImageCommand = new RelayCommand(_ => ChooseImage());
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
            GuideCommand = new RelayCommand(_ => ShowGuide());
            
            LoadIngredientLists();
            if (IsEditMode)
            {
                LoadDrinkForEdit();
            }
            
        }
        
        private void LoadDrinkForEdit()
        {
            if (!IsEditMode)
                return;

            var drink = _drinkLogic.GetDrinkById(_drinkId!.Value);
            if (drink == null)
                throw new KeyNotFoundException("Drink not found.");

            // Basic fields
            DrinkName   = drink.Name;
            ImagePreview = drink.Image;
            IsMocktail  = drink.IsMocktail;
            
            ScriptText = drink.DrinkScripts?
                             .OrderBy(s => s.Number)
                             .FirstOrDefault()
                             ?.UrScript
                         ?? "";

            // Ingredients already in the drink
            var drinkIngredients = drink.DrinkContents
                .Select(dc => dc.Ingredient)
                .ToList();

            // Alcohol slots
            SelectedAlcohol1 = drinkIngredients.ElementAtOrDefault(0);
            SelectedAlcohol2 = drinkIngredients.ElementAtOrDefault(1);
            SelectedAlcohol3 = drinkIngredients.ElementAtOrDefault(2);
            SelectedAlcohol4 = drinkIngredients.ElementAtOrDefault(3);

            // Syrups
            SelectedSyrup1 = drinkIngredients
                .FirstOrDefault(i => i.Type == "Syrup");
            SelectedSyrup2 = drinkIngredients
                .Skip(1)
                .FirstOrDefault(i => i.Type == "Syrup");
            SelectedSyrup3 = drinkIngredients
                .Skip(2)
                .FirstOrDefault(i => i.Type == "Syrup");

            // Sodas
            SelectedSoda1 = drinkIngredients
                .FirstOrDefault(i => i.Type == "Soda");
            SelectedSoda2 = drinkIngredients
                .Skip(1)
                .FirstOrDefault(i => i.Type == "Soda");
        }
        private void Cancel()
        {
            if (IsEditMode)
            {
                _navigation.NavigateTo<KatalogViewModel>();
            }
            else
            {
                _navigation.NavigateTo<EventViewModel>(_eventId);
            }
        }
        

        private string _drinkName = "";
        public string DrinkName
        {
            get => _drinkName;
            set => SetProperty(ref _drinkName, value);
        }

        private bool _isMocktail;
        public bool IsMocktail
        {
            get => _isMocktail;
            set => SetProperty(ref _isMocktail, value);
        }

        private string _scriptText = "";
        public string ScriptText
        {
            get => _scriptText;
            set => SetProperty(ref _scriptText, value);
        }

        private string? _imagePreview;
        public string? ImagePreview
        {
            get => _imagePreview;
            set => SetProperty(ref _imagePreview, value);
        }



        public ObservableCollection<Ingredient> AlcoholOptions { get; } = new();
        public ObservableCollection<Ingredient> SyrupOptions { get; } = new();
        public ObservableCollection<Ingredient> SodaOptions { get; } = new();

        private void LoadIngredientLists()
        {
            AlcoholOptions.Clear();
            SyrupOptions.Clear();
            SodaOptions.Clear();

            foreach (var i in _ingredientLogic.GetAlcohol(_eventId)) AlcoholOptions.Add(i);
            foreach (var i in _ingredientLogic.GetSyrups(_eventId)) SyrupOptions.Add(i);
            foreach (var i in _ingredientLogic.GetSoda(_eventId)) SodaOptions.Add(i);
        }

        private Ingredient? _selectedAlcohol1;
        public Ingredient? SelectedAlcohol1
        {
            get => _selectedAlcohol1;
            set => SetProperty(ref _selectedAlcohol1, value);
        }

        private Ingredient? _selectedAlcohol2;
        public Ingredient? SelectedAlcohol2
        {
            get => _selectedAlcohol2;
            set => SetProperty(ref _selectedAlcohol2, value);
        }

        private Ingredient? _selectedAlcohol3;
        public Ingredient? SelectedAlcohol3
        {
            get => _selectedAlcohol3;
            set => SetProperty(ref _selectedAlcohol3, value);
        }

        private Ingredient? _selectedAlcohol4;
        public Ingredient? SelectedAlcohol4
        {
            get => _selectedAlcohol4;
            set => SetProperty(ref _selectedAlcohol4, value);
        }
        public int AlcoholAmount1 { get; set; }
        public int AlcoholAmount2 { get; set; }
        public int AlcoholAmount3 { get; set; }
        public int AlcoholAmount4 { get; set; }

        public ICommand IncreaseAlcoholAmount1Command => new RelayCommand(_ => AlcoholAmount1++);
        public ICommand DecreaseAlcoholAmount1Command => new RelayCommand(_ => { if (AlcoholAmount1 > 0) AlcoholAmount1--; });

        public ICommand IncreaseAlcoholAmount2Command => new RelayCommand(_ => AlcoholAmount2++);
        public ICommand DecreaseAlcoholAmount2Command => new RelayCommand(_ => { if (AlcoholAmount2 > 0) AlcoholAmount2--; });

        public ICommand IncreaseAlcoholAmount3Command => new RelayCommand(_ => AlcoholAmount3++);
        public ICommand DecreaseAlcoholAmount3Command => new RelayCommand(_ => { if (AlcoholAmount3 > 0) AlcoholAmount3--; });

        public ICommand IncreaseAlcoholAmount4Command => new RelayCommand(_ => AlcoholAmount4++);
        public ICommand DecreaseAlcoholAmount4Command => new RelayCommand(_ => { if (AlcoholAmount4 > 0) AlcoholAmount4--; });
        

        private Ingredient? _selectedSyrup1;
        public Ingredient? SelectedSyrup1
        {
            get => _selectedSyrup1;
            set => SetProperty(ref _selectedSyrup1, value);
        }

        private Ingredient? _selectedSyrup2;
        public Ingredient? SelectedSyrup2
        {
            get => _selectedSyrup2;
            set => SetProperty(ref _selectedSyrup2, value);
        }

        private Ingredient? _selectedSyrup3;
        public Ingredient? SelectedSyrup3
        {
            get => _selectedSyrup3;
            set => SetProperty(ref _selectedSyrup3, value);
        }
        

        private Ingredient? _selectedSoda1;
        public Ingredient? SelectedSoda1
        {
            get => _selectedSoda1;
            set => SetProperty(ref _selectedSoda1, value);
        }

        private Ingredient? _selectedSoda2;
        public Ingredient? SelectedSoda2
        {
            get => _selectedSoda2;
            set => SetProperty(ref _selectedSoda2, value);
        }
        

        public ICommand ChooseImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand GuideCommand { get; }

        private void ChooseImage()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp"
            };

            if (dlg.ShowDialog() == true)
                ImagePreview = dlg.FileName;
        }

        private void ShowGuide()
        {
            System.Windows.MessageBox.Show("GUIDE COMING LATER");
        }

        private static bool IsLikelyExternalPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // If it already looks like a project-relative resources path, don't copy.
            var normalized = path.Replace('\\', '/');
            return Path.IsPathRooted(path) && !normalized.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase);
        }

        private void Save()
        {
            try
            {
                // Build ingredient list from slots
                var ingredientIds = new List<Guid>();

                void AddIfSelected(Ingredient? ingredient)
                {
                    if (ingredient != null)
                        ingredientIds.Add(ingredient.IngredientId);
                }

                AddIfSelected(SelectedAlcohol1);
                AddIfSelected(SelectedAlcohol2);
                AddIfSelected(SelectedAlcohol3);
                AddIfSelected(SelectedAlcohol4);

                AddIfSelected(SelectedSyrup1);
                AddIfSelected(SelectedSyrup2);
                AddIfSelected(SelectedSyrup3);

                AddIfSelected(SelectedSoda1);
                AddIfSelected(SelectedSoda2);

                ingredientIds = ingredientIds.Distinct().ToList();
                var scripts = string.IsNullOrWhiteSpace(ScriptText)
                    ? new List<string>()
                    : new List<string> { ScriptText };

                // Ensure image is stored in Resources/DrinkPics and DB contains a relative path.
                var imageToPersist = ImagePreview ?? "";
                if (IsLikelyExternalPath(imageToPersist))
                {
                    imageToPersist = DrinkImageStorage.SaveToDrinkPics(imageToPersist);
                    ImagePreview = imageToPersist;
                }

                if (IsEditMode)
                {
                    _drinkLogic.UpdateDrink(
                        drinkId: _drinkId!.Value,
                        name: DrinkName,
                        image: imageToPersist,
                        isMocktail: IsMocktail,
                        ingredientIds: ingredientIds,
                        scriptNames: scripts
                    );

                    // ✅ Edit → back to catalog
                    _navigation.NavigateTo<KatalogViewModel>();
                }
                else
                {
                    _drinkLogic.AddDrink(
                        name: DrinkName,
                        image: imageToPersist,
                        isMocktail: IsMocktail,
                        ingredientIds: ingredientIds,
                        scriptNames: scripts
                    );

                    // ✅ Create → back to event
                    _navigation.NavigateTo<EventViewModel>(_eventId);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error");
            }
        }
    }
}
