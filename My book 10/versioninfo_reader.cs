using System;
using System.IO;
using Newtonsoft.Json;

namespace My_book_10
{
    public class VersionInfoReader
    {
        private string filePath = "versioninfo.json"; // Путь к файлу в корне проекта

        public VersionInfo ReadVersionInfo()
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Файл версии не найден", filePath);
            }

            string json = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<VersionInfo>(json);
        }
    }
}
