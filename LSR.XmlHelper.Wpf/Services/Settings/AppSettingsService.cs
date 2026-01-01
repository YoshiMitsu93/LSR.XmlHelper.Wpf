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

        public AppSettings Load()
        {
            try
            {
                if (!File.Exists(_settingsPath))
                    return new AppSettings();

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
