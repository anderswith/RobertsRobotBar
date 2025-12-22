using System.Windows;
using System.Windows.Controls;

namespace RobotBarApp.View
{
    public partial class KundeIngredientCard : UserControl
    {
        public KundeIngredientCard()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(KundeIngredientCard), new PropertyMetadata(""));

        public static readonly DependencyProperty ImagePathProperty =
            DependencyProperty.Register(nameof(ImagePath), typeof(string), typeof(KundeIngredientCard), new PropertyMetadata(""));

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

        public event RoutedEventHandler Click;

        private void Button_Click(object sender, RoutedEventArgs e)
            => Click?.Invoke(this, e);
    }
}