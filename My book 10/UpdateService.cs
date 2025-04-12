using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using System.IO.Compression;
using System.Diagnostics;
using System.Windows;
using System.Reflection;

public class UpdateService
{
    private const string VersionInfoUrl = "https://raw.githubusercontent.com/Van0099/My-book-10/master/versioninfo.json";
    private const string CurrentVersionFile = "versioninfo.json";
    private const string UpdateFilePath = "My_book10_update.zip";

    public static async Task<bool> IsUpdateAvailableAsync()
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string remoteJson = await client.GetStringAsync(VersionInfoUrl);
                var remoteVersion = JsonConvert.DeserializeObject<VersionInfous>(remoteJson);

                if (remoteVersion == null)
                    return false;

                var localVersion = await VersionInfous.LoadFromFileAsync(CurrentVersionFile);

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
        MessageBox.Show("ok");
        try
        {
            MessageBox.Show("ok2");
            using (HttpClient client = new HttpClient())
            {
                string remoteJson = await client.GetStringAsync(VersionInfoUrl);
                var remoteVersion = JsonConvert.DeserializeObject<VersionInfous>(remoteJson);
                byte[] data = await client.GetByteArrayAsync(remoteVersion.DownloadURL);
                await Task.Run(() => File.WriteAllBytes(UpdateFilePath, data));
                MessageBox.Show("процесс идёт");
            }

            string extractPath = AppDomain.CurrentDomain.BaseDirectory;
            string updaterPath = Path.Combine(extractPath, "updater", "MybookUpdater");

            // Сохранение текущего процесса
            string currentExe = Process.GetCurrentProcess().MainModule.FileName;

            string appDataPath = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
    "mybook");

            string updaterSourceDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "updater");
            string appDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "mybook");
            string updaterTargetDir = Path.Combine(appDataDir, "Updater");

            // Проверяем и копируем, если не существует
            if (!Directory.Exists(updaterTargetDir))
                Directory.CreateDirectory(updaterTargetDir);

            foreach (string file in Directory.GetFiles(updaterSourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(updaterTargetDir, fileName);
                File.Copy(file, destFile, true);
            }

            MessageBox.Show(UpdateFilePath);

            string updaterExe = Path.Combine(updaterTargetDir, "MyBookUpdater.exe");

            // Запускаем Updater.exe и передаём путь к текущему .exe
            Process.Start(new ProcessStartInfo
        {
            FileName = updaterExe,
            Arguments = $"\"{AppDomain.CurrentDomain.BaseDirectory}\" \"{UpdateFilePath}\"",
            UseShellExecute = true
        });

            // Завершаем текущее приложение
            Environment.Exit(0);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Ошибка при установке обновления: " + ex.Message);
        }
    }

    public static async Task CheckForUpdatesAsync()
    {
        bool isUpdateAvailable = await IsUpdateAvailableAsync();

        if (isUpdateAvailable)
        {
            Console.WriteLine("Доступно новое обновление!");
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

                var localVersion = await VersionInfous.LoadFromFileAsync(CurrentVersionFile);

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

public class VersionInfous
{
    public string DownloadURL { get; set; }
    public string VersionType { get; set; }
    public string VersionID { get; set; }

    public static async Task<VersionInfo> LoadFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
            return null;

        string json = await Task.Run(() => File.ReadAllText(filePath));
        return JsonConvert.DeserializeObject<VersionInfo>(json);
    }
}

public class UpdateInfoRow
{
    public string RemoteValue { get; set; }
    public string LocalValue { get; set; }
}
