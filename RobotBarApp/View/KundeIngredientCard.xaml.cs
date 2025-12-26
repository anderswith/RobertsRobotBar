using System.Windows;
using System.Windows.Input;

namespace RobotBarApp.View
{
    public partial class KundeIngredientCard
    {
        public KundeIngredientCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(KundeIngredientCard), new PropertyMetadata(""));

        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(KundeIngredientCard), new PropertyMetadata(""));

        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(KundeIngredientCard), new PropertyMetadata(null));

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register(nameof(CommandParameter), typeof(object), typeof(KundeIngredientCard), new PropertyMetadata(null));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public string ImagePath
        {
            get => (string)GetValue(ImagePathProperty);
            set => SetValue(ImagePathProperty, value);
        }

        public ICommand? Command
        {
            get => (ICommand?)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public object? CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        public event RoutedEventHandler? Click;

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            // First: run bound MVVM command (if any)
            if (Command?.CanExecute(CommandParameter) == true)
                Command.Execute(CommandParameter);

            // Also keep legacy Click event for other usages
            Click?.Invoke(this, e);
        }
    }
}