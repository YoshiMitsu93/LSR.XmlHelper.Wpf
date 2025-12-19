using LSR.XmlHelper.Wpf.ViewModels;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class AppSettings
    {
        public string? LastFolder { get; set; }

        public string ViewMode { get; set; } = XmlListViewMode.Flat.ToString();

        public bool IncludeSubfolders { get; set; }

        public bool IsDarkMode { get; set; } = true;

        public bool IsFriendlyView { get; set; } = true;

        public AppearanceSettings Appearance { get; set; } = new AppearanceSettings();
    }
}