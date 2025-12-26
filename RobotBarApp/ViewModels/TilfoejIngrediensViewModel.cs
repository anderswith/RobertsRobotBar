using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.BLL.Interfaces;
using System.Collections.Generic;
using RobotBarApp.BLL;
using System;
using System.IO;
using System.Windows;

namespace RobotBarApp.ViewModels
{
    public class TilfoejIngrediensViewModel : ViewModelBase
    {
        private readonly INavigationService _navigation;
        private readonly IIngredientLogic _ingredientLogic;
        private readonly IRobotLogic _robotLogic;
        
        private readonly Guid? _ingredientId;
        public bool IsEditMode => _ingredientId.HasValue;


        private string _ingredientName = "";
        public string IngredientName
        {
            get => _ingredientName;
            set => SetProperty(ref _ingredientName, value);
        }

        private string _sizeCl = "";
        public string SizeCl
        {
            get => _sizeCl;
            set => SetProperty(ref _sizeCl, value);
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

        public bool IsSingleDose { get; set; } = true;
        public bool IsDoubleDose { get; set; }

        private string _scriptText = "";
        public string ScriptText
        {
            get => _scriptText;
            set => SetProperty(ref _scriptText, value);
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
            Guid? ingredientId = null
            )
        {
            _navigation = navigation;
            _ingredientLogic = ingredientLogic;
            _robotLogic = robotLogic;
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
            SizeCl = ingredient.Size.ToString();
            ImagePreview = ingredient.Image;

            IsAlkohol = ingredient.Type == "Alkohol";
            IsMock    = ingredient.Type == "Mock";
            IsSyrup   = ingredient.Type == "Syrup";
            IsSoda    = ingredient.Type == "Soda";

            IsSingleDose = ingredient.Dose == "Single";
            IsDoubleDose = ingredient.Dose == "Double";

            SelectedHolder = ingredient.IngredientPositions
                .Select(p => p.Position)
                .FirstOrDefault();

            ScriptText = ingredient.IngredientScripts
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
            System.Windows.MessageBox.Show("Guide kommer senere :)");
        }

        private void Cancel()
        {
            _navigation.NavigateTo<EventListViewModel>(); 
        }

        private void Save()
        {
            if (!double.TryParse(SizeCl, out double sizeParsed))
            {
                MessageBox.Show("Size (cl) has to be a valid number.");
                return;
            }

            string type =
                IsAlkohol ? "Alkohol" :
                IsMock    ? "Mock" :
                IsSyrup   ? "Syrup" :
                IsSoda    ? "Soda" :
                "Ukendt";

            string dose = IsSingleDose ? "Single" : "Double";

            var scripts = new List<string>();
            if (!string.IsNullOrWhiteSpace(ScriptText))
                scripts.Add(ScriptText);

            try
            {
                var imagePath = IsEditMode
                    ? ImagePreview
                    : CopyImageToResources();

                if (IsEditMode)
                {
                    _ingredientLogic.UpdateIngredient(
                        ingredientId: _ingredientId!.Value,
                        name: IngredientName,
                        type: type,
                        image: imagePath,
                        size: sizeParsed,
                        dose: dose,
                        positionNumber: SelectedHolder,
                        scriptNames: scripts
                    );
                }
                else
                {
                    _ingredientLogic.AddIngredient(
                        name: IngredientName,
                        type: type,
                        image: imagePath,
                        size: sizeParsed,
                        dose: dose,
                        positionNumber: SelectedHolder,
                        scriptNames: scripts
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

        private string CopyImageToResources()
        {
            var projectRoot = Directory.GetParent(AppContext.BaseDirectory)
                .Parent.Parent.Parent.FullName;

            var destinationFolder = Path.Combine(projectRoot, "Resources", "IngredientPics");
            Directory.CreateDirectory(destinationFolder);

            var extension = Path.GetExtension(ImagePreview);
            var safeName = new string((IngredientName ?? "ingredient")
                .Select(ch => Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch).ToArray());

            var fileName = $"{safeName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            var destinationPath = Path.Combine(destinationFolder, fileName);

            File.Copy(ImagePreview, destinationPath, overwrite: true);

            return Path.Combine("Resources", "IngredientPics", fileName).Replace("\\", "/");
        }


        

    }
}
