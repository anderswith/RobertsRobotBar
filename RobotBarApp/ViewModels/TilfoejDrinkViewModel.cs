using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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
        private readonly IImageStorageService _imageStorageService;
        private readonly Guid _eventId;
        
        private readonly Guid? _drinkId;
        public bool IsEditMode => _drinkId.HasValue;

        // Guard so setting IsMocktail during initial edit-load doesn't clear selections
        private bool _isInitializing;

        public TilfoejDrinkViewModel(
            IIngredientLogic ingredientLogic,
            IDrinkLogic drinkLogic,
            INavigationService navigation,
            IEventLogic eventLogic,
            IImageStorageService imageStorageService,
            Guid contextId)
        {
            _ingredientLogic = ingredientLogic;
            _drinkLogic = drinkLogic;
            _navigation = navigation;
            _eventLogic = eventLogic;
            _imageStorageService = imageStorageService;
            
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

            _isInitializing = true;
            try
            {
                // Basic fields
                DrinkName = drink.Name;
                ImagePreview = drink.Image;

                // Set IsMocktail first so options list is correct before assigning SelectedAlcohol*
                IsMocktail = drink.IsMocktail;

                ScriptText = drink.DrinkScripts?
                                     .OrderBy(s => s.Number)
                                     .FirstOrDefault()
                                     ?.UrScript
                                 ?? "";

                // Load the correct "spirit" type based on mocktail flag
                var spiritType = drink.IsMocktail ? "Mock" : "Alkohol";

                var spiritContents = drink.DrinkContents
                    .Where(dc => dc.Ingredient.Type == spiritType)
                    .ToList();

                SelectedAlcohol1 = spiritContents.ElementAtOrDefault(0)?.Ingredient;
                AlcoholDose1 = spiritContents.ElementAtOrDefault(0)?.Dose ?? "single";

                SelectedAlcohol2 = spiritContents.ElementAtOrDefault(1)?.Ingredient;
                AlcoholDose2 = spiritContents.ElementAtOrDefault(1)?.Dose ?? "single";

                SelectedAlcohol3 = spiritContents.ElementAtOrDefault(2)?.Ingredient;
                AlcoholDose3 = spiritContents.ElementAtOrDefault(2)?.Dose ?? "single";

                SelectedAlcohol4 = spiritContents.ElementAtOrDefault(3)?.Ingredient;
                AlcoholDose4 = spiritContents.ElementAtOrDefault(3)?.Dose ?? "single";

                SelectedSyrup1 = drink.DrinkContents
                    .FirstOrDefault(dc => dc.Ingredient.Type == "Syrup")?.Ingredient;

                SelectedSyrup2 = drink.DrinkContents
                    .Where(dc => dc.Ingredient.Type == "Syrup")
                    .Skip(1).FirstOrDefault()?.Ingredient;

                SelectedSyrup3 = drink.DrinkContents
                    .Where(dc => dc.Ingredient.Type == "Syrup")
                    .Skip(2).FirstOrDefault()?.Ingredient;

                SelectedSoda1 = drink.DrinkContents
                    .FirstOrDefault(dc => dc.Ingredient.Type == "Soda")?.Ingredient;

                SelectedSoda2 = drink.DrinkContents
                    .Where(dc => dc.Ingredient.Type == "Soda")
                    .Skip(1).FirstOrDefault()?.Ingredient;
            }
            finally
            {
                _isInitializing = false;
            }
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
            set
            {
                if (_isMocktail == value)
                    return;

                _isMocktail = value;
                OnPropertyChanged();

                // Update label + options
                OnPropertyChanged(nameof(AlcoholSectionTitle));
                RefreshAlcoholOrMockoholOptions();

                // If user toggles, clear already selected spirit so it can't mix Alkohol/Mock
                if (!_isInitializing)
                    ClearSpiritSelections();
            }
        }

        public string AlcoholSectionTitle => IsMocktail ? "Mockohol:" : "Alkohol:";

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
            RefreshAlcoholOrMockoholOptions();

            SyrupOptions.Clear();
            SodaOptions.Clear();

            foreach (var i in _ingredientLogic.GetSyrups(_eventId)) SyrupOptions.Add(i);
            foreach (var i in _ingredientLogic.GetSoda(_eventId)) SodaOptions.Add(i);
        }

        private void RefreshAlcoholOrMockoholOptions()
        {
            AlcoholOptions.Clear();

            var source = IsMocktail
                ? _ingredientLogic.getMockohol(_eventId)
                : _ingredientLogic.GetAlcohol(_eventId);

            foreach (var i in source)
                AlcoholOptions.Add(i);
        }

        private void ClearSpiritSelections()
        {
            SelectedAlcohol1 = null;
            SelectedAlcohol2 = null;
            SelectedAlcohol3 = null;
            SelectedAlcohol4 = null;

            // Reset doses too (keeps UI consistent)
            AlcoholDose1 = "single";
            AlcoholDose2 = "single";
            AlcoholDose3 = "single";
            AlcoholDose4 = "single";
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
        public IReadOnlyList<string> DoseOptions { get; } =
            new[] { "single", "double" };

        private string _alcoholDose1 = "single";
        public string AlcoholDose1
        {
            get => _alcoholDose1;
            set => SetProperty(ref _alcoholDose1, value);
        }

        private string _alcoholDose2 = "single";
        public string AlcoholDose2
        {
            get => _alcoholDose2;
            set => SetProperty(ref _alcoholDose2, value);
        }

        private string _alcoholDose3 = "single";
        public string AlcoholDose3
        {
            get => _alcoholDose3;
            set => SetProperty(ref _alcoholDose3, value);
        }

        private string _alcoholDose4 = "single";
        public string AlcoholDose4
        {
            get => _alcoholDose4;
            set => SetProperty(ref _alcoholDose4, value);
        }
        

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
            // Pass the same contextId this VM was constructed with (eventId in create mode, drinkId in edit mode)
            _navigation.NavigateTo<ScriptCreationWizardViewModel>(_drinkId ?? _eventId);
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
        var contents = new List<DrinkContent>();

        void AddAlcohol(Ingredient? ingredient, string dose)
        {
            if (ingredient == null) return;

            contents.Add(new DrinkContent
            {
                IngredientId = ingredient.IngredientId,
                Dose = dose
            });
        }

        void AddSingle(Ingredient? ingredient)
        {
            if (ingredient == null) return;

            contents.Add(new DrinkContent
            {
                IngredientId = ingredient.IngredientId,
                Dose = "single"
            });
        }

        // Alcohol
        AddAlcohol(SelectedAlcohol1, AlcoholDose1);
        AddAlcohol(SelectedAlcohol2, AlcoholDose2);
        AddAlcohol(SelectedAlcohol3, AlcoholDose3);
        AddAlcohol(SelectedAlcohol4, AlcoholDose4);

        // Syrups & sodas (always single)
        AddSingle(SelectedSyrup1);
        AddSingle(SelectedSyrup2);
        AddSingle(SelectedSyrup3);

        AddSingle(SelectedSoda1);
        AddSingle(SelectedSoda2);

        var scripts = string.IsNullOrWhiteSpace(ScriptText)
            ? new List<string>()
            : new List<string> { ScriptText };

        var imageToPersist = ImagePreview ?? "";
        if (IsLikelyExternalPath(imageToPersist))
        {
            imageToPersist = _imageStorageService.SaveImage(
                imageToPersist,
                "DrinkPics",
                DrinkName);

            ImagePreview = imageToPersist;
        }

        if (IsEditMode)
        {
            _drinkLogic.UpdateDrink(
                drinkId: _drinkId!.Value,
                name: DrinkName,
                image: imageToPersist,
                isMocktail: IsMocktail,
                contents: contents,
                scriptNames: scripts
            );

            _navigation.NavigateTo<KatalogViewModel>();
        }
        else
        {
            _drinkLogic.AddDrink(
                name: DrinkName,
                image: imageToPersist,
                isMocktail: IsMocktail,
                contents: contents,
                scriptNames: scripts
            );

            _navigation.NavigateTo<EventViewModel>(_eventId);
        }
    }
    catch (Exception ex)
    {
        MessageBox.Show(ex.Message, "Error");
    }
}
    }
}
