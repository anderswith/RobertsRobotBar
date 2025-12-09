using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;


namespace RobotBarApp.ViewModels
{
    public class EventViewModel : ViewModelBase
    {
        private readonly IEventLogic _eventLogic;
        private readonly IBarSetupLogic _barSetupLogic;
        private readonly INavigationService _navigation;

        public Guid EventId { get; }

        public Event CurrentEvent { get; private set; }

        public ObservableCollection<RackSlot> Rack { get; }

        public ICommand BackCommand { get; }
        public ICommand LaunchCommand { get; }
        public ICommand NavigateTilfoejDrinkCommand { get; }

        public EventViewModel(
            Guid eventId,
            IEventLogic eventLogic,
            IBarSetupLogic barSetupLogic,
            INavigationService navigation)
        {
            EventId = eventId;
            _eventLogic = eventLogic;
            _barSetupLogic = barSetupLogic;
            _navigation = navigation;

            CurrentEvent = _eventLogic.GetEventById(eventId);

            Rack = new ObservableCollection<RackSlot>(
                Enumerable.Range(1, 24).Select(i => new RackSlot(i)));

            foreach (var bs in _barSetupLogic.GetBarSetupForEvent(eventId))
            {
                Debug.WriteLine($"Slot:{bs.PositionNumber}  ->  {bs.Ingredient.Image}");
                var slot = Rack.First(r => r.Position == bs.PositionNumber);
                slot.Ingredient = bs.Ingredient;
            }

            BackCommand = new RelayCommand(_ => _navigation.NavigateTo<EventListViewModel>());
            LaunchCommand = new RelayCommand(_ => Launch());
            NavigateTilfoejDrinkCommand = new RelayCommand(_ => _navigation.NavigateTo<TilfoejDrinkViewModel>(EventId));
        }

        private void Launch()
        {
        }
        
        
        
    }
}