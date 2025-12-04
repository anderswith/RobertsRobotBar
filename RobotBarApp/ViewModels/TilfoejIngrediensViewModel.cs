using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.BLL.Interfaces;
using System.Collections.Generic;
using System;
using System.IO;

namespace RobotBarApp.ViewModels
{
    public class TilfoejIngrediensViewModel : ViewModelBase
    {
        private readonly INavigationService _navigation;
        private readonly IIngredientLogic _ingredientLogic;



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

        private string _imagePreview = string.Empty;
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

        public TilfoejIngrediensViewModel(
            INavigationService navigation,
            IIngredientLogic ingredientLogic)
        {
            _navigation = navigation;
            _ingredientLogic = ingredientLogic;

            //  Position numbers 1 to 24 
            for (int i = 1; i <= 24; i++)
                Holders.Add(i);

            SelectedHolder = Holders.FirstOrDefault();

            ChooseImageCommand = new RelayCommand(_ => ChooseImage());
            GuideCommand = new RelayCommand(_ => ShowGuide());
            CancelCommand = new RelayCommand(_ => Cancel());
            SaveCommand = new RelayCommand(_ => Save());
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
            _navigation.NavigateTo<MainWindowViewModel>(); 
        }

        private void Save()
        {

            if (string.IsNullOrWhiteSpace(IngredientName))
            {
                System.Windows.MessageBox.Show("Du skal angive et navn.");
                return;
            }

            if (!double.TryParse(SizeCl, out double sizeParsed))
            {
                System.Windows.MessageBox.Show("Størrelsen (cl) skal være et tal.");
                return;
            }

            if (SelectedHolder == 0)
            {
                System.Windows.MessageBox.Show("Vælg en holder.");
                return;
            }
            
            if (string.IsNullOrWhiteSpace(ImagePreview) || !File.Exists(ImagePreview))
            {
                System.Windows.MessageBox.Show("Vælg et billede, så det kan gemmes lokalt.");
                return;
            }

            string type = "Ukendt";
            if (IsAlkohol) type = "Alkohol";
            else if (IsMock) type = "Mock";
            else if (IsSyrup) type = "Syrup";
            else if (IsSoda) type = "Soda";


            string dose = IsSingleDose ? "Single" : "Double";


            List<string> scripts = new();
            if (!string.IsNullOrWhiteSpace(ScriptText))
                scripts.Add(ScriptText);
            
            var storedImagePath = CopyImageToResources();
            
            _ingredientLogic.AddIngredient(
                name: IngredientName,
                type: type,
                image: storedImagePath,
                size: sizeParsed,
                dose: dose,
                positionNumber: SelectedHolder,
                scriptNames: scripts
            );

            System.Windows.MessageBox.Show("Ingrediens tilføjet!");

            _navigation.NavigateTo<TilfoejIngrediensViewModel>(); 
        }

        private string CopyImageToResources()
        {
            var destinationFolder = GetIngredientPicsDirectory();
            Directory.CreateDirectory(destinationFolder);

            var extension = Path.GetExtension(ImagePreview);
            var safeName = new string((IngredientName ?? "ingredient").Select(ch =>
                Path.GetInvalidFileNameChars().Contains(ch) ? '_' : ch).ToArray());
            if (string.IsNullOrWhiteSpace(safeName))
                safeName = "ingredient";

            var fileName = $"{safeName}_{DateTime.UtcNow:yyyyMMddHHmmssfff}{extension}";
            var destinationPath = Path.Combine(destinationFolder, fileName);

            File.Copy(ImagePreview, destinationPath, overwrite: true);

            return Path.GetRelativePath(AppContext.BaseDirectory, destinationPath);
        }

        private string GetIngredientPicsDirectory()
        {
            var baseDir = AppContext.BaseDirectory;
            var projectResourcePath = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "Resources", "IngredientPics"));

            try
            {
                Directory.CreateDirectory(projectResourcePath);
                return projectResourcePath;
            }
            catch
            {
                // Fall back to output directory if we cannot reach the project folder
                return Path.Combine(baseDir, "Resources", "IngredientPics");
            }
        }
    }
}
