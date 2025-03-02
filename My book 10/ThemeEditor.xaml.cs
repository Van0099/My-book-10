using System;
using System.IO;
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
using System.Windows.Shapes;
using System.Windows.Media.Animation;

namespace My_book_10
{
    // <summary>
    // Логика взаимодействия для ThemeEditor.xaml
    // </summary>
    public partial class ThemeEditor : Window
    {
        public ThemeEditor()
        {
            InitializeComponent();
        }

        private string hexColor;

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void SaveTheme(bool isDarkMode)
        {

            string themeName = CustomThemeName.Text;
            string themesDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mybook", "CustomThemes");

            // Создаём папку, если её нет
            if (!Directory.Exists(themesDir))
                Directory.CreateDirectory(themesDir);

            string themePath = System.IO.Path.Combine(themesDir, $"{themeName}.xaml");

            if (File.Exists(themePath))
            {
                MessageBoxResult result = MessageBox.Show($"Тема '{themeName}' уже существует. Перезаписать?",
                                                          "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Warning);

                if (result == MessageBoxResult.No)
                {
                    MessageBox.Show("Сохранение отменено.", "Отмена", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            string iconSuffix = isDarkMode ? "_dark" : "";

            string themeXaml = $@"
                    <ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                        <BitmapImage x:Key=""Align.Left"" UriSource=""pack://application:,,,/Resources/icons/alignleft{iconSuffix}.png"" />
                        <BitmapImage x:Key=""Align.Center"" UriSource=""pack://application:,,,/Resources/icons/aligncenter{iconSuffix}.png"" />
                        <BitmapImage x:Key=""Align.Right"" UriSource=""pack://application:,,,/Resources/icons/alignright{iconSuffix}.png"" />
    
                        <BitmapImage x:Key=""Image.Add"" UriSource=""pack://application:,,,/Resources/icons/addimage{iconSuffix}.png"" />
                        <BitmapImage x:Key=""Image.Remove"" UriSource=""pack://application:,,,/Resources/icons/removeimage{iconSuffix}.png"" />

                        <BitmapImage x:Key=""Search"" UriSource=""pack://application:,,,/Resources/icons/search{iconSuffix}.png"" />
                        <BitmapImage x:Key=""Search.Next"" UriSource=""pack://application:,,,/Resources/icons/searchnext{iconSuffix}.png"" />
                        <BitmapImage x:Key=""Search.Back"" UriSource=""pack://application:,,,/Resources/icons/searchback{iconSuffix}.png"" />

                        <BitmapImage x:Key=""Cm.Copy"" UriSource=""pack://application:,,,/Resources/icons/copy{iconSuffix}.png"" />
                        <BitmapImage x:Key=""Cm.Paste"" UriSource=""pack://application:,,,/Resources/icons/paste{iconSuffix}.png"" />
                        <BitmapImage x:Key=""Cm.SelectAll"" UriSource=""pack://application:,,,/Resources/icons/selectall{iconSuffix}.png"" />

                        <BitmapImage x:Key=""Table.Insert"" UriSource=""pack://application:,,,/Resources/icons/inserttable{iconSuffix}.png"" />
                        <BitmapImage x:Key=""Table.Remove"" UriSource=""pack://application:,,,/Resources/icons/removetable{iconSuffix}.png"" />

                        <BitmapImage x:Key=""List.Insert"" UriSource=""pack://application:,,,/Resources/icons/newlist{iconSuffix}.png"" />

                        <SolidColorBrush x:Key=""BackGround"" Color=""{sBackGround.HEXColor}""/>
                        <SolidColorBrush x:Key=""BrandColor"" Color=""{sBrandColor.HEXColor}""/>
                        <SolidColorBrush x:Key=""OptionalColor"" Color=""{sOptionalColor.HEXColor}""/>
                        <SolidColorBrush x:Key=""OptionalBack"" Color=""{sOptionalBack.HEXColor}""/>
                        <SolidColorBrush x:Key=""ForeGround"" Color=""{sForeGround.HEXColor}""/>
                        <SolidColorBrush x:Key=""OptionalBack2"" Color=""{sOptionalBack2.HEXColor}""/>
                    </ResourceDictionary>";

            File.WriteAllText(themePath, themeXaml);
            MessageBox.Show($"Тема '{themeName}' сохранена!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void sBackGround_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Color selectedColor = e.NewValue;

            //Преобразуем в формат HEX (#AARRGGBB)
            hexColor = $"#{selectedColor.A:X2}{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";

            //Например, выводим результат для проверки
            MessageBox.Show($"Выбранный цвет в HEX: {hexColor}");
        }

        private void SaveTheme_Click(object sender, RoutedEventArgs e)
        {
            bool isDarkMode = Dark.IsChecked == true;
            SaveTheme(isDarkMode);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Storyboard openAnimation = (Storyboard)FindResource("OpenWindowAnimation");
            openAnimation.Begin(this);
        }
    }
}
