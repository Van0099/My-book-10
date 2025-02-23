using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    /// Логика взаимодействия для ColorBox.xaml
    /// </summary>
    public partial class ColorBox : UserControl
    {
        public string HEXColor { get; private set; }

        public ColorBox()
        {
            InitializeComponent();
        }

        // Метод для проверки, является ли строка валидным HEX-кодом
        private void HexBox_TextChanged(object sender, RoutedEventArgs e)
        {
            string input = HexBox.Text.Trim();

            // Убираем проверку на # и добавляем его, если его нет
            if (!input.StartsWith("#"))
            {
                input = "#" + input;
            }

            // Проверяем, что строка является валидным HEX-кодом
            if (IsHexColor(input))
            {
                HEXColor = input;
                // Устанавливаем BorderBrush в цвет #89FFFFFF при успешной проверке
                HexBox.BorderBrush = new SolidColorBrush(Color.FromArgb(0x89, 0xFF, 0xFF, 0xFF));
            }
            else
            {
                // Устанавливаем BorderBrush в красный цвет при неудачной попытке
                HexBox.BorderBrush = Brushes.Red;
            }
        }

        // Метод для проверки, является ли строка валидным HEX-кодом
        private bool IsHexColor(string input)
        {
            // Регулярное выражение для проверки HEX-цвета (например, #RRGGBB или #RGB)
            string hexPattern = @"^#([0-9A-Fa-f]{3}){1,2}$";
            return Regex.IsMatch(input, hexPattern);
        }
    }
}
