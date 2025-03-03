using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;

public class UpdateService
{
    private const string VersionInfoUrl = "https://github.com/Van0099/My-book-10/releases/latest/download/versioninfo.json";
    private const string CurrentVersionFile = "versioninfo.json";

    public static async Task<bool> IsUpdateAvailableAsync()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string remoteJson = await client.GetStringAsync(VersionInfoUrl);
                var remoteVersion = JsonConvert.DeserializeObject<VersionInfo>(remoteJson);

                if (remoteVersion == null)
                    return false;

                var localVersion = await VersionInfo_.LoadFromFileAsync(CurrentVersionFile);

                if (localVersion == null)
                    return true;

                return remoteVersion.VersionType == localVersion.VersionType &&
                       remoteVersion.VersionID != localVersion.VersionID;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при проверке обновлений: " + ex.Message);
            return false;
        }
    }

    public static async Task CheckForUpdatesAsync()
    {
        bool isUpdateAvailable = await IsUpdateAvailableAsync();

        if (isUpdateAvailable)
        {
            Console.WriteLine("Доступно новое обновление!");
            // Здесь можно добавить логику для загрузки и установки обновления
        }
        else
        {
            Console.WriteLine("У вас установлена последняя версия My Book.");
        }
    }
}

public class VersionInfo_
{
    public string PublicVersion { get; set; }
    public string VersionType { get; set; }
    public string VersionID { get; set; }

    public static async Task<VersionInfo> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        string json = await File.ReadAllTextAsync(filePath);
        return JsonConvert.DeserializeObject<VersionInfo>(json);
    }
}
