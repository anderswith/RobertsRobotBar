using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.Commands;
using RobotBarApp.BE;

namespace RobotBarApp.ViewModels
{
    public class StatistikViewModel : ViewModelBase
    {
        private readonly IDrinkUseCountLogic _drinkLogic;
        private readonly IIngredientUseCountLogic _ingredientLogic;
        private readonly ILogLogic _logLogic;
        private readonly IEventLogic _eventLogic;

        // Selected filters
        public Event SelectedEvent { get; set; }
        public DateTime FromDate { get; set; } = DateTime.Now.AddDays(-7);
        public DateTime ToDate { get; set; } = DateTime.Now;

        // Dropdown source
        public ObservableCollection<Event> Events { get; } = new();

        // Graphs
        public PlotModel DrinkPieChart { get; private set; }
        public PlotModel IngredientBarChart { get; private set; }

        // Logs
        public ObservableCollection<LogEntry> Logs { get; } = new();

        // Command
        public ICommand RefreshCommand { get; }

        public StatistikViewModel(
            IDrinkUseCountLogic drinkLogic,
            IIngredientUseCountLogic ingredientLogic,
            IEventLogic eventLogic,
            ILogLogic logLogic)
        {
            _drinkLogic = drinkLogic;
            _ingredientLogic = ingredientLogic;
            _eventLogic = eventLogic;
            _logLogic = logLogic;

            RefreshCommand = new RelayCommand(_ => LoadStatistics());

            LoadEvents();
            LoadStatistics();
        }

        private void LoadEvents()
        {
            Events.Clear();
            foreach (var ev in _eventLogic.GetAllEvents())
                Events.Add(ev);

            if (SelectedEvent == null && Events.Count > 0)
                SelectedEvent = Events.First();
        }

        private void LoadStatistics()
        {
            if (SelectedEvent == null)
                return;

            // =============================
            // FILTERED DRINK DATA
            // =============================
            var drinkData = _drinkLogic
                .GetDrinkUseCountsForEvent(SelectedEvent.EventId, FromDate, ToDate)
                .GroupBy(x => x.Drink.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            DrinkPieChart = new PlotModel { Title = $"Drink brug – {SelectedEvent.Name}" };

            var pie = new PieSeries
            {
                StrokeThickness = 2,
                AngleSpan = 360,
                StartAngle = 0,
                OutsideLabelFormat = "{1}: {0}",
                InsideLabelFormat = ""
            };

            foreach (var d in drinkData)
                pie.Slices.Add(new PieSlice(d.Name, d.Count));

            DrinkPieChart.Series.Clear();
            DrinkPieChart.Series.Add(pie);


            // =============================
            // FILTERED INGREDIENT DATA
            // =============================
            var ingredientData = _ingredientLogic
                .GetIngredientUseCountsForEvent(SelectedEvent.EventId, FromDate, ToDate)
                .GroupBy(i => i.Ingredient.Name)
                .Select(g => new { Name = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .ToList();

            IngredientBarChart = new PlotModel { Title = "Ingrediens brug" };

            // X = brug count, Y = ingredienser
            var categoryAxis = new CategoryAxis { Position = AxisPosition.Left };
            categoryAxis.Labels.AddRange(ingredientData.Select(i => i.Name));

            var valueAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Minimum = 0
            };

            var barSeries = new BarSeries();
            barSeries.Items.AddRange(ingredientData.Select(i => new BarItem(i.Count)));

            IngredientBarChart.Axes.Add(categoryAxis);
            IngredientBarChart.Axes.Add(valueAxis);
            IngredientBarChart.Series.Add(barSeries);


            // =============================
            // LOG DATA
            // =============================
            Logs.Clear();
            foreach (var log in _logLogic.GetLogsForEvent(SelectedEvent.EventId, FromDate, ToDate))
                Logs.Add(new LogEntry(log.Timestamp, log.Message));

            // 🔔 Notify UI
            OnPropertyChanged(nameof(DrinkPieChart));
            OnPropertyChanged(nameof(IngredientBarChart));
            OnPropertyChanged(nameof(Logs));
        }
    }

    public record LogEntry(DateTime Timestamp, string Message);
}
