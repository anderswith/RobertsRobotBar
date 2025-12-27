﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RobotBarApp.View
{
    public partial class KundeMenuItemCard : UserControl
    {
        public KundeMenuItemCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(KundeMenuItemCard), new PropertyMetadata(""));

        public static readonly DependencyProperty IngredientsTextProperty =
            DependencyProperty.Register(nameof(IngredientsText), typeof(string), typeof(KundeMenuItemCard), new PropertyMetadata(""));

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register(nameof(ImageSource), typeof(object), typeof(KundeMenuItemCard), new PropertyMetadata(null));

        public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
        public string IngredientsText { get => (string)GetValue(IngredientsTextProperty); set => SetValue(IngredientsTextProperty, value); }
        public object ImageSource
        {
            get => GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        // Bubble a Click event to parent
        public event RoutedEventHandler Click;

        private void Button_Click(object sender, RoutedEventArgs e) => Click?.Invoke(this, e);
        
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(
                nameof(Command),
                typeof(ICommand),
                typeof(KundeMenuItemCard));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }
    }
}