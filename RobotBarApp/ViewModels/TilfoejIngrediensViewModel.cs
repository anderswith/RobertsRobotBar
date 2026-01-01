using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.BLL.Interfaces;
using System.IO;
using System.Windows;
using RobotBarApp.BE;


namespace RobotBarApp.ViewModels
{
    public class TilfoejIngrediensViewModel : ViewModelBase
    {
        private readonly INavigationService _navigation;
        private readonly IIngredientLogic _ingredientLogic;
        private readonly IRobotLogic _robotLogic;
        private readonly IImageStorageService _imageStorageService;
        
        private readonly Guid? _ingredientId;
        public bool IsEditMode => _ingredientId.HasValue;

        public string PageTitle => IsEditMode ? "Rediger ingrediens" : "Tilføj ingrediens";

        // Holder positions (used by UI + save)
        public ObservableCollection<int> Holders { get; } = new();

        private int _selectedHolder;
        public int SelectedHolder
        {
            get => _selectedHolder;
            set => SetProperty(ref _selectedHolder, value);
        }


        private string _ingredientName = "";
        public string IngredientName
        {
            get => _ingredientName;
            set => SetProperty(ref _ingredientName, value);
        }

        private string _color = "#FFFFFF";
        public string Color
        {
            get => _color;
            set => SetProperty(ref _color, value);
        }

        private string _imagePreview = "";
        public string ImagePreview
        {
            get => _imagePreview;
            set => SetProperty(ref _imagePreview, value);
        }

        private bool _isAlkohol;
        public bool IsAlkohol
        {
            get => _isAlkohol;
            set
            {
                if (SetProperty(ref _isAlkohol, value) && value)
                    SetIngredientTypeExclusive("Alkohol");
            }
        }

        private bool _isMock;
        public bool IsMock
        {
            get => _isMock;
            set
            {
                if (SetProperty(ref _isMock, value) && value)
                    SetIngredientTypeExclusive("Mock");
            }
        }

        private bool _isSyrup;
        public bool IsSyrup
        {
            get => _isSyrup;
            set
            {
                if (SetProperty(ref _isSyrup, value) && value)
                    SetIngredientTypeExclusive("Syrup");
            }
        }

        private bool _isSoda;
        public bool IsSoda
        {
            get => _isSoda;
            set
            {
                if (SetProperty(ref _isSoda, value) && value)
                    SetIngredientTypeExclusive("Soda");
            }
        }

        // When Soda is selected, we hide the "DoubleScript" input.
        public bool ShowDoubleScript => !IsSoda;

        private void SetIngredientTypeExclusive(string type)
        {
            // Ensure only one type is selected at a time.
            // IMPORTANT: set backing fields directly to avoid recursion.
            _isAlkohol = type == "Alkohol";
            _isMock = type == "Mock";
            _isSyrup = type == "Syrup";
            _isSoda = type == "Soda";

            OnPropertyChanged(nameof(IsAlkohol));
            OnPropertyChanged(nameof(IsMock));
            OnPropertyChanged(nameof(IsSyrup));
            OnPropertyChanged(nameof(IsSoda));
            OnPropertyChanged(nameof(ShowDoubleScript));
        }


        private string _singleScript = "";
        public string SingleScriptText
        {
            get => _singleScript;
            set => SetProperty(ref _singleScript, value);
        }
        private string _doubleScript = "";
        public string DoubleScriptText
        {
            get => _doubleScript;
            set => SetProperty(ref _doubleScript, value);
        }


        public ICommand ChooseImageCommand { get; }
        public ICommand GuideCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RunTestScriptCommand { get; }

        public TilfoejIngrediensViewModel(
            INavigationService navigation,
            IIngredientLogic ingredientLogic,
            IRobotLogic robotLogic,
            IImageStorageService imageStorageService,
            Guid? ingredientId = null
            
            )
        {
            _navigation = navigation;
            _ingredientLogic = ingredientLogic;
            _robotLogic = robotLogic;
            _imageStorageService = imageStorageService;
            _ingredientId = ingredientId;
            

            //  Position numbers 1 to 24 
            for (int i = 1; i <= 24; i++)
                Holders.Add(i);

            SelectedHolder = Holders.FirstOrDefault();
            ChooseImageCommand = new RelayCommand(_ => ChooseImage());
            GuideCommand = new RelayCommand(_ => ShowGuide());
            CancelCommand = new RelayCommand(_ => Cancel());
            SaveCommand = new RelayCommand(_ => Save());
            RunTestScriptCommand = new RelayCommand(_ => RunTestScripts());
            if (IsEditMode)
            {
                LoadIngredient();
            }
        }
        private void LoadIngredient()
        {
            var ingredient = _ingredientLogic.GetIngredientById(_ingredientId!.Value);

            IngredientName = ingredient.Name;
            ImagePreview = ingredient.Image;

            // Load saved color so edit doesn't reset to default
            Color = string.IsNullOrWhiteSpace(ingredient.Color) ? Color : ingredient.Color;

            // Use the helper so we get correct UI updates (e.g., ShowDoubleScript)
            SetIngredientTypeExclusive(ingredient.Type);

            SelectedHolder = ingredient.IngredientPositions?
                                 .Select(p => p.Position)
                                 .FirstOrDefault()
                             ?? SelectedHolder;

            SingleScriptText = (ingredient.SingleScripts ?? Enumerable.Empty<SingleScript>())
                               .OrderBy(s => s.Number)
                               .FirstOrDefault()
                               ?.UrScript
                               ?? "";

            DoubleScriptText = (ingredient.DoubleScripts ?? Enumerable.Empty<DoubleScript>())
                               .OrderBy(s => s.Number)
                               .FirstOrDefault()
                               ?.UrScript
                               ?? "";

            // For Soda, we treat scripts as "one script only". If double is missing, mirror single.
            if (IsSoda && !string.IsNullOrWhiteSpace(SingleScriptText) && string.IsNullOrWhiteSpace(DoubleScriptText))
                DoubleScriptText = SingleScriptText;
        }
        

        public void RunTestScripts()
        {
            var testScripts = new List<string>
            {
                "test.urp",
                "test.urp"
            };

            _robotLogic.RunRobotScripts(testScripts);
        }
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
            _navigation.NavigateTo<ScriptCreationWizardViewModel>(ScriptCreationWizardViewModel.ScriptGuideReturnTarget.Ingredient);
        }

        private void Cancel()
        {
            if (IsEditMode)
            {
                _navigation.NavigateTo<KatalogViewModel>();
            }
            else
            {
                _navigation.NavigateTo<EventListViewModel>();
            }
        }

        private void Save()
        {
            string type =
                IsAlkohol ? "Alkohol" :
                IsMock ? "Mock" :
                IsSyrup ? "Syrup" :
                IsSoda ? "Soda" :
                "Ukendt";

            if (type == "Ukendt")
            {
                MessageBox.Show("Ingredient must have a type");
                return;
            }

            // Soda only has one script in the UI. To satisfy the rest of the system,
            // copy single -> double right before validation/persist.
            if (type == "Soda")
                DoubleScriptText = SingleScriptText;

            if (string.IsNullOrWhiteSpace(SingleScriptText) && string.IsNullOrWhiteSpace(DoubleScriptText))
            {
                MessageBox.Show("At least one script is required");
                return;
            }

            if (!string.IsNullOrWhiteSpace(SingleScriptText) &&
                !SingleScriptText.EndsWith(".urp", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Single script must end with .urp");
                return;
            }

            if (!string.IsNullOrWhiteSpace(DoubleScriptText) &&
                !DoubleScriptText.EndsWith(".urp", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Double script must end with .urp");
                return;
            }

            if (SingleScriptText.Any(c => c == 'æ' || c == 'ø' || c == 'å') || DoubleScriptText.Any(c => c == 'æ' || c == 'ø' || c == 'å'))
            {
                MessageBox.Show("Script cannot contain æ, ø or å");
                return;
            }
            
            var singleScripts = new List<string>();
            if (!string.IsNullOrWhiteSpace(SingleScriptText))
            {
                singleScripts.Add(SingleScriptText);
            }
            var doubleScripts = new List<string>();
            if (!string.IsNullOrWhiteSpace(DoubleScriptText))
            {
                doubleScripts.Add(DoubleScriptText);
            }
                

            try
            {
                var imagePath = IsEditMode
                    ? ImagePreview
                    : _imageStorageService.SaveImage(
                        ImagePreview,
                        "IngredientPics",
                        IngredientName);

                if (IsEditMode)
                {
                    _ingredientLogic.UpdateIngredient(
                        ingredientId: _ingredientId!.Value,
                        name: IngredientName,
                        type: type,
                        image: imagePath,
                        color: Color,
                        positionNumber: SelectedHolder,
                        singleScriptNames: singleScripts,
                        doubleScriptNames: doubleScripts
                    );
                }
                else
                {
                    _ingredientLogic.AddIngredient(
                        name: IngredientName,
                        type: type,
                        image: imagePath,
                        color: Color,
                        positionNumber: SelectedHolder,
                        singleScriptNames: singleScripts,
                        doubleScriptNames: doubleScripts
                    );
                }

                MessageBox.Show(IsEditMode ? "Ingrediens opdateret!" : "Ingrediens tilføjet!");

                if (IsEditMode)
                {
                    _navigation.NavigateTo<KatalogViewModel>();
                }
                else
                {
                    _navigation.NavigateTo<TilfoejIngrediensViewModel>();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public class ColorOption
        {
            public string Name { get; set; } = "";   // "Red", "Blue", etc.
            public string Hex { get; set; } = "";    // "#FF0000"
        }
        public ObservableCollection<ColorOption> AvailableColors { get; } =
            new()
            {
                new ColorOption { Name = "Red",     Hex = "#FF0000" },
                new ColorOption { Name = "Green",   Hex = "#00FF00" },
                new ColorOption { Name = "Blue",    Hex = "#0000FF" },
                new ColorOption { Name = "Yellow",  Hex = "#FFFF00" },
                new ColorOption { Name = "Purple",  Hex = "#FF00FF" },
                new ColorOption { Name = "Cyan",    Hex = "#00FFFF" },
                new ColorOption { Name = "Pink",    Hex = "#FF69B4" }, 
                new ColorOption { Name = "Orange",  Hex = "#FFA500" },
                new ColorOption { Name = "White",   Hex = "#FFFFFF" },
                new ColorOption { Name = "Black",   Hex = "#000000" },
                new ColorOption { Name = "Transparent", Hex = "#00FFFFFF" },
            };
        

    }
}
