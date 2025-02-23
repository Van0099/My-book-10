using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace My_book_10
{
    /// <summary>
    /// Логика взаимодействия для ColorPicker.xaml
    /// </summary>
    public partial class ColorPicker : UserControl
    {
        public ColorPicker()
        {
            InitializeComponent();
        }

        private void HueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double hue = e.NewValue;
            Color color = ColorFromHSV(hue, 1, 1);
            GradientBrush.GradientStops[1].Color = color;
        }

        private void GradientPicker_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point position = e.GetPosition(GradientPicker);

                // Ограничиваем координаты, чтобы маркер не выходил за границы
                double x = Math.Max(0, Math.Min(position.X, GradientPicker.Width));
                double y = Math.Max(0, Math.Min(position.Y, GradientPicker.Height));

                double saturation = x / GradientPicker.Width;
                double brightness = 1 - (y / GradientPicker.Height);

                Color color = ColorFromHSV(HueSlider.Value, saturation, brightness);
                SelectedColorPreview.Background = new SolidColorBrush(color);

                // Обновление позиции маркера
                Canvas.SetLeft(SelectionMarker, x - SelectionMarker.Width / 2);
                Canvas.SetTop(SelectionMarker, y - SelectionMarker.Height / 2);
            }
        }

        private Color ColorFromHSV(double hue, double saturation, double brightness)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            brightness = Math.Max(0, Math.Min(brightness, 1)); // Ограничение диапазона
            saturation = Math.Max(0, Math.Min(saturation, 1)); // Ограничение диапазона

            int v = (int)(brightness * 255);
            int p = (int)(brightness * (1 - saturation) * 255);
            int q = (int)(brightness * (1 - f * saturation) * 255);
            int t = (int)(brightness * (1 - (1 - f) * saturation) * 255);

            switch (hi)
            {
                case 0: return Color.FromRgb((byte)v, (byte)t, (byte)p);
                case 1: return Color.FromRgb((byte)q, (byte)v, (byte)p);
                case 2: return Color.FromRgb((byte)p, (byte)v, (byte)t);
                case 3: return Color.FromRgb((byte)p, (byte)q, (byte)v);
                case 4: return Color.FromRgb((byte)t, (byte)p, (byte)v);
                default: return Color.FromRgb((byte)v, (byte)p, (byte)q);
            }
        }


    }
}
