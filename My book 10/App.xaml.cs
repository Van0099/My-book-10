using MaterialDesignThemes.Wpf;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace My_book_10
{
	/// <summary>
	/// Логика взаимодействия для App.xaml
	/// </summary>
	/// 
	public class Settings
	{
		public string Theme { get; set; }
		public string Language { get; set; }
		public bool Spellcheck { get; set; }
        public bool IsCustomTheme { get; set; }
    }

	public partial class App : Application
	{
		public static string GetSettingsFilePath()
		{
			string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData); // Путь к AppData/Roaming
			string myBookPath = Path.Combine(appDataPath, "mybook"); // Создаём путь к каталогу mybook

			// Если папки ещё нет, создаём её
			if (!Directory.Exists(myBookPath))
			{
				Directory.CreateDirectory(myBookPath);
			}

			return Path.Combine(myBookPath, "settings.json"); // Возвращаем путь к файлу настроек
		}

		// Метод для создания файла настроек с дефолтными значениями
		public static void CreateDefaultSettingsFile()
		{
			string filePath = GetSettingsFilePath();

			// Проверяем, существует ли файл настроек
			if (!File.Exists(filePath))
			{
				Settings settings = new Settings
				{
					Theme = "light",
					Language = "ru",
					Spellcheck = false
				};

				// Сериализуем объект в JSON
				string json = JsonConvert.SerializeObject(settings, Formatting.Indented);

				// Сохраняем сериализованный JSON в файл
				File.WriteAllText(filePath, json);
			}
		}

		// Этот метод вызывается при старте приложения
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// Создаём файл настроек с дефолтными значениями, если его ещё нет
			CreateDefaultSettingsFile();
        }
    }
}
