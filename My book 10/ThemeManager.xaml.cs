using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Win32;
using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;

namespace My_book_10
{
    /// <summary>
    /// Логика взаимодействия для ThemeManager.xaml
    /// </summary>
    public partial class ThemeManager : Window
    {
        private static readonly string settingsfile = System.IO.Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "mybook", "settings.json");
        private string customThemesPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mybook", "CustomThemes");
        public ThemeManager()
        {
            InitializeComponent();
            LoadThemes();
        }

        private void LoadThemes()
        {
            if (!Directory.Exists(customThemesPath))
            {
                Directory.CreateDirectory(customThemesPath);
            }

            ThemeListBox.Items.Clear();
            var themeFiles = Directory.GetFiles(customThemesPath, "*.xaml");

            foreach (var file in themeFiles)
            {
                ThemeListBox.Items.Add(System.IO.Path.GetFileNameWithoutExtension(file));
            }
        }

        private void ThemeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeListBox.SelectedItem is string selectedTheme)
            {
                string themePath = System.IO.Path.Combine(customThemesPath, selectedTheme + ".xaml");
                ApplyTheme(themePath);

                // Сохранение в настройки
                Settings settings = new Settings
                {
                    Theme = themePath.ToString(),
                    Language = MainWindow.language,
                    Spellcheck = MainWindow.spellcheck
                };

                string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

                File.WriteAllText(settingsfile, json);
            }
        }

        private void RemoveStandardThemes()
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;

            // Получаем пути к стандартным темам
            string lightThemePath = "Resources/Themes/light.xaml";
            string darkThemePath = "Resources/Themes/dark.xaml";

            // Находим и удаляем словари, если они есть
            var toRemove = dictionaries.Where(d =>
                d.Source != null &&
                (d.Source.OriginalString == lightThemePath || d.Source.OriginalString == darkThemePath))
                .ToList();

            foreach (var dict in toRemove)
            {
                dictionaries.Remove(dict);
            }
        }

        private void ApplyTheme(string themePath)
        {
            RemoveStandardThemes();

            MessageBox.Show("Загружаемая тема: " + themePath);

            ResourceDictionary theme = new ResourceDictionary
            {
                Source = new Uri(themePath, UriKind.RelativeOrAbsolute)
            };

            // Убедитесь, что ресурс не добавляется повторно
            if (!Application.Current.Resources.MergedDictionaries.Contains(theme))
            {
                Application.Current.Resources.MergedDictionaries.Add(theme);
            }

            Application.Current.Dispatcher.Invoke(() =>
            {
                ((MainWindow)Application.Current.MainWindow).Dark.IsChecked = false;
                ((MainWindow)Application.Current.MainWindow).Light.IsChecked = false;
            });


            ThemeListBox.SelectedItem = null;
            this.Close();
            
        }

        private void AddThemeButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "XAML files (*.xaml)|*.xaml",
                Title = "Выберите файл темы"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                string destinationPath = System.IO.Path.Combine(customThemesPath, System.IO.Path.GetFileName(openFileDialog.FileName));
                File.Copy(openFileDialog.FileName, destinationPath, true);
                LoadThemes();
            }
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
    }
}
