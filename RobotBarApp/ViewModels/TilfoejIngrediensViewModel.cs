using System.Collections.ObjectModel;
using System.Windows.Input;

namespace RobotBarApp.ViewModels
{
    public class TilfoejIngrediensViewModel : ViewModelBase
    {
        private string _ingredientName = string.Empty;
        private string _sizeCl = string.Empty;
        private string? _imagePreview;
        private bool _isAlkohol;
        private bool _isMock;
        private bool _isSyrup;
        private bool _isSoda;
        private string _scriptText = string.Empty;
        private string? _selectedHolder;

        public string IngredientName
        {
            get => _ingredientName;
            set => SetProperty(ref _ingredientName, value);
        }

        public string SizeCl
        {
            get => _sizeCl;
            set => SetProperty(ref _sizeCl, value);
        }

        public string? ImagePreview
        {
            get => _imagePreview;
            set => SetProperty(ref _imagePreview, value);
        }

        public bool IsAlkohol
        {
            get => _isAlkohol;
            set => SetProperty(ref _isAlkohol, value);
        }

        public bool IsMock
        {
            get => _isMock;
            set => SetProperty(ref _isMock, value);
        }

        public bool IsSyrup
        {
            get => _isSyrup;
            set => SetProperty(ref _isSyrup, value);
        }

        public bool IsSoda
        {
            get => _isSoda;
            set => SetProperty(ref _isSoda, value);
        }

        public ObservableCollection<string> Holders { get; } = new();

        public string? SelectedHolder
        {
            get => _selectedHolder;
            set => SetProperty(ref _selectedHolder, value);
        }

        public bool IsSingleDose { get; set; } = true;
        public bool IsDoubleDose { get; set; }

        public string ScriptText
        {
            get => _scriptText;
            set => SetProperty(ref _scriptText, value);
        }

        public ICommand ChooseImageCommand { get; }
        public ICommand GuideCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand SaveCommand { get; }

        public TilfoejIngrediensViewModel()
        {
            // Sample holders; replace with real data later
            Holders.Add("Holder 1");
            Holders.Add("Holder 2");
            Holders.Add("Holder 3");

            ChooseImageCommand = new RelayCommand(_ => { /* TODO: open file dialog */ });
            GuideCommand = new RelayCommand(_ => { /* TODO: show guide */ });
            CancelCommand = new RelayCommand(_ => { /* TODO: navigate back */ });
            SaveCommand = new RelayCommand(_ => { /* TODO: save ingredient */ });
        }
    }
}
