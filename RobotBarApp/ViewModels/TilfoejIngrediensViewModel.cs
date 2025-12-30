using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.BLL.Interfaces;
using System.IO;
using System.Windows;


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

        private string _imagePreview;
        public string ImagePreview
        {
            get => _imagePreview;
            set => SetProperty(ref _imagePreview, value);
        }

        public bool IsAlkohol { get; set; }
        public bool IsMock { get; set; }
        public bool IsSyrup { get; set; }
        public bool IsSoda { get; set; }


        public ObservableCollection<int> Holders { get; } = new();

        private int _selectedHolder;
        public int SelectedHolder
        {
            get => _selectedHolder;
            set => SetProperty(ref _selectedHolder, value);
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
            

            IsAlkohol = ingredient.Type == "Alkohol";
            IsMock    = ingredient.Type == "Mock";
            IsSyrup   = ingredient.Type == "Syrup";
            IsSoda    = ingredient.Type == "Soda";
            

            SelectedHolder = ingredient.IngredientPositions
                .Select(p => p.Position)
                .FirstOrDefault();

            SingleScriptText = ingredient.SingleScripts
                .OrderBy(s => s.Number)
                .FirstOrDefault()?.UrScript ?? "";
            
            DoubleScriptText = ingredient.DoubleScripts
                .OrderBy(s => s.Number)
                .FirstOrDefault()?.UrScript ?? "";
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
            _navigation.NavigateTo<EventListViewModel>(); 
        }

        private void Save()
        {
            
            string type =
                IsAlkohol ? "Alkohol" :
                IsMock    ? "Mock" :
                IsSyrup   ? "Syrup" :
                IsSoda    ? "Soda" :
                "Ukendt";
            if (type == "Ukendt")
            {
                MessageBox.Show("Ingredient must have a type");
                return;
            }
            

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
            public string Name { get; set; }   // "Red", "Blue", etc.
            public string Hex { get; set; }    // "#FF0000"
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
