using System;
using System.Windows;
using RobotBarApp.BLL.Interfaces;
using RobotBarApp.ViewModels;

namespace RobotBarApp.View
{
    public partial class KundeStartView : Window
    {
        public KundeStartView()
        {
            InitializeComponent();
        }

        private void Menu_Click(object sender, RoutedEventArgs e)
        {
            var menuLogic = ResolveMenuLogic();
            if (menuLogic == null)
                return;

            var menuView = new KundeMenuView(menuLogic)
            {
                DataContext = new KundeMenuViewModel(menuLogic)
            };

            menuView.BackRequested += (_, _) => ShowStartScreen();

            ShowChild(menuView);
        }

        private void MixSelv_Click(object sender, RoutedEventArgs e)
        {
            var mixSelvView = new KundeMixSelvView();
            mixSelvView.BackRequested += (_, _) => ShowStartScreen();

            ShowChild(mixSelvView);
        }

        private void ShowChild(object child)
        {
            HostContent.Content = child;
            StartRoot.Visibility = Visibility.Collapsed;
        }

        private void ShowStartScreen()
        {
            HostContent.Content = null;
            StartRoot.Visibility = Visibility.Visible;

            // make sure touch focus returns to the window
            Activate();
            Focus();
        }

        private IMenuLogic? ResolveMenuLogic()
        {
            if (DataContext is KundeStartViewModel vm)
            {
                // VM has the same instance injected; easiest is to reuse DI here, but keep this path for future.
            }

            return App.AppHost?.Services.GetService(typeof(IMenuLogic)) as IMenuLogic;
        }
    }
}