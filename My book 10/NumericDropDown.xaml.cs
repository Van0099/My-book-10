using System.Windows;
using System.Windows.Controls;

namespace My_book_10
{
    public partial class NumericDropDown : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(int), typeof(NumericDropDown), new PropertyMetadata(0));

        public int Value
        {
            get => (int)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public NumericDropDown()
        {
            InitializeComponent();
        }

        private void IncreaseValue(object sender, RoutedEventArgs e)
        {
            if (Value < 100)
            {
                Value++;
            }
        }

        private void DecreaseValue(object sender, RoutedEventArgs e)
        {
            if (Value > 0)
                Value--;
        }
    }
}
