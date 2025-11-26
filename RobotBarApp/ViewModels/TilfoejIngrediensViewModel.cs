using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.BLL.Interfaces;
using System.Collections.Generic;
using RobotBarApp.BLL;

namespace RobotBarApp.ViewModels
{
    public class TilfoejIngrediensViewModel : ViewModelBase
    {
        private readonly INavigationService _navigation;
        private readonly IIngredientLogic _ingredientLogic;
        private readonly IRobotLogic _robotLogic;



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
            IRobotLogic robotLogic
            )
        {
            _navigation = navigation;
            _ingredientLogic = ingredientLogic;
            _robotLogic = robotLogic;

            //  Position numbers 1 to 24 
            for (int i = 1; i <= 24; i++)
                Holders.Add(i);

            SelectedHolder = Holders.FirstOrDefault();

            ChooseImageCommand = new RelayCommand(_ => ChooseImage());
            GuideCommand = new RelayCommand(_ => ShowGuide());
            CancelCommand = new RelayCommand(_ => Cancel());
            SaveCommand = new RelayCommand(_ => Save());
            RunTestScriptCommand = new RelayCommand(_ => RunTestScripts());
        }

        public void RunTestScripts()
        {
            var testScripts = new List<string>
            {
                "test_script1.urp",
                "test_script2.urp"
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

            string type = "Ukendt";
            if (IsAlkohol) type = "Alkohol";
            else if (IsMock) type = "Mock";
            else if (IsSyrup) type = "Syrup";
            else if (IsSoda) type = "Soda";


            string dose = IsSingleDose ? "Single" : "Double";


            List<string> scripts = new();
            if (!string.IsNullOrWhiteSpace(ScriptText))
                scripts.Add(ScriptText);
            
            _ingredientLogic.AddIngredient(
                name: IngredientName,
                type: type,
                image: ImagePreview,
                size: sizeParsed,
                dose: dose,
                positionNumber: SelectedHolder,
                scriptNames: scripts
            );

            System.Windows.MessageBox.Show("Ingrediens tilføjet!");

            _navigation.NavigateTo<TilfoejIngrediensViewModel>(); 
        }
    }
}
