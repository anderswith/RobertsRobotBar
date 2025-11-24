using System.Collections.ObjectModel;
using System.Windows.Input;
using Microsoft.Win32;
using RobotBarApp.BE;
using RobotBarApp.Services.Interfaces;
using RobotBarApp.BLL.Interfaces;


namespace RobotBarApp.ViewModels
{
    public class TilfoejEventViewModel : ViewModelBase
    {
        private readonly INavigationService _navigation;
        private readonly IEventLogic _eventLogic;
        private readonly IMenuLogic _menuLogic;

        public ObservableCollection<Menu> Menus { get; } = new();

        private Menu _selectedMenu;
        public Menu SelectedMenu
        {
            get => _selectedMenu;
            set => SetProperty(ref _selectedMenu, value);
        }

        private string _eventName = "";
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

        // Commands
        public ICommand ChooseImageCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public TilfoejEventViewModel(
            INavigationService navigation,
            IEventLogic eventLogic,
            IMenuLogic menuLogic)
        {
            _navigation = navigation;
            _eventLogic = eventLogic;
            _menuLogic = menuLogic;

            ChooseImageCommand = new RelayCommand(_ => ChooseImage());
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());

            LoadMenus();
        }

        private void LoadMenus()
        {
            Menus.Clear();

            var menus = _menuLogic.GetAllMenus(); // fra DB
            foreach (var menu in menus)
                Menus.Add(menu);

            // Auto-select first if available
            SelectedMenu = Menus.FirstOrDefault();
        }

        private void ChooseImage()
        {
            var dlg = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp" };
            if (dlg.ShowDialog() == true)
                ImagePath = dlg.FileName;
        }

        private void Save()
        {
            if (string.IsNullOrWhiteSpace(EventName))
            {
                System.Windows.MessageBox.Show("Event navn skal udfyldes.");
                return;
            }
            if (SelectedMenu == null)
            {
                System.Windows.MessageBox.Show("Vælg en menu.");
                return;
            }

            _eventLogic.AddEvent(
                EventName,
                ImagePath,
                SelectedMenu.MenuId        
            );

            System.Windows.MessageBox.Show("Event blev tilføjet!");

            _navigation.NavigateTo<EventListViewModel>();
        }

        private void Cancel()
        {
            _navigation.NavigateTo<EventListViewModel>();
        }
    }
}
