using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.Services.UI;
using System.Windows;

namespace RobotBarApp.ViewModels
{
    public class TilfoejEventViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        private readonly IIngredientLogic _ingredientLogic;
        private readonly IEventLogic _eventLogic;
        private readonly IBarSetupLogic _barSetupLogic;
        private readonly IImageStorageService _imageStorageService;
        
        private readonly Guid? _eventId;
        public bool IsEditMode => _eventId.HasValue;
        public TilfoejEventViewModel(
            INavigationService navigationService,
            IIngredientLogic ingredientLogic,
            IEventLogic eventLogic,
            IBarSetupLogic barSetupLogic,
            IImageStorageService imageStorageService,
            Guid contextId)
        {
            _imageStorageService = imageStorageService;
            _navigationService = navigationService;
            _ingredientLogic = ingredientLogic;
            _eventLogic = eventLogic;
            _barSetupLogic = barSetupLogic;
            
            
            
            Step = 1;

            Ingredients = new ObservableCollection<Ingredient>(_ingredientLogic.GetIngredientsForPositions());
            FilteredIngredients = new ObservableCollection<Ingredient>();

            RackItems = new ObservableCollection<RackSlot>(
                Enumerable.Range(1, 24).Select(i => new RackSlot(i)));

            SelectSlotCommand = new RelayCommand(SelectSlot);
            AddIngredientCommand = new RelayCommand(AddIngredient);
            SaveCommand = new RelayCommand(SaveEvent);
            NextCommand = new RelayCommand(_ => GoNextStep());
            CancelCommand = new RelayCommand(_ => NavigateAfterClose());
            
            var ev = _eventLogic.GetEventById(contextId);
            if (ev != null)
            {
                // EDIT
                _eventId = contextId;
                LoadEvent(ev);
            }
            else
            {
                // CREATE
                _eventId = null;
            }

        }
        private bool EnsureImageOnCreate()
        {
            if (IsEditMode)
                return true;

            if (!string.IsNullOrWhiteSpace(ImagePath))
                return true;

            var result = MessageBox.Show(
                "Du har ikke valgt et billede til eventet.\n\n" +
                "Ja: Vælg billede\n" +
                "Nej: Brug standardbillede\n" +
                "Annuller: Annullér",
                "Mangler billede",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            switch (result)
            {
                case MessageBoxResult.Yes:
                    // inline choose image (same as ChooseImageCommand)
                    var dialog = new OpenFileDialog();
                    if (dialog.ShowDialog() == true)
                        ImagePath = dialog.FileName;
                    return !string.IsNullOrWhiteSpace(ImagePath);

                case MessageBoxResult.No:
                    ImagePath = DefaultImagePaths.Event;
                    return true;

                default:
                    return false;
            }
        }

        private void GoNextStep()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EventName))
                    throw new Exception("You have to enter a name");

                // Only enforce image selection in create mode.
                if (!EnsureImageOnCreate())
                    return;

                Step = 2;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public RelayCommand SelectSlotCommand { get; }
        public RelayCommand AddIngredientCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand NextCommand { get; }
        public RelayCommand CancelCommand { get; }

        public ObservableCollection<Ingredient> Ingredients { get; }
        public ObservableCollection<Ingredient> FilteredIngredients { get; }
        public ObservableCollection<RackSlot> RackItems { get; }

        private RackSlot? _selectedSlot;
        private Ingredient EmptyIngredient => new Ingredient
        {
            IngredientId = Guid.Empty,
            Name = "Tom",
            Image = null
        };

        private void SelectSlot(object obj)
        {
            if (obj is not RackSlot slot) return;
            _selectedSlot = slot;

            FilteredIngredients.Clear();
            FilteredIngredients.Add(EmptyIngredient);

            foreach (var ing in Ingredients.Where(i =>
                i.IngredientPositions.Any(ip => ip.Position == slot.Position)))
            {
                FilteredIngredients.Add(ing);
            }
        }
      
        private void AddIngredient(object param)
        {
            if (_selectedSlot == null) return;
            if (param is not Ingredient ing) return;
            if (ing.IngredientId == Guid.Empty)
            {
                _selectedSlot.Ingredient = null;
                return;
            }

            _selectedSlot.Ingredient = ing;
        }

        // step 1
        private int _step;
        public int Step
        {
            get => _step;
            set => SetProperty(ref _step, value);
        }

        private string _eventName;
        public string EventName
        {
            get => _eventName;
            set => SetProperty(ref _eventName, value);
        }

        private string _imagePath;
        public string ImagePath
        {
            get => _imagePath;
            set => SetProperty(ref _imagePath, value);
        }

        public RelayCommand ChooseImageCommand => new(_ =>
        {
            var dialog = new OpenFileDialog();
            if (dialog.ShowDialog() == true)
                ImagePath = dialog.FileName;
        });
        
        private void LoadEvent(Event ev)
        {


            EventName = ev.Name;
            ImagePath = ev.Image;

            foreach (var bs in _barSetupLogic.GetBarSetupForEvent(_eventId.Value))
            {
                var slot = RackItems.FirstOrDefault(r => r.Position == bs.PositionNumber);
                if (slot != null)
                    slot.Ingredient = bs.Ingredient;
            }
        }

        // SAVE
        private void SaveEvent(object _)
        {
            try
            {
                var finalImagePath = ImagePath;

                if (!EnsureImageOnCreate())
                    return;

                // Copy only if it's an external path (not already a Resources/ path)
                if (string.IsNullOrWhiteSpace(finalImagePath))
                {
                    finalImagePath = DefaultImagePaths.Event;
                }
                else if (!finalImagePath.StartsWith("Resources/", StringComparison.OrdinalIgnoreCase))
                {
                    finalImagePath = _imageStorageService.SaveImage(
                        finalImagePath,
                        "EventPics",
                        EventName);
                }

                if (!IsEditMode)
                {
                    // CREATE
                    var newEventId = _eventLogic.AddEvent(EventName, finalImagePath);

                    foreach (var slot in RackItems.Where(r => r.Ingredient != null))
                    {
                        _barSetupLogic.AddBarSetup(
                            slot.Position,
                            slot.Ingredient.IngredientId,
                            newEventId);
                    }
                }
                else
                {
                    // UPDATE EVENT CORE DATA
                    var ev = _eventLogic.GetEventById(_eventId!.Value)
                             ?? throw new Exception("Event not found");

                    _eventLogic.UpdateEvent(
                        _eventId.Value,
                        EventName,
                        finalImagePath,
                        ev.MenuId!.Value
                    );

                    // EXISTING bar setup from DB
                    var existingSetups = _barSetupLogic
                        .GetBarSetupForEvent(_eventId.Value)
                        .ToDictionary(bs => bs.PositionNumber);

                    // LOOP ALL RACK POSITIONS (1–24)
                    foreach (var slot in RackItems)
                    {
                        var hasDbSetup = existingSetups.TryGetValue(slot.Position, out var dbSetup);

                        if (slot.Ingredient == null)
                        {
                            // UI removed ingredient → delete if existed
                            if (hasDbSetup)
                            {
                                _barSetupLogic.DeleteBarSetup(_eventId.Value, slot.Position);
                            }
                        }
                        else
                        {
                            // UI has ingredient → add or update
                            _barSetupLogic.AddBarSetup(
                                slot.Position,
                                slot.Ingredient.IngredientId,
                                _eventId.Value
                            );
                        }
                    }
                }

                NavigateAfterClose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Fejl", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        
        private void NavigateAfterClose()
        {
            if (IsEditMode)
                _navigationService.NavigateTo<KatalogViewModel>();
            else
                _navigationService.NavigateTo<EventListViewModel>();
        }

    }


}
