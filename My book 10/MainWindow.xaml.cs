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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Runtime.Remoting.Contexts;
using System.Data.SqlTypes;
using Newtonsoft.Json;
using System.Reflection;


namespace My_book_10
{
	/// <summary>
	/// Логика взаимодействия для MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public class Settings
		{
			public string Theme { get; set; }
			public string Language { get; set; }
			public bool Spellcheck { get; set; }
		}

		private static readonly string settingsfile = System.IO.Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"mybook", "settings.json");

		static string filename;

		static string theme;
		static string language;
		static bool spellcheck;
		public MainWindow()
		{
			InitializeComponent();
			cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
			cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
			{

				string json = File.ReadAllText(settingsfile);

				Settings settings = JsonConvert.DeserializeObject<Settings>(json);
				if (settings.Theme == "light")
				{
					ChangeSetting(new Uri("Resources/Themes/light.xaml", UriKind.Relative));
				}
				else
				{
					ChangeSetting(new Uri("Resources/Themes/dark.xaml", UriKind.Relative));
				}
				if (settings.Language == "ru")
				{
					ChangeSetting(new Uri("Resources/Languages/russian.xaml", UriKind.Relative));
				}
				else
				{
					ChangeSetting(new Uri("Resources/Languages/english.xaml", UriKind.Relative));
				}
				if (settings.Spellcheck == true)
				{
					rtbEditor.SpellCheck.IsEnabled = true;
				}
				else if (settings.Spellcheck == false)
				{
					rtbEditor.SpellCheck.IsEnabled = false;
				}
				UpdateRecentFilesList();
			}
		}

		private void Close_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void cmbFontFamily_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cmbFontFamily.SelectedItem != null)
				rtbEditor.Selection.ApplyPropertyValue(Inline.FontFamilyProperty, cmbFontFamily.SelectedItem);
		}

		private void cmbFontSize_TextChanged(object sender, TextChangedEventArgs e)
		{
			rtbEditor.Selection.ApplyPropertyValue(Inline.FontSizeProperty, cmbFontSize.Text);
		}

		private void FullScrin_Click(object sender, RoutedEventArgs e)
		{
			if (this.WindowState == WindowState.Maximized)
			{
				this.WindowState = WindowState.Normal;
			}
			else if (this.WindowState == WindowState.Normal)
			{
				this.WindowState = WindowState.Maximized;
			}
		}

		private void Minimize_Click(object sender, RoutedEventArgs e)
		{
			this.WindowState = WindowState.Minimized;
		}

		private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}

		private void Open_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();
			dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
			if (dlg.ShowDialog() == true)
			{
				try
				{
					using (FileStream fileStream = new FileStream(dlg.FileName, FileMode.Open))
					{
						TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
						string extension = System.IO.Path.GetExtension(dlg.FileName).ToLower();

						if (extension == ".rtf")
						{
							range.Load(fileStream, DataFormats.Rtf);
						}
						else if (extension == ".txt")
						{
							range.Load(fileStream, DataFormats.Text);
						}

						string selectedFilePath = dlg.FileName;
						filename = dlg.FileName;
						RecentFilesManager.AddRecentlyOpenedFile(selectedFilePath);

						// Обновляем список недавно открытых файлов
						UpdateRecentFilesList();
					}
				}
				catch
				{
					MessageBox.Show("Похоже, этот формат не получиться открыть");
				}
			}
		}

		// Vapertu...

		private void Save_Click(object sender, RoutedEventArgs e)
		{
			if (filename != null)
			{
				using (FileStream fileStream = new FileStream(filename, FileMode.Open))
				{
					TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
					string extension = System.IO.Path.GetExtension(filename).ToLower();

					if (extension == ".rtf")
					{
						range.Save(fileStream, DataFormats.Rtf);
					}
					else if (extension == ".txt")
					{
						range.Save(fileStream, DataFormats.Text);
					}
				}
			}
			else
			{
				SaveFileDialog dlg = new SaveFileDialog();
				dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|Text File (*.txt)|*.txt|All files (*.*)|*.*";
				if (dlg.ShowDialog() == true)
				{
					string extension = System.IO.Path.GetExtension(dlg.FileName).ToLower();
					FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
					TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
					if (extension == ".rtf")
					{
						range.Save(fileStream, DataFormats.Rtf);
						filename = dlg.FileName;
					}
					else if (extension == ".txt")
					{
						range.Save(fileStream, DataFormats.Text);
						filename = dlg.FileName;
					}
				}
			}
		}

		private void Saveas()
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|All files (*.*)|*.*";
			if (dlg.ShowDialog() == true)
			{
				FileStream fileStream = new FileStream(dlg.FileName, FileMode.Create);
				TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
				range.Save(fileStream, DataFormats.Rtf);
				filename = dlg.FileName;
			}
		}

		private void New_Click(object sender, RoutedEventArgs e)
		{
			ClearRichTextBox(rtbEditor);
		}

		private void CloseDocument_Click(object sender, RoutedEventArgs e)
		{

		}

		private void BoldToggleButton_Checked(object sender, RoutedEventArgs e)
		{
			// Получаем текущий RichTextBox
			var rtb = rtbEditor;

			// Проверяем, есть ли выделение текста
			if (!rtb.Selection.IsEmpty)
			{
				// Создаем новый объект TextRange для выделения
				TextRange textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);

				// Устанавливаем жирность шрифта
				textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
			}
		}

		public void ClearRichTextBox(RichTextBox richTextBox)
		{
			richTextBox.Document.Blocks.Clear();
		}

		private void BoldToggleButton_Unchecked(object sender, RoutedEventArgs e)
		{
			// Получаем текущий RichTextBox
			var rtb = rtbEditor;

			// Проверяем, есть ли выделение текста
			if (!rtb.Selection.IsEmpty)
			{
				// Создаем новый объект TextRange для выделения
				TextRange textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);

				// Сбрасываем жирность шрифта до нормального
				textRange.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Normal);
			}
		}

		private void UnderlineToggleButton_Checked(object sender, RoutedEventArgs e)
		{
			// Получаем текущий RichTextBox
			var rtb = rtbEditor;

			// Проверяем, есть ли выделение текста
			if (!rtb.Selection.IsEmpty)
			{
				TextRange textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
				textRange.ApplyPropertyValue(Inline.TextDecorationsProperty, TextDecorations.Underline);
			}
		}

		private void UnderlineToggleButton_Unchecked(object sender, RoutedEventArgs e)
		{
			// Получаем текущий RichTextBox
			var rtb = rtbEditor;

			// Проверяем, есть ли выделение текста
			if (!rtb.Selection.IsEmpty)
			{
				TextRange textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
				textRange.ApplyPropertyValue(Inline.TextDecorationsProperty, null);
			}
		}

		private void ItalicToggleButton_Checked(object sender, RoutedEventArgs e)
		{
			// Получаем текущий RichTextBox
			var rtb = rtbEditor;

			// Проверяем, есть ли выделение текста
			if (!rtb.Selection.IsEmpty)
			{
				TextRange textRange = new TextRange(rtb.Selection.Start, rtb.Selection.End);
				textRange.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Italic);
			}
		}

		private void ItalicToggleButton_Unchecked(object sender, RoutedEventArgs e)
		{
			// Проверяем, есть ли выделение текста
			if (!rtbEditor.Selection.IsEmpty)
			{
				TextRange textRange = new TextRange(rtbEditor.Selection.Start, rtbEditor.Selection.End);
				textRange.ApplyPropertyValue(TextElement.FontStyleProperty, FontStyles.Normal);
			}
		}

		private void StandartColor_Click(object sender, RoutedEventArgs e)
		{
			TextSelection selection = rtbEditor.Selection;

			var foreGroundBrush = TryFindResource("ForeGround") as Brush;

			if (foreGroundBrush != null & !selection.IsEmpty)
			{
				// Создаем новый диапазон текста на основе выделения
				TextRange range = new TextRange(selection.Start, selection.End);

				// Устанавливаем цвет текста
				range.ApplyPropertyValue(TextElement.ForegroundProperty, foreGroundBrush);
			}
		}

		private void ChangeColor(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			string colorn = button.Name.ToString();

			Color color = (Color)ColorConverter.ConvertFromString(colorn);

			if (button != null)
			{
				TextSelection selection = rtbEditor.Selection;

				var foreGroundBrush = TryFindResource("ForeGround") as Brush;

				if (foreGroundBrush != null & !selection.IsEmpty)
				{
					// Создаем новый диапазон текста на основе выделения
					TextRange range = new TextRange(selection.Start, selection.End);

					// Устанавливаем цвет текста
					range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
				}
			}
		}

		private void ChangeColorHEX(object sender, RoutedEventArgs e)
		{
			var button = sender as Button;
			string colorn = button.Name.ToString();

			Color color = (Color)ColorConverter.ConvertFromString("#" + colorn);

			if (button != null)
			{
				TextSelection selection = rtbEditor.Selection;

				var foreGroundBrush = TryFindResource("ForeGround") as Brush;

				if (foreGroundBrush != null & !selection.IsEmpty)
				{
					// Создаем новый диапазон текста на основе выделения
					TextRange range = new TextRange(selection.Start, selection.End);

					// Устанавливаем цвет текста
					range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(color));
				}
			}
		}

		private void TextColor_Click(object sender, RoutedEventArgs e)
		{
			if (Colors.Visibility != Visibility.Hidden)
			{
				Colors.Visibility = Visibility.Hidden;
			}

			else
			{
				Colors.Visibility = Visibility.Visible;
			}
		}

		private void Dark_Checked(object sender, RoutedEventArgs e)
		{
			//File.WriteAllText("config/theme.txt", "Dark");
			ChangeSetting(new Uri("Resources/Themes/dark.xaml", UriKind.Relative));
			theme = "dark";
			SettingsSave();
		}

		public void ChangeSetting(Uri setting)
		{
			ResourceDictionary Setting = new ResourceDictionary() { Source = setting }; ;
			Resources.Remove(Setting);
			Resources.MergedDictionaries.
			Add(Setting);
		}

		private void Light_Checked(object sender, RoutedEventArgs e)
		{
			//File.WriteAllText("config/theme.txt", "Dark");
			ChangeSetting(new Uri("Resources/Themes/light.xaml", UriKind.Relative));
			theme = "light";
			SettingsSave();
		}

		private void Language_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (Language.SelectedIndex == 0)
			{
				ChangeSetting(new Uri("Resources/Languages/russian.xaml", UriKind.Relative));
				language = "ru";
				SettingsSave();
			}
			else if (Language.SelectedIndex == 1)
			{
				ChangeSetting(new Uri("Resources/Languages/english.xaml", UriKind.Relative));
				language = "en";
				SettingsSave();
			}
		}

		private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (Menu.Visibility != Visibility.Visible)
			{
				Menu.Visibility = Visibility.Visible;
			}
			else
			{
				Menu.Visibility = Visibility.Hidden;
			}
		}

		private void Window_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Escape) // Открыть Меню / Open Menu
			{
				if (Menu.Visibility != Visibility.Visible)
				{
					Menu.Visibility = Visibility.Visible;
				}
				else
				{
					Menu.Visibility = Visibility.Hidden;
				}
			}

			if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.S) // Ctrl S
			{
				Save_Click(sender, e);
			}

			if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && (Keyboard.Modifiers & ModifierKeys.Alt) != 0 && e.Key == Key.S) // Ctrl Alt S
			{
				Saveas();
			}

			if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.O) // Ctrl O
			{
				Open_Click(sender, e);
			}

			if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.N) // Ctrl N
			{
				New_Click(sender, e);
			}
		}
		private void AlignLeft_Click(object sender, RoutedEventArgs e)
		{
			SetTextAlignment(TextAlignment.Left);
		}

		private void AlignCenter_Click(object sender, RoutedEventArgs e)
		{
			SetTextAlignment(TextAlignment.Center);
		}

		private void AlignRight_Click(object sender, RoutedEventArgs e)
		{
			SetTextAlignment(TextAlignment.Right);
		}

		// Метод для установки выравнивания текста
		private void SetTextAlignment(TextAlignment alignment)
		{
			TextRange textRange = new TextRange(rtbEditor.Selection.Start, rtbEditor.Selection.End);

			// Получаем текущее форматирование текста
			var currentProperties = textRange.GetPropertyValue(Block.TextAlignmentProperty);

			// Устанавливаем новое выравнивание
			if (currentProperties != null)
			{
				textRange.ApplyPropertyValue(Block.TextAlignmentProperty, alignment);
			}
		}

		private void InsertImage_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "Image Files (*.png; *.jpg)|*.png;*.jpg|All files (*.*)|*.*",
				Multiselect = false
			};

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					using (Stream stream = File.OpenRead(openFileDialog.FileName))
					{
						var image = new Image
						{
							Stretch = Stretch.Uniform,
							MaxWidth = 300,
							MaxHeight = 300
						};

						var bitmapImage = new System.Windows.Media.Imaging.BitmapImage();
						bitmapImage.BeginInit();
						bitmapImage.StreamSource = stream;
						bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
						bitmapImage.EndInit();

						image.Source = bitmapImage;

						Block block = new BlockUIContainer(image);
						rtbEditor.Document.Blocks.InsertBefore(rtbEditor.CaretPosition.Paragraph, block);
					}
				}
				catch (IOException ex)
				{
					MessageBox.Show($"Ошибка при открытии файла: {ex.Message}");
				}
			}
		}

		private void RemoveImage_Click(object sender, RoutedEventArgs e)
		{
			if (rtbEditor.Selection.Text.Length > 0)
			{
				rtbEditor.Selection.Text = "";
			}
			else
			{
				TextPointer pointer = rtbEditor.CaretPosition;
				InlineUIContainer container = pointer.Paragraph.Inlines.FirstInline as InlineUIContainer;

				if (container != null && container.Child is Image)
				{
					rtbEditor.Document.Blocks.Remove(pointer.Paragraph);
				}
			}
		}

		private void SpellCheck_Checked(object sender, RoutedEventArgs e)
		{
			rtbEditor.SpellCheck.IsEnabled = true;
			spellcheck = true;
			SettingsSave();
        }

		private void SpellCheck_Unchecked(object sender, RoutedEventArgs e)
		{
			rtbEditor.SpellCheck.IsEnabled = false;
			spellcheck = false;
			SettingsSave();
		}

		private void SettingsSave()
		{
			Settings settings = new Settings
			{
				Theme = theme,
				Language = language,
				Spellcheck = spellcheck
			};

			string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

			File.WriteAllText(settingsfile, json);
		}

		private void SetLineSpacing_Click(object sender, RoutedEventArgs e)
		{
			Button button = sender as Button;

			// Извлекаем параметр из свойства Tag и приводим его к типу double
			double parameter = (double)button.Tag;

			TextPointer start = rtbEditor.Selection.Start;
			TextPointer end = rtbEditor.Selection.End;

			while (start != null && start.CompareTo(end) <= 0)
			{
				var currentParagraph = start.Paragraph;
				if (currentParagraph != null)
				{
					currentParagraph.LineHeight = parameter * 12; // Устанавливаем высоту строки равной 24 пикселям (пример для 1.5x от стандартного размера шрифта)
				}

				start = start.GetNextContextPosition(LogicalDirection.Forward);
			}
		}

		private void UpdateRecentFilesList()
		{
			// Загружаем словарь недавно открытых файлов
			var recentFiles = RecentFilesManager.LoadRecentFiles();

			// Очищаем ListBox перед добавлением новых данных
			recentFilesListBox.Items.Clear();

			if (recentFiles.Count == 0)
			{
				recentFilesListBox.Items.Add("Нет недавно открытых файлов.");
			}
			else
			{
				// Добавляем в ListBox только имена файлов
				foreach (var filePath in recentFiles.Values)
				{
					recentFilesListBox.Items.Add(System.IO.Path.GetFileName(filePath));
				}
			}
		}



		// Обработчик клика на элемент в ListBox
		private void RecentFilesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			if (recentFilesListBox.SelectedItem != null)
			{
				// Получаем выбранный индекс
				int selectedIndex = recentFilesListBox.SelectedIndex;

				// Загружаем словарь недавно открытых файлов
				var recentFiles = RecentFilesManager.LoadRecentFiles();

				// Проверяем, что индекс в пределах допустимого
				if (selectedIndex >= 0 && selectedIndex < recentFiles.Count)
				{
					// Получаем полный путь файла из словаря
					string selectedFilePath = recentFiles.Values.ElementAt(selectedIndex);

					FileStream fileStream = new FileStream(selectedFilePath, FileMode.Open);
					TextRange range = new TextRange(rtbEditor.Document.ContentStart, rtbEditor.Document.ContentEnd);
					range.Load(fileStream, DataFormats.Rtf);
					Menu.Visibility = Visibility.Hidden;
				}

				// Снимаем выделение, чтобы избежать повторного открытия того же файла
				recentFilesListBox.SelectedItem = null;
			}
		}

        private void InsertTable_menu(object sender, RoutedEventArgs e)
        {

        }

        private void InsertTable_apply(object sender, RoutedEventArgs e)
        {
            int rows = RowsNDD.Value;      // Количество строк
            int columns = ColumnsNDD.Value; // Количество столбцов

            // Создаём таблицу
            Table table = new Table();

            // Устанавливаем количество столбцов
            for (int i = 0; i < columns; i++)
            {
                table.Columns.Add(new TableColumn());
            }

            // Добавляем строки
            for (int i = 0; i < rows; i++)
            {
                TableRow row = new TableRow();

                for (int j = 0; j < columns; j++)
                {
                    TableCell cell = new TableCell(new Paragraph(new Run(" "))); // Пустая ячейка
                    cell.BorderBrush = Brushes.Black; // Границы ячеек
                    cell.BorderThickness = new Thickness(1);
                    row.Cells.Add(cell);
                }

                TableRowGroup rowGroup = table.RowGroups.Count > 0 ? table.RowGroups[0] : new TableRowGroup();
                rowGroup.Rows.Add(row);
                if (table.RowGroups.Count == 0) table.RowGroups.Add(rowGroup);
            }

            // Вставляем таблицу в RichTextBox
            rtbEditor.Document.Blocks.Add(table);
        }
    }
}



