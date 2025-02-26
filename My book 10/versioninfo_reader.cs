using System;
using System.IO;
using Newtonsoft.Json;

namespace My_book_10
{
    public class VersionReader
    {
        private string versionFilePath;

        public VersionReader(string filePath)
        {
            versionFilePath = filePath;
        }

        public VersionInfo LoadVersion()
        {
            if (!File.Exists(versionFilePath))
            {
                throw new FileNotFoundException("Файл версии не найден: " + versionFilePath);
            }

            string jsonContent = File.ReadAllText(versionFilePath);
            return JsonConvert.DeserializeObject<VersionInfo>(jsonContent);
        }
    }
}
