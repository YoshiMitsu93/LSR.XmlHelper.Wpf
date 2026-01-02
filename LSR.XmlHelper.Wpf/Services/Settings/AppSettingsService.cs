using System;
using System.IO;
using System.Text.Json;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class AppSettingsService
    {
        private readonly string _settingsPath;

        public AppSettingsService()
        {
            var root = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LSR.XmlHelper.Wpf");

            Directory.CreateDirectory(root);
            _settingsPath = Path.Combine(root, "settings.json");
        }

        public string SettingsPath => _settingsPath;

        public AppSettings Load()
        {
            return Load(out _);
        
        }

        public AppSettings Load(out bool isFirstRun)
        {
            isFirstRun = false;

            try
            {
                if (!File.Exists(_settingsPath))
                {
                    isFirstRun = true;
                    var first = new AppSettings();
                    Save(first);
                    return first;
                }

                var json = File.ReadAllText(_settingsPath);
                var settings = JsonSerializer.Deserialize<AppSettings>(json);

                return settings ?? new AppSettings();
            }
            catch
            {
                return new AppSettings();
            }
        }

        public void Save(AppSettings settings)
        {
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_settingsPath, json);
        }
    }
}
