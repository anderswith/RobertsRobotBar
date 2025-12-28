using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Microsoft.Win32;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
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
        private void GoNextStep()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(EventName))
                    throw new Exception("You have to enter a name");

                if (string.IsNullOrWhiteSpace(ImagePath))
                    throw new Exception("You have to select an image");
                
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

        private void SelectSlot(object obj)
        {
            if (obj is not RackSlot slot) return;
            _selectedSlot = slot;

            FilteredIngredients.Clear();

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

                if (!ImagePath.StartsWith("Resources/"))
                {
                    finalImagePath = _imageStorageService.SaveImage(
                        ImagePath,
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
