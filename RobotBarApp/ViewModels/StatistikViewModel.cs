using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;
using OxyPlot;

namespace RobotBarApp.ViewModels
{
    public class StatistikViewModel : ViewModelBase
    {
        private readonly IEventLogic _eventLogic;
        private readonly IDrinkUseCountLogic _drinkLogic;
        private readonly IIngredientUseCountLogic _ingredientLogic;
        private readonly ILogLogic _logLogic;

        public StatistikViewModel(
            IEventLogic eventLogic,
            IDrinkUseCountLogic drinkLogic,
            IIngredientUseCountLogic ingredientLogic,
            ILogLogic logLogic)
        {
            _eventLogic = eventLogic;
            _drinkLogic = drinkLogic;
            _ingredientLogic = ingredientLogic;
            _logLogic = logLogic;

            LoadEvents();

            RefreshStatisticsCommand = new RelayCommand(_ => RefreshStatistics());
            // hours 0-24
            TimeOptions = new ObservableCollection<TimeSpan>(
                Enumerable.Range(0, 24).Select(h => new TimeSpan(h, 0, 0)));
        }
        

        public ObservableCollection<Event> Events { get; } = new();

        private Event _selectedEvent;
        public Event SelectedEvent
        {
            get => _selectedEvent;
            set
            {
                _selectedEvent = value;
                OnPropertyChanged();
                RefreshStatistics();
            }
        }

        public DateTime? StartDate { get; set; } 
        public DateTime? EndDate { get; set; }    

        public ObservableCollection<TimeSpan> TimeOptions { get; } 

        private TimeSpan? _startTime;
        public TimeSpan? StartTime  
        {
            get => _startTime;
            set
            {
                _startTime = value;
                OnPropertyChanged();
                RefreshStatistics();
            }
        }

        private TimeSpan? _endTime;
        public TimeSpan? EndTime   
        {
            get => _endTime;
            set
            {
                _endTime = value;
                OnPropertyChanged();
                RefreshStatistics();
            }
        }

        public ObservableCollection<(string Name, int Count)> DrinkStats { get; } = new();
        public ObservableCollection<(string Name, int Count)> IngredientStats { get; } = new();
        public ObservableCollection<Log> Logs { get; } = new();


        public PlotModel DrinkPieChart { get; set; }
        public PlotModel IngredientBarChart { get; set; }

        public ICommand RefreshStatisticsCommand { get; }


        private void LoadEvents()
        {
            Events.Clear();
            foreach (var ev in _eventLogic.GetAllEvents())
                Events.Add(ev);
        }

        private void RefreshStatistics()
        {
            if (SelectedEvent == null)
                return;

            Guid eventId = SelectedEvent.EventId;
            

            bool hasStartDate = StartDate.HasValue;
            bool hasEndDate = EndDate.HasValue;

            DateTime start = DateTime.MinValue;
            DateTime end = DateTime.MaxValue;


            if (hasStartDate)
            {
                var date = StartDate.Value.Date;
                var time = StartTime ?? TimeSpan.Zero; 
                start = date + time;
            }


            if (hasEndDate)
            {
                var date = EndDate.Value.Date;
                var time = EndTime ?? new TimeSpan(23, 59, 59);
                end = date + time;
            }


            bool useTimeFrame = hasStartDate || hasEndDate;

            if (useTimeFrame && start > end)
                return;
            
            DrinkStats.Clear();
            IngredientStats.Clear();
            Logs.Clear();

            var drinkStats = useTimeFrame
                ? _drinkLogic.GetAllDrinkUseCountByTimeFrame(eventId, start, end)
                : _drinkLogic.GetAllDrinksUseCountForEvent(eventId);

            foreach (var d in drinkStats)
                DrinkStats.Add((d.DrinkName, d.TotalUseCount));


            var ingredientStats = useTimeFrame
                ? _ingredientLogic.GetIngredientUseCountByTimeFrame(eventId, start, end)
                : _ingredientLogic.GetAllIngredientsUseCountForEvent(eventId);

            foreach (var i in ingredientStats)
                IngredientStats.Add((i.IngredientName, i.TotalUseCount));
            
            var logs = useTimeFrame
                ? _logLogic.GetLogsInTimeFrame(eventId, start, end)
                : _logLogic.GetLogsForEvent(eventId);

            foreach (var log in logs)
                Logs.Add(log);
            
        }
    }
}
