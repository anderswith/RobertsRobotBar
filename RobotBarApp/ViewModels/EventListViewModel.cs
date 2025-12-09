using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.ViewModels;

public class EventListViewModel : ViewModelBase
{
    private readonly IEventLogic _eventLogic;
    private readonly INavigationService _navigation;

    public ObservableCollection<Event> Events { get; }

    public ICommand SelectEventCommand { get; }

    public EventListViewModel(
        IEventLogic eventLogic,
        INavigationService navigation)
    {
        _eventLogic = eventLogic;
        _navigation = navigation;

        Events = new ObservableCollection<Event>(_eventLogic.GetAllEvents());

        SelectEventCommand = new RelayCommand(evt =>
        {
            if (evt is Event e)
            {
                MessageBox.Show($"You clicked {e.Name}");
            }
        });
    }
}