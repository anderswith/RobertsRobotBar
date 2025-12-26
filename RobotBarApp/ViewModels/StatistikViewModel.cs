using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using LiveCharts;
using LiveCharts.Wpf;
using RobotBarApp.BE;
using RobotBarApp.BLL.Interfaces;


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
            
            DrinkPieSeries = new SeriesCollection();
            IngredientBarSeries = new SeriesCollection();
            XAxes = new AxesCollection
            {
                new Axis
                {
                    Labels = Array.Empty<string>()
                }
            };

            YAxes = new AxesCollection
            {
                new Axis
                {
                    Title = "Uses",
                    MinValue = 0
                }
            };

            

            RefreshStatisticsCommand = new RelayCommand(_ => RefreshStatistics());

            TimeOptions = new ObservableCollection<TimeSpan>(
                Enumerable.Range(0, 24).Select(h => new TimeSpan(h, 0, 0)));
            
            LoadEvents();
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

        public SeriesCollection DrinkPieSeries { get; }
        public SeriesCollection IngredientBarSeries { get; }

        public AxesCollection XAxes { get; }
        public AxesCollection YAxes { get; }
        public ObservableCollection<(string Name, int Count)> DrinkStats { get; } = new();
        public ObservableCollection<(string Name, int Count)> IngredientStats { get; } = new();
        public ObservableCollection<Log> Logs { get; } = new();

        

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

            try
            {
                Guid eventId = SelectedEvent.EventId;

                bool hasStartDate = StartDate.HasValue;
                bool hasEndDate = EndDate.HasValue;
                bool useTimeFrame = hasStartDate || hasEndDate;

                DateTime start = DateTime.MinValue;
                DateTime end = DateTime.MaxValue;

                if (hasStartDate)
                    start = StartDate.Value.Date + (StartTime ?? TimeSpan.Zero);

                if (hasEndDate)
                    end = EndDate.Value.Date + (EndTime ?? new TimeSpan(23, 59, 59));

                if (useTimeFrame && start >= end)
                    throw new ArgumentException("Start date/time must be before end date/time.");

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
                
                BuildCharts();

                bool hasAnyData =
                    DrinkStats.Any() ||
                    IngredientStats.Any() ||
                    Logs.Any();

                if (!hasAnyData)
                {
                    string message = useTimeFrame
                        ? "No statistics were found in the selected time frame."
                        : "No statistics were found for the selected event.";

                    MessageBox.Show(
                        message,
                        "No data",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (ArgumentException ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Invalid input",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "An unexpected error occurred while loading statistics.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
        private void BuildCharts()
        {
            // PIE — Drinks
            DrinkPieSeries.Clear();

            foreach (var d in DrinkStats)
            {
                DrinkPieSeries.Add(new PieSeries
                {
                    Title = d.Name,
                    Values = new ChartValues<int> { d.Count },
                    DataLabels = true
                });
            }

            // COLUMN — Ingredients
            IngredientBarSeries.Clear();

            IngredientBarSeries.Add(new ColumnSeries
            {
                Title = "Ingredients",
                Values = new ChartValues<int>(
                    IngredientStats.Select(i => i.Count)),
                DataLabels = true
            });

            // 🔑 SAFE axis label update (no crashes)
            XAxes[0].Labels = IngredientStats
                .Select(i => i.Name)
                .ToArray();
        }

    }
}
