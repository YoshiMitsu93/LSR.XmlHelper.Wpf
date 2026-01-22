using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LSR.XmlHelper.Wpf.ViewModels.Windows
{
    public sealed class SettingsInfoWindowViewModel : ObservableObject
    {
        private readonly MainWindowViewModel _main;
        private readonly AppSettingsService _settingsService;

        public SettingsInfoWindowViewModel(MainWindowViewModel main, AppSettingsService settingsService, AppearanceService appearance)
        {
            _main = main;
            _settingsService = settingsService;
            Appearance = appearance;
            OpenRepoCommand = new RelayCommand(OpenRepo);
            OpenReleasesCommand = new RelayCommand(OpenReleases);
            OpenSettingsFolderCommand = new RelayCommand(OpenSettingsFolder);
        }
        public AppearanceService Appearance { get; }

        public string AppVersion
        {
            get
            {
                var v = Assembly.GetExecutingAssembly().GetName().Version;
                return v is null ? "Unknown" : v.ToString(3);
            }
        }

        public string? LoadedFolderPath => _main.RootFolderPath;

        public string SettingsFilePath => _settingsService.SettingsPath;

        public bool IsDarkMode
        {
            get => _main.IsDarkMode;
            set
            {
                if (_main.IsDarkMode == value)
                    return;

                _main.IsDarkMode = value;
                OnPropertyChanged();
            }
        }

        public bool IsFriendlyView
        {
            get => _main.IsFriendlyView;
            set
            {
                if (_main.IsFriendlyView == value)
                    return;

                _main.IsFriendlyView = value;
                OnPropertyChanged();
            }
        }

        public bool IncludeSubfolders
        {
            get => _main.IncludeSubfolders;
            set
            {
                if (_main.IncludeSubfolders == value)
                    return;

                _main.IncludeSubfolders = value;
                OnPropertyChanged();
            }
        }
        public bool IsScopeShadingEnabled
        {
            get => _main.IsScopeShadingEnabled;
            set
            {
                if (_main.IsScopeShadingEnabled == value)
                    return;

                _main.IsScopeShadingEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsRegionHighlightEnabled
        {
            get => _main.IsRegionHighlightEnabled;
            set
            {
                if (_main.IsRegionHighlightEnabled == value)
                    return;

                _main.IsRegionHighlightEnabled = value;
                OnPropertyChanged();
            }
        }
        public bool IsIndentGuidesEnabled
        {
            get => _main.IsIndentGuidesEnabled;
            set
            {
                if (_main.IsIndentGuidesEnabled == value)
                    return;

                _main.IsIndentGuidesEnabled = value;
                OnPropertyChanged();
            }
        }

        public bool IsRawOutlineEnabled
        {
            get => _main.IsRawOutlineEnabled;
            set
            {
                if (_main.IsRawOutlineEnabled == value)
                    return;

                _main.IsRawOutlineEnabled = value;
                OnPropertyChanged();
            }
        }
        public XmlListViewMode ViewMode
        {
            get => _main.ViewMode;
            set
            {
                if (_main.ViewMode == value)
                    return;

                _main.ViewMode = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand OpenRepoCommand { get; }
        public RelayCommand OpenReleasesCommand { get; }
        public RelayCommand OpenSettingsFolderCommand { get; }

        private void OpenRepo()
        {
            OpenUrl("https://github.com/YoshiMitsu93/LSR.XmlHelper.Wpf");
        }

        private void OpenReleases()
        {
            OpenUrl("https://github.com/YoshiMitsu93/LSR.XmlHelper.Wpf/releases");
        }

        private void OpenSettingsFolder()
        {
            var folder = Path.GetDirectoryName(SettingsFilePath);
            if (string.IsNullOrWhiteSpace(folder))
                return;

            Process.Start(new ProcessStartInfo(folder) { UseShellExecute = true });
        }

        private void OpenUrl(string url)
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}
