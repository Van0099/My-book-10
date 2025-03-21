using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using Newtonsoft.Json;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using static UpdateService;
using System.Windows.Media.Effects;


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

		public static string theme;
		public static string language;
		public static bool spellcheck;
		static bool isFocusMode = false;

        private TextSearcher textSearcher;

        public void ShowWithAnimation(FrameworkElement element)
        {
            if (element.Visibility != Visibility.Visible)
            {
                element.Visibility = Visibility.Visible;
                element.Opacity = 0; // Ставим 0 перед анимацией

                if (element.FindResource("FadeInAnimation") is Storyboard fadeIn)
                {
                    fadeIn.Begin(element);
                }
            }
        }

        //private SearchService searcher; // Было TestSearcher, но должен быть TextSearcher

        public MainWindow()
		{
			InitializeComponent();
            textSearcher = new TextSearcher(rtbEditor);
            cmbFontFamily.ItemsSource = Fonts.SystemFontFamilies.OrderBy(f => f.Source);
			cmbFontSize.ItemsSource = new List<double>() { 8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72 };
			{

				string json = File.ReadAllText(settingsfile);

				Settings settings = JsonConvert.DeserializeObject<Settings>(json);
				theme = settings.Theme;
				language = settings.Language;
				spellcheck = settings.Spellcheck;
				if (theme == "light")
				{
					ChangeSetting(new Uri("Resources/Themes/light.xaml", UriKind.Relative));
				}
				else if (theme == "dark")
				{
					ChangeSetting(new Uri("Resources/Themes/dark.xaml", UriKind.Relative));
				}
				if (language == "ru")
				{
					ChangeSetting(new Uri("Resources/Languages/russian.xaml", UriKind.Relative));
				}
				else
				{
					ChangeSetting(new Uri("Resources/Languages/english.xaml", UriKind.Relative));
				}
				if (spellcheck == true)
				{
					rtbEditor.SpellCheck.IsEnabled = true;
				}
				else if (spellcheck == false)
				{
					rtbEditor.SpellCheck.IsEnabled = false;
				}
				{ // настройка ui элементов настроек
                    if (settings.Theme == "light")
                    {
						Light.IsChecked = true;
                    }
                    else if (settings.Theme == "dark")
                    {
                        Dark.IsChecked = true;
                    }
                    if (settings.Language == "ru")
                    {
						Language.Text = "Русский";
                    }
                    else if (settings.Language == "en")
                    {
                        Language.Text = "English";
                    }
					if (settings.Spellcheck == true)
					{
						SpellCheck.IsChecked = true;
					}
					else if (settings.Spellcheck == false)
                    {
                        SpellCheck.IsChecked = false;
                    }
                }
				UpdateRecentFilesList();
                SystemEvents.UserPreferenceChanged += SystemEvents_UserPreferenceChanged;
                this.Closed += (s, e) => SystemEvents.UserPreferenceChanged -= SystemEvents_UserPreferenceChanged;
				VersionRead();
            }
		}

		private void VersionRead()
		{
            VersionInfoReader reader = new VersionInfoReader();
            VersionInfo version = reader.ReadVersionInfo();
            versionTB.Text = $"My book {version.PublicVersion} {version.VersionType} {version.VersionID}";
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateAppIcon();
            Storyboard openAnimation = (Storyboard)FindResource("OpenWindowAnimation");
            openAnimation.Begin(this);
        }

        private void SystemEvents_UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General) // Тема могла измениться
            {
                UpdateAppIcon();
            }
        }

        private void UpdateAppIcon()
        {
            string iconPath = ThemeHelper.IsDarkTheme()
                ? "pack://application:,,,/Resources/icon_dark.png"
                : "pack://application:,,,/Resources/icon.png";

            Uri iconUri = new Uri(iconPath, UriKind.RelativeOrAbsolute);
            this.Icon = BitmapFrame.Create(iconUri);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
			if (Search.Visibility != Visibility.Visible)
				Search.Visibility = Visibility.Visible;
			else
			{
                Search.Visibility = Visibility.Hidden;
                textSearcher.ClearHighlight();
            }

        }

        private void FocusMode(object sender, RoutedEventArgs e)
        {
			if (isFocusMode != true)
			{
				ToggleFocusMode(true);
            }
			else
			{
                ToggleFocusMode(false);
            }
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
				dlg.Filter = "Rich Text Format (*.rtf)|*.rtf|Portable Document Format (*.pdf)|*.pdf|Text File (*.txt)|*.txt|All files (*.*)|*.*";
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
                    else if (extension == ".pdf")
                    {
                        PdfDocument document = new PdfDocument();
                        document.Info.Title = "Экспортированный документ";

                        PdfPage page = document.AddPage();
                        XGraphics gfx = XGraphics.FromPdfPage(page);

                        // Пример: выводим текст из RichTextBox в PDF
                        string text = range.Text;

                        XFont font = new XFont("Verdana", 12);
                        gfx.DrawString(text, font, XBrushes.Black, new XRect(40, 40, page.Width - 80, page.Height - 80), XStringFormats.TopLeft);

                        document.Save(fileStream);
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

        private void RemoveCustomTheme()
        {
            var dictionaries = Application.Current.Resources.MergedDictionaries;
            string customThemePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mybook", "CustomThemes");

            var toRemove = dictionaries.Where(d =>
                d.Source != null && d.Source.OriginalString.StartsWith(customThemePath))
                .ToList();

            foreach (var dict in toRemove)
            {
                dictionaries.Remove(dict);
            }
        }
        public void ChangeSetting(Uri setting)
		{
            RemoveCustomTheme();
            ResourceDictionary Setting = new ResourceDictionary() { Source = setting }; ;
            Application.Current.Resources.Remove(Setting);
            Application.Current.Resources.MergedDictionaries.
			Add(Setting);
		}

		private void Light_Checked(object sender, RoutedEventArgs e)
		{
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
                //ShowWithAnimation(Menu);
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
                    //ShowWithAnimation(Menu);
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

			string json = JsonConvert.SerializeObject(settings, Newtonsoft.Json.Formatting.Indented);

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
			if (Table.Visibility == Visibility.Visible)
                Table.Visibility = Visibility.Hidden;
			else
                Table.Visibility = Visibility.Visible;
        }

		private void InsertTable_apply(object sender, RoutedEventArgs e)
		{
			int rows = RowsNDD.Value;
			int columns = ColumnsNDD.Value;

            Table table = new Table
    {
        CellSpacing = 0 // Убираем зазоры между ячейками
    };

    TableRowGroup tableRowGroup = new TableRowGroup();
    table.RowGroups.Add(tableRowGroup);

    for (int i = 0; i < rows; i++)
    {
        TableRow row = new TableRow();

        for (int j = 0; j < columns; j++)
        {
            TableCell cell = new TableCell(new Paragraph(new Run(" ")))
            {
                BorderThickness = new Thickness(0.5),
                BorderBrush = Brushes.Gray,
                Padding = new Thickness(5),
                TextAlignment = TextAlignment.Center
            };

            // Первая строка - заголовок
            if (i == 0)
            {
                cell.Background = Brushes.LightGray;
                cell.FontWeight = FontWeights.Bold;
            }

            // Скругляем углы только у крайних ячеек
            if ((i == 0 && j == 0) || // Верхний левый угол
                (i == 0 && j == columns - 1) || // Верхний правый угол
                (i == rows - 1 && j == 0) || // Нижний левый угол
                (i == rows - 1 && j == columns - 1)) // Нижний правый угол
            {
                cell.BorderBrush = Brushes.Transparent; // Убираем стандартную рамку
                cell.Background = Brushes.White; // Делаем фон ячейки белым
            }

            row.Cells.Add(cell);
        }

        tableRowGroup.Rows.Add(row);
    }

    rtbEditor.Selection.Text = ""; // Убираем выделенный текст
    rtbEditor.CaretPosition.InsertTextInRun("\n");
    rtbEditor.CaretPosition.Paragraph.SiblingBlocks.Add(table);
			Table.Visibility = Visibility.Hidden;
        }

        private void RemoveTable(object sender, RoutedEventArgs e)
        {
            Table table = FindTableUnderSelection();
            if (table == null) return;

            // Получаем родительский блок таблицы (обычно это Paragraph или TableRowGroup)
            TextPointer caretPos = rtbEditor.CaretPosition;
            TableCell cell = caretPos.Paragraph?.Parent as TableCell;

            if (cell != null)
            {
                // Находим таблицу в родительском блоке
                TableRow row = cell.Parent as TableRow;
                if (row != null)
                {
                    TableRowGroup rowGroup = row.Parent as TableRowGroup;
                    if (rowGroup != null)
                    {
                        // Теперь удаляем таблицу, просто удалив родительский блок из документа
                        var parentBlock = rowGroup.Parent as Block;
                        if (parentBlock != null)
                        {
                            // Удаляем блок, содержащий таблицу
                            rtbEditor.Document.Blocks.Remove(parentBlock);
                        }
                    }
                }
            }
        }

        private void CopyText_Click(object sender, RoutedEventArgs e)
		{
			if (rtbEditor.Selection.Text.Length > 0) // Проверяем, что есть выделенный текст
			{
				Clipboard.SetText(rtbEditor.Selection.Text);
			}
		}

		// Вставить текст из буфера обмена
		private void PasteText_Click(object sender, RoutedEventArgs e)
		{
			if (Clipboard.ContainsText())
			{
				rtbEditor.Selection.Text = Clipboard.GetText();
			}
		}

		// Выделить весь текст
		private void SelectAllText_Click(object sender, RoutedEventArgs e)
		{
			rtbEditor.SelectAll();
		}

        private void View_Click(object sender, RoutedEventArgs e)
        {
			View.ContextMenu.IsOpen = true;
        }

        private Table FindTableUnderSelection()
        {
            // Получаем позицию курсора
            TextPointer caretPos = rtbEditor.CaretPosition;

            // Получаем абзац, в котором находится курсор
            Paragraph para = caretPos.Paragraph;
            if (para == null)
                return null;

            // Проверяем, является ли родитель абзаца элементом TableCell
            TableCell cell = para.Parent as TableCell;
            if (cell == null)
                return null;

            // Получаем родительскую строку таблицы
            TableRow row = cell.Parent as TableRow;
            if (row == null)
                return null;

            // Получаем группу строк
            TableRowGroup trg = row.Parent as TableRowGroup;
            if (trg == null)
                return null;

            // Наконец, получаем таблицу
            Table table = trg.Parent as Table;
            return table;
        }

        private void AddCollumnTable(object sender, RoutedEventArgs e)
        {
            Table table = FindTableUnderSelection();
            if (table == null) return;

            TableRowGroup rowGroup = table.RowGroups.FirstOrDefault();
            if (rowGroup == null || rowGroup.Rows.Count == 0) return;

            // Берем первую строку для копирования стиля
            TableRow firstRow = rowGroup.Rows.First();

            // Создаем новую ячейку для каждого столбца
            foreach (TableRow row in rowGroup.Rows)
            {
                // Берем стиль из первой ячейки строки
                TableCell templateCell = firstRow.Cells.FirstOrDefault();

                if (templateCell != null)
                {
                    TableCell newCell = new TableCell(new Paragraph(new Run(" ")))
                    {
                        BorderThickness = templateCell.BorderThickness,
                        BorderBrush = templateCell.BorderBrush,
                        Padding = templateCell.Padding,
                        TextAlignment = templateCell.TextAlignment
                    };

                    newCell.Background = Brushes.White;

                    // Добавляем новую ячейку в строку
                    row.Cells.Add(newCell);
                }
            }
        }

        private void RemoveCollumnTable(object sender, RoutedEventArgs e)
        {
            Table table = FindTableUnderSelection();
            if (table == null)
            {
                MessageBox.Show("Таблица не найдена!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TextPointer caretPos = rtbEditor.CaretPosition;
            Paragraph para = caretPos.Paragraph;
            if (para == null)
                return;

            TableCell selectedCell = para.Parent as TableCell;
            if (selectedCell == null)
                return;

            TableRow selectedRow = selectedCell.Parent as TableRow;
            if (selectedRow == null)
                return;

            int columnIndex = selectedRow.Cells.IndexOf(selectedCell);
            if (columnIndex == -1)
                return;

            // Удаляем ячейки в каждом ряду, относящиеся к этому столбцу
            foreach (TableRow row in table.RowGroups[0].Rows)
            {
                if (columnIndex < row.Cells.Count)
                {
                    row.Cells.RemoveAt(columnIndex);
                }
            }
        }
        private void AddRowToTable(object sender, RoutedEventArgs e)
        {
            Table table = FindTableUnderSelection();
            if (table == null) return;

            TableRowGroup rowGroup = table.RowGroups.FirstOrDefault();
            if (rowGroup == null || rowGroup.Rows.Count == 0) return;

            // Берем первую строку для копирования стиля
            TableRow firstRow = rowGroup.Rows.First();

            // Создаем новую строку
            TableRow newRow = new TableRow();

            // Копируем стиль из первой строки
            foreach (TableCell templateCell in firstRow.Cells)
            {
                TableCell newCell = new TableCell(new Paragraph(new Run(" ")))
                {
                    BorderThickness = templateCell.BorderThickness,
                    BorderBrush = templateCell.BorderBrush,
                    Padding = templateCell.Padding,
                    TextAlignment = templateCell.TextAlignment
                };

                newCell.Background = Brushes.White;

                newRow.Cells.Add(newCell);
            }

            // Добавляем строку в таблицу
            rowGroup.Rows.Add(newRow);
        }

        private void RemoveRowFromTable(object sender, RoutedEventArgs e)
        {
            Table table = FindTableUnderSelection();
            if (table == null) return;

            TableRowGroup rowGroup = table.RowGroups.FirstOrDefault();
            if (rowGroup == null || rowGroup.Rows.Count == 0) return;

            // Определяем текущую строку
            TextPointer caretPos = rtbEditor.CaretPosition;
            TableCell cell = caretPos.Paragraph?.Parent as TableCell;
            TableRow row = cell?.Parent as TableRow;

            if (row != null)
            {
                rowGroup.Rows.Remove(row);
            }
        }

        private void rtbEditor_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // Проверяем, находится ли курсор в таблице
            Table table = FindTableUnderSelection();

            // Если курсор находится на таблице, показываем пункты меню для таблицы
            if (table != null)
            {
                // Включаем пункты меню, связанные с таблицей
                cRemoveTable.Visibility = Visibility.Visible;
                cAddCollumnTable.Visibility = Visibility.Visible;
                cRemoveCollumnTable.Visibility = Visibility.Visible;
                cAddRowTable.Visibility = Visibility.Visible;
                cRemoveRowTable.Visibility = Visibility.Visible;
            }
            else
            {
                cRemoveTable.Visibility = Visibility.Collapsed;
                cAddCollumnTable.Visibility = Visibility.Collapsed;
                cRemoveCollumnTable.Visibility = Visibility.Collapsed;
                cAddRowTable.Visibility = Visibility.Collapsed;
                cRemoveRowTable.Visibility = Visibility.Collapsed;
            }
        }

        private void rtbEditor_SelectionChanged(object sender, RoutedEventArgs e)
        {
            TextRange selection = new TextRange(rtbEditor.Selection.Start, rtbEditor.Selection.End);
            if (!selection.IsEmpty)
            {
                object fontSize = selection.GetPropertyValue(TextElement.FontSizeProperty);
                if (fontSize != DependencyProperty.UnsetValue)
                {
                    cmbFontSize.Text = fontSize.ToString();
                }
            }
        }

        private void ToggleList(bool isNumbered)
        {
            TextSelection selection = rtbEditor.Selection;
            if (!selection.IsEmpty)
            {
                // Проверяем, уже ли внутри списка
                List currentList = selection.Start.Paragraph?.Parent as List;

                if (currentList == null) // Если выделение не внутри списка — создаём новый список
                {
                    List newList = new List
                    {
                        MarkerStyle = isNumbered ? TextMarkerStyle.Decimal : TextMarkerStyle.Disc
                    };

                    ListItem listItem = new ListItem();

                    // Перемещаем выделенный текст в список
                    foreach (Block block in selection.Start.Paragraph?.SiblingBlocks)
                    {
                        if (block is Paragraph paragraph && selection.Contains(paragraph.ContentStart))
                        {
                            listItem.Blocks.Add(new Paragraph(new Run(new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text)));
                        }
                    }

                    newList.ListItems.Add(listItem);
                    rtbEditor.Document.Blocks.Add(newList);
                }
            }
        }

        private void RemoveList()
        {
            TextSelection selection = rtbEditor.Selection;
            if (!selection.IsEmpty)
            {
                List currentList = selection.Start.Paragraph?.Parent as List;
                if (currentList != null)
                {
                    List<Block> paragraphs = new List<Block>();

                    foreach (ListItem item in currentList.ListItems)
                    {
                        foreach (Block block in item.Blocks)
                        {
                            if (block is Paragraph paragraph)
                            {
                                paragraphs.Add(new Paragraph(new Run(new TextRange(paragraph.ContentStart, paragraph.ContentEnd).Text)));
                            }
                        }
                    }

                    // Удаляем старый список
                    rtbEditor.Document.Blocks.Remove(currentList);

                    // Добавляем параграфы вместо списка
                    foreach (Block paragraph in paragraphs)
                    {
                        rtbEditor.Document.Blocks.Add(paragraph);
                    }
                }
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            WindowHelper.BringWindowToFront<ThemeEditor>();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            WindowHelper.BringWindowToFront<ThemeManager>();
        }

        private void CreateListParameter(object sender, RoutedEventArgs e)
        {
            Button button = sender as Button;

            // Извлекаем параметр из свойства Tag и приводим его к типу double
            bool parameter = (Boolean)button.Tag;

			ToggleList(parameter);
        }

        private void InsertList_Click(object sender, RoutedEventArgs e)
        {
			if (ListParameter.Visibility != Visibility.Visible)
				ListParameter.Visibility = Visibility.Visible;
			else
				ListParameter.Visibility = Visibility.Hidden;
        }

        private async void CheckUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateInfo updateInfo = await UpdateService.GetUpdateInfoAsync();
                List<UpdateInfoRow> updateRows = new List<UpdateInfoRow>
				{
					new UpdateInfoRow { RemoteValue = updateInfo.RemoteVersionType, LocalValue = updateInfo.LocalVersionType },
					new UpdateInfoRow { RemoteValue = updateInfo.RemoteVersionID, LocalValue = updateInfo.LocalVersionID }
				};

				bool updateavialble;
				if (updateInfo.IsUpdateAvailable) updateavialble = true;
				else updateavialble = false;
                UpdateStatusText.Text = updateInfo.IsUpdateAvailable ? (string)Application.Current.Resources["us.versionavilable"] : (string)Application.Current.Resources["us.versionislatest"];

                UpdateTable.ItemsSource = updateRows;
				UpdateMessage.Visibility = Visibility.Visible;
				if (updateavialble) InstallUpdate.Visibility = Visibility.Visible;
                InstallUpdate.Visibility = Visibility.Visible;

                Storyboard storyboard = (Storyboard)UpdateMessage.Resources["BorderAnimation"];
                storyboard.Begin();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при проверке обновлений: {ex.Message}");
            }

            //UpdateStatusText.Text = updateInfo.IsUpdateAvailable ? "Доступно обновление!" : "У вас последняя версия.";

            //UpdateTable.ItemsSource = new List<UpdateInfo> { updateInfo };

            //Storyboard storyboard = (Storyboard)UpdateMessage.Resources["BorderAnimation"];
            //storyboard.Begin();

        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            Storyboard hideStoryboard = (Storyboard)UpdateMessage.Resources["HideAnimation"];

            // Запускаем анимацию
            hideStoryboard.Begin();
        }

        private void DoubleAnimation_Completed(object sender, EventArgs e)
        {
            UpdateMessage.Visibility = Visibility.Hidden;
        }

        private void ToggleFocusMode(bool enable)
        {
            isFocusMode = !isFocusMode;

			if (enable) Items.Height = new GridLength(0);
			else Items.Height = new GridLength(50);
        }

        private void SearchBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string query = SearchBox.Text;
                bool found = textSearcher.FindNext(query);

                if (!found)
                    MessageBox.Show("Текст не найден", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void SearchBack_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchBox.Text;
            bool found = textSearcher.FindPrevious(query);

            if (!found)
                MessageBox.Show("Предыдущих совпадений нет", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void SearchNext_Click(object sender, RoutedEventArgs e)
        {
            string query = SearchBox.Text;
            bool found = textSearcher.FindNext(query);

            if (!found)
                MessageBox.Show("Совпадений больше нет", "Поиск", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void rtbEditor_KeyDown(object sender, KeyEventArgs e)
        {
            if ((Keyboard.Modifiers & ModifierKeys.Control) != 0 && e.Key == Key.F)
            {
                Search.Visibility = Visibility.Visible;
            }
        }

        private async void InstallUpdate_Click(object sender, RoutedEventArgs e)
        {
			MessageBox.Show("yes, sir!");
            await UpdateService.DownloadAndInstallUpdateAsync();
        }
    }
}
