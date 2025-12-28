using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Card_Addiction_POS_System.Data.Settings
{
    /// <summary>
    /// Stores settings in %AppData%\Card_Addiction_POS_System\settings.json
    /// </summary>
    public sealed class JsonSettingsStore : ISettingsStore
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        private readonly string _path;

        public JsonSettingsStore(string path)
        {
            _path = path;
        }

        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(_path))
                    return AppSettings.Default;

                var json = File.ReadAllText(_path);
                return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? AppSettings.Default;
            }
            catch
            {
                return AppSettings.Default;
            }
        }

        public void Save(AppSettings settings)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(_path, json);
        }

        public void Reset()
        {
            if (File.Exists(_path))
                File.Delete(_path);
        }
    }

    public static class AppPaths
    {
        public static string AppDataDir =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Card_Addiction_POS_System");

        public static string SettingsPath => Path.Combine(AppDataDir, "settings.json");
    }
}
