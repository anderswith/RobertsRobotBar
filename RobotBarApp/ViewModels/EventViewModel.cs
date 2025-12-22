using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Application.Interfaces;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.View; // <-- add this

namespace RobotBarApp.ViewModels
{
    public class EventViewModel : ViewModelBase
    {
        private readonly IEventLogic _eventLogic;
        private readonly IBarSetupLogic _barSetupLogic;
        private readonly INavigationService _navigation;
        private readonly IDrinkAvailabilityService _drinkAvailabilityService;
        private readonly IMenuLogic _menuLogic;
        private readonly IEventSessionService _eventSessionService;

        public Guid EventId { get; }
        public Event CurrentEvent { get; private set; }

        // RACK DISPLAY
        public ObservableCollection<RackSlot> Rack { get; } = new();

        // DRINK LISTS
        public ObservableCollection<Drink> AvailableDrinks { get; } = new();
        public ObservableCollection<Drink> MenuDrinks { get; } = new();

        // SELECTED ITEMS (bound from ListBox via Attached Behavior)
        public ObservableCollection<Drink> SelectedAvailable { get; } = new();
        public ObservableCollection<Drink> SelectedMenu { get; } = new();

        // Commands
        public ICommand BackCommand { get; }
        public ICommand LaunchCommand { get; }
        public ICommand AddToMenuCommand { get; }
        public ICommand RemoveFromMenuCommand { get; }
        public ICommand NavigateTilfoejDrinkCommand { get; }

        public EventViewModel(
            Guid eventId,
            IEventLogic eventLogic,
            IBarSetupLogic barSetupLogic,
            IMenuLogic menuLogic,
            IDrinkAvailabilityService drinkAvailabilityService,
            INavigationService navigation,
            IEventSessionService eventSessionService)
        {
            EventId = eventId;
            _eventLogic = eventLogic;
            _barSetupLogic = barSetupLogic;
            _menuLogic = menuLogic;
            _navigation = navigation;
            _drinkAvailabilityService = drinkAvailabilityService;
            _eventSessionService = eventSessionService;

            LoadEvent();
            LoadRack();
            LoadDrinkLists();

            BackCommand = new RelayCommand(_ => _navigation.NavigateTo<EventListViewModel>());
            LaunchCommand = new RelayCommand(_ => Launch());
            NavigateTilfoejDrinkCommand = new RelayCommand(_ => _navigation.NavigateTo<TilfoejDrinkViewModel>(EventId));

            AddToMenuCommand = new RelayCommand(_ => AddToMenu(), _ => SelectedAvailable.Any());
            RemoveFromMenuCommand = new RelayCommand(_ => RemoveFromMenu(), _ => SelectedMenu.Any());
        }

        private void LoadEvent()
        {
            CurrentEvent = _eventLogic.GetEventById(EventId);
        }

        private void LoadRack()
        {
            Rack.Clear();
            foreach (var i in Enumerable.Range(1, 24))
                Rack.Add(new RackSlot(i));

            foreach (var bs in _barSetupLogic.GetBarSetupForEvent(EventId))
            {
                var slot = Rack.First(r => r.Position == bs.PositionNumber);
                slot.Ingredient = bs.Ingredient;
            }
        }

        private void LoadDrinkLists()
        {
            AvailableDrinks.Clear();
            MenuDrinks.Clear();

            foreach (var drink in _drinkAvailabilityService.GetAvailableDrinksForEvent(EventId))
                AvailableDrinks.Add(drink);

            foreach (var drink in _menuLogic.GetDrinksForMenu(EventId))
                MenuDrinks.Add(drink);

            // Remove drinks already on menu from available list
            foreach (var menuDrink in MenuDrinks.ToList())
            {
                var match = AvailableDrinks.FirstOrDefault(d => d.DrinkId == menuDrink.DrinkId);
                if (match != null)
                    AvailableDrinks.Remove(match);
            }
        }

        private void AddToMenu()
        {
            var drinkIds = SelectedAvailable.Select(d => d.DrinkId).ToList();

            // Add to database
            _menuLogic.AddDrinksToMenu(drinkIds, EventId);

            // Update lists
            foreach (var drink in SelectedAvailable.ToList())
            {
                AvailableDrinks.Remove(drink);
                MenuDrinks.Add(drink);
            }
            SelectedAvailable.Clear();
        }

        private void RemoveFromMenu()
        {
            foreach (var drink in SelectedMenu.ToList())
            {
                _menuLogic.RemoveDrinkFromMenu(EventId, drink.DrinkId);

                MenuDrinks.Remove(drink);
                AvailableDrinks.Add(drink);
            }
            SelectedMenu.Clear();
        }

        private void Launch()
        {
            // Open the customer-facing full-screen window
            var kundeStartViewModel = new KundeStartViewModel();
            var kundeStartWindow = new KundeStartView
            {
                DataContext = kundeStartViewModel
            };
            _eventSessionService.StartEvent(EventId);

            kundeStartWindow.Show();
        }
    }
}
