using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.Windows;

public class UpdateService
{
    private const string VersionInfoUrl = "https://raw.githubusercontent.com/Van0099/My-book-10/main/versioninfo.json";
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

                var localVersion = usVersionInfo.LoadFromFile(CurrentVersionFile);

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
            MessageBox.Show("Доступна новая версия My book!", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            MessageBox.Show("У вас установлена последняя версия My book", "Обновление", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

public class usVersionInfo
{
    public string PublicVersion { get; set; }
    public string VersionType { get; set; }
    public string VersionID { get; set; }

    public static VersionInfo LoadFromFile(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<VersionInfo>(json);
    }
}
