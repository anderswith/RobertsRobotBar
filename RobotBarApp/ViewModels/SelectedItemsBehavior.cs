using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace RobotBarApp.Behaviors
{
    public static class SelectedItemsBehavior
    {
        public static readonly DependencyProperty BindableSelectedItemsProperty =
            DependencyProperty.RegisterAttached(
                "BindableSelectedItems",
                typeof(IList),
                typeof(SelectedItemsBehavior),
                new PropertyMetadata(null, OnBindableSelectedItemsChanged));

        public static void SetBindableSelectedItems(DependencyObject obj, IList value)
        {
            obj.SetValue(BindableSelectedItemsProperty, value);
        }

        public static IList GetBindableSelectedItems(DependencyObject obj)
        {
            return (IList)obj.GetValue(BindableSelectedItemsProperty);
        }

        private static void OnBindableSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not ListBox listBox)
                return;

            listBox.SelectionChanged -= ListBox_SelectionChanged;

            if (e.NewValue is IList newList)
            {
                listBox.SelectionMode = SelectionMode.Extended;
                listBox.SelectionChanged += ListBox_SelectionChanged;
            }
        }

        private static void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is not ListBox listBox)
                return;

            IList boundList = GetBindableSelectedItems(listBox);
            if (boundList == null)
                return;

            foreach (var removed in e.RemovedItems)
                boundList.Remove(removed);

            foreach (var added in e.AddedItems)
                if (!boundList.Contains(added))
                    boundList.Add(added);
        }
    }
}