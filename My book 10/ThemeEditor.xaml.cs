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

namespace My_book_10
{
    /// <summary>
    /// Логика взаимодействия для ThemeEditor.xaml
    /// </summary>
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

        private void SaveTheme_Click(object sender, RoutedEventArgs e)
        {
            string userThemePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mybook", "CustomTheme.xaml");

            //string themeXaml = $@"
                    //<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
                    //xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
                        //<BitmapImage x:Key=""Align.Left"" UriSource=""/Resources/icons/alignleft.png"" />
                        //<BitmapImage x:Key=""Align.Center"" UriSource=""/Resources/icons/aligncenter.png"" />
                        //<BitmapImage x:Key=""Align.Right"" UriSource=""/Resources/icons/alignright.png"" />
    
                        //<BitmapImage x:Key=""Image.Add"" UriSource=""/Resources/icons/addimage.png"" />
                        //<BitmapImage x:Key=""Image.Remove"" UriSource=""/Resources/icons/removeimage.png"" />

                        //<BitmapImage x:Key=""Cm.Copy"" UriSource=""/Resources/icons/copy.png"" />
                        //<BitmapImage x:Key=""Cm.Paste"" UriSource=""/Resources/icons/paste.png"" />
                        //<BitmapImage x:Key=""Cm.SelectAll"" UriSource=""/Resources/icons/selectall.png"" />

                        //<BitmapImage x:Key=""Table.Insert"" UriSource=""/Resources/icons/inserttable_light.png"" />
                        //<BitmapImage x:Key=""Table.Remove"" UriSource=""/Resources/icons/removetable_light.png"" />

                        //<BitmapImage x:Key=""List.Insert"" UriSource=""/Resources/icons/newlist.png"" />

                        //<SolidColorBrush x:Key=""BackGround"" Color=""{sBackGround.Color}""/>
                        //<SolidColorBrush x:Key=""BrandColor"" Color=""{sBrandColor.Color}""/>
                        //<SolidColorBrush x:Key=""OptionalColor"" Color=""{sOptionalColor.Color}""/>
                        //<SolidColorBrush x:Key=""OptionalBack"" Color=""{sOptionalBack.Color}""/>
                        //<SolidColorBrush x:Key=""ForeGround"" Color=""{sForeGround.Color}""/>
                        //<SolidColorBrush x:Key=""OptionalBack2"" Color=""{sOptionalBack2.Color}""/>
                    //</ResourceDictionary>";

            // File.WriteAllText(userThemePath, themeXaml);
            MessageBox.Show("Тема сохранена!", "Готово", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }

        private void sBackGround_ColorChanged(object sender, RoutedPropertyChangedEventArgs<Color> e)
        {
            Color selectedColor = e.NewValue;

            // Преобразуем в формат HEX (#AARRGGBB)
            hexColor = $"#{selectedColor.A:X2}{selectedColor.R:X2}{selectedColor.G:X2}{selectedColor.B:X2}";

            // Например, выводим результат для проверки
            MessageBox.Show($"Выбранный цвет в HEX: {hexColor}");
        }
    }
}
