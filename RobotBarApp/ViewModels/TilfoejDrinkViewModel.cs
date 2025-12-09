using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;

namespace RobotBarApp.ViewModels
{
    public class TilfoejDrinkViewModel : ViewModelBase
    {
       
        private readonly IIngredientLogic _ingredientLogic;
        private readonly IDrinkLogic _drinkLogic;
        private readonly INavigationService _navigation;
        private readonly Guid _eventId;

        public TilfoejDrinkViewModel(
            IIngredientLogic ingredientLogic,
            IDrinkLogic drinkLogic,
            INavigationService navigation,
            Guid eventId)
        {
            _ingredientLogic = ingredientLogic;
            _drinkLogic = drinkLogic;
            _navigation = navigation;
            _eventId = eventId;

            ChooseImageCommand = new RelayCommand(_ => ChooseImage());
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
            GuideCommand = new RelayCommand(_ => ShowGuide());

            LoadIngredientLists();
        }
        private void Cancel()
        {
            _navigation.NavigateTo<MainWindowViewModel>(); 
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

        public Ingredient? SelectedAlcohol1 { get; set; }
        public Ingredient? SelectedAlcohol2 { get; set; }
        public Ingredient? SelectedAlcohol3 { get; set; }
        public Ingredient? SelectedAlcohol4 { get; set; }

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
        

        public Ingredient? SelectedSyrup1 { get; set; }
        public Ingredient? SelectedSyrup2 { get; set; }
        public Ingredient? SelectedSyrup3 { get; set; }
        

        public Ingredient? SelectedSoda1 { get; set; }
        public Ingredient? SelectedSoda2 { get; set; }
        

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

        private void Save()
        {
            Console.WriteLine("SAVE() CALLED!");
            if (string.IsNullOrWhiteSpace(DrinkName))
            {
                System.Windows.MessageBox.Show("Drinken skal have et navn.");
                return;
            }

            var ingredientIds = new List<Guid>();
            var scriptNames = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(ScriptText))
                scriptNames.Add(ScriptText);

            void AddIngredient(Ingredient? ing)
            {
                if (ing != null)
                    ingredientIds.Add(ing.IngredientId);
            }

            AddIngredient(SelectedAlcohol1);
            AddIngredient(SelectedAlcohol2);
            AddIngredient(SelectedAlcohol3);
            AddIngredient(SelectedAlcohol4);

            AddIngredient(SelectedSyrup1);
            AddIngredient(SelectedSyrup2);
            AddIngredient(SelectedSyrup3);

            AddIngredient(SelectedSoda1);
            AddIngredient(SelectedSoda2);

            _drinkLogic.AddDrink(
                DrinkName,
                ImagePreview ?? "",
                IsMocktail,
                ingredientIds,
                scriptNames
            );

            System.Windows.MessageBox.Show("Drinken blev tilføjet.");

            _navigation.NavigateTo<EventViewModel>(_eventId);
        }
    }
}
