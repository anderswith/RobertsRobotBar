using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Microsoft.EntityFrameworkCore;
using RobotBarApp.DAL;

namespace RobotBarApp.View
{
    public partial class EventListView : UserControl
    {
        public EventListView()
        {
            InitializeComponent();
            LoadEvents();
        }

        private void LoadEvents()
        {
            using var context = new RobotBarContext();
            var events = context.Events.AsNoTracking().OrderBy(e => e.Name).ToList();
            if (events.Count == 0)
            {
                EmptyState.Visibility = Visibility.Visible;
                EventItems.ItemsSource = null;
            }
            else
            {
                EmptyState.Visibility = Visibility.Collapsed;
                EventItems.ItemsSource = events;
            }
        }
    }
}
