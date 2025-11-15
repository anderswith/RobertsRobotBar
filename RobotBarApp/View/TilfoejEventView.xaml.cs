using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Linq;

namespace RobotBarApp.View
{
    public partial class TilfoejEventView : UserControl
    {
        public ObservableCollection<RackSlotVm> RackSlots { get; } = new();
        public ObservableCollection<string> MenuOptions { get; } = new() { "Festival", "Standard", "Custom" };
        public string SelectedMenu { get; set; } = "Festival";
        public string EventName { get; set; } = "Fest"; // renamed from Name to avoid FrameworkElement.Name
        public string? ImagePath { get; set; }

        public ObservableCollection<IngredientVm> Ingredients { get; } = new()
        {
            new("Tequila", true){ TargetSlot = 2, BottleImage = "Images/Bottles/tequila.png" },
            new("Gin", false){ TargetSlot = 12, BottleImage = "Images/Bottles/gin.png" },
            new("Mango syrup", true){ TargetSlot = 8, BottleImage = "Images/Bottles/mango.png" },
            new("Vodka", true){ TargetSlot = 23, BottleImage = "Images/Bottles/vodka.png" },
            new("Ananas syrup", true){ TargetSlot = 6, BottleImage = "Images/Bottles/ananas.png" },
            // ... add more
        };

        public TilfoejEventView()
        {
            InitializeComponent();
            DataContext = this;

            // Build 24 rack slots
            for (int i = 1; i <= 24; i++)
                RackSlots.Add(new RackSlotVm { Number = i });

            // Initial placement based on checked ingredients
            ApplyIngredientPlacements();

            // React to check/uncheck in real-time
            foreach (var ing in Ingredients)
                ing.PropertyChanged += (_, __) => ApplyIngredientPlacements();
        }

        private void ApplyIngredientPlacements()
        {
            // Clear bottles
            foreach (var slot in RackSlots) slot.BottleImage = null;

            // Place bottles where ingredients are checked and have TargetSlot
            foreach (var ing in Ingredients)
            {
                if (ing.IsSelected && ing.TargetSlot is int idx && idx >= 1 && idx <= 24)
                {
                    RackSlots[idx - 1].BottleImage = ing.BottleImage;
                }
            }
        }

        private void ChooseImage_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp" };
            if (dlg.ShowDialog() == true)
            {
                ImagePath = dlg.FileName;
                // refresh binding
                DataContext = null;
                DataContext = this;
            }
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.ShowView(new EventListView());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            if (Window.GetWindow(this) is MainWindow mw)
                mw.ShowView(new EventListView());
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Gemte event (demo).");
            if (Window.GetWindow(this) is MainWindow mw)
                mw.ShowView(new EventListView());
        }
    }

    public class RackSlotVm : System.ComponentModel.INotifyPropertyChanged
    {
        private string? _bottleImage;
        public int Number { get; set; }
        public string? BottleImage { get => _bottleImage; set { _bottleImage = value; OnChanged(nameof(BottleImage)); } }
        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        void OnChanged(string n) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(n));
    }

    public class IngredientVm : System.ComponentModel.INotifyPropertyChanged
    {
        private bool _isSelected;
        public string Name { get; }
        public string? BottleImage { get; set; }   // path to image shown on rack
        public int? TargetSlot { get; set; }      // where this bottle should appear (1..24)

        public bool IsSelected { get => _isSelected; set { _isSelected = value; OnChanged(nameof(IsSelected)); } }

        public IngredientVm(string name, bool selected = false) { Name = name; _isSelected = selected; }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        void OnChanged(string n) => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(n));
    }
}
