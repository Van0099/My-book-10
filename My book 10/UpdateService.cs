using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Diagnostics;
using System.IO.Packaging;


public class UpdateService
{
    private const string VersionInfoUrl = "https://raw.githubusercontent.com/Van0099/My-book-10/master/versioninfo.json";
    private const string CurrentVersionFile = "versioninfo.json";
    private const string DownloadUrl = "https://github.com/Van0099/My-book-10/releases/latest/download/My_book10_update.zip";
    private const string UpdateFilePath = "update.zip";

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

    public static async Task DownloadAndInstallUpdateAsync()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                byte[] data = await client.GetByteArrayAsync(DownloadUrl);
                await Task.Run(() => File.WriteAllBytes(UpdateFilePath, data));
            }

            string extractPath = AppDomain.CurrentDomain.BaseDirectory;
            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true);
            }

            if (Directory.Exists(extractPath))
            {
                Directory.Delete(extractPath, true); // Удаляем старые файлы перед распаковкой
            }

            ZipFile.ExtractToDirectory(UpdateFilePath, extractPath);
            File.Delete(UpdateFilePath);
            
            Process.Start(new ProcessStartInfo
            {
                FileName = "MyBook.exe",
                UseShellExecute = true
            });

            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            Console.WriteLine("Ошибка при установке обновления: " + ex.Message);
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

    public class UpdateInfo
    {
        public string RemoteVersionType { get; set; }
        public string LocalVersionType { get; set; }
        public string RemoteVersionID { get; set; }
        public string LocalVersionID { get; set; }
        public bool IsUpdateAvailable { get; set; }
    }

    public static async Task<UpdateInfo> GetUpdateInfoAsync()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string remoteJson = await client.GetStringAsync(VersionInfoUrl);
                var remoteVersion = JsonConvert.DeserializeObject<VersionInfo>(remoteJson);

                if (remoteVersion == null)
                    return null;

                var localVersion = await VersionInfo_.LoadFromFileAsync(CurrentVersionFile);

                return new UpdateInfo
                {
                    RemoteVersionType = remoteVersion.VersionType,
                    LocalVersionType = localVersion?.VersionType ?? "Неизвестно",
                    RemoteVersionID = remoteVersion.VersionID,
                    LocalVersionID = localVersion?.VersionID ?? "Неизвестно",
                    IsUpdateAvailable = localVersion == null || remoteVersion.VersionID != localVersion.VersionID
                };
            }
        }
        catch
        {
            return null;
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

        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<VersionInfo>(json);
    }
}

public class UpdateInfoRow
{
    public string RemoteValue { get; set; }
    public string LocalValue { get; set; }
}
