using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace My_book_10
{
	public class RecentFilesManager
	{
		// Указываем путь к файлу, в котором будем хранить недавно открытые файлы
		private static readonly string recentFilesPath = Path.Combine(
			Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
			"mybook", "recentFiles.json");

		// Метод для добавления файла в список недавно открытых
		public static void AddRecentlyOpenedFile(string filePath)
		{
			// Проверяем, существует ли файл с недавно открытыми файлами
			Dictionary<string, string> recentFiles = LoadRecentFiles();

			// Генерируем новый ключ для файла (например, file1, file2, ...)
			string newFileKey = $"file{recentFiles.Count + 1}";

			// Добавляем новый файл в словарь
			recentFiles[newFileKey] = filePath;

			// Ограничиваем количество файлов в списке (например, до 5)
			if (recentFiles.Count > 5)
			{
				// Удаляем первый (старый) файл из списка
				string firstKey = $"file1";
				recentFiles.Remove(firstKey);
			}

			// Сохраняем обновленный список файлов
			SaveRecentFiles(recentFiles);
		}

		// Метод для чтения списка недавно открытых файлов
		public static Dictionary<string, string> LoadRecentFiles()
		{
			if (File.Exists(recentFilesPath))
			{
				string json = File.ReadAllText(recentFilesPath);
				return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
			}
			return new Dictionary<string, string>();
		}

		// Метод для сохранения списка недавно открытых файлов в файл
		public static void SaveRecentFiles(Dictionary<string, string> recentFiles)
		{
			// Сериализуем словарь в JSON
			string json = JsonConvert.SerializeObject(recentFiles, Formatting.Indented);

			// Создаем директорию, если её нет
			Directory.CreateDirectory(Path.GetDirectoryName(recentFilesPath));

			// Записываем JSON в файл
			File.WriteAllText(recentFilesPath, json);
		}
	}
}
