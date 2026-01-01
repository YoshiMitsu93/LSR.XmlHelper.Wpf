using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class BackupXmlFilterItem
    {
        public BackupXmlFilterItem(string id, string displayText)
        {
            Id = id;
            DisplayText = displayText;
        }

        public string Id { get; }
        public string DisplayText { get; }
    }

    public sealed class BackupFileListItem
    {
        public BackupFileListItem(string fullPath, string sourceBaseName, string? targetXmlPath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(fullPath);
            var info = new FileInfo(fullPath);
            LastWriteTime = info.LastWriteTime;
            SizeBytes = info.Length;
            SourceBaseName = sourceBaseName;
            SourceXmlDisplay = $"{sourceBaseName}.xml";
            TargetXmlPath = targetXmlPath;
        }

        public string FileName { get; }
        public string FullPath { get; }
        public DateTime LastWriteTime { get; }
        public long SizeBytes { get; }
        public string SourceBaseName { get; }
        public string SourceXmlDisplay { get; }
        public string? TargetXmlPath { get; }
        public bool TargetExists => !string.IsNullOrWhiteSpace(TargetXmlPath) && File.Exists(TargetXmlPath);
        public string LastWriteText => LastWriteTime.ToString("G");
        public string SizeText => FormatSize(SizeBytes);

        private static string FormatSize(long bytes)
        {
            if (bytes < 0)
                return "0 B";

            if (bytes < 1024)
                return $"{bytes} B";

            var kb = bytes / 1024d;
            if (kb < 1024)
                return $"{kb:0.##} KB";

            var mb = kb / 1024d;
            if (mb < 1024)
                return $"{mb:0.##} MB";

            var gb = mb / 1024d;
            return $"{gb:0.##} GB";
        }
    }


    public sealed class BackupBrowserWindowViewModel : ObservableObject
    {
        private const string AllFilterId = "__ALL__";

        private readonly List<BackupFileListItem> _allBackups = new();
        private ObservableCollection<BackupXmlFilterItem> _xmlFilters = new();
        private BackupXmlFilterItem? _selectedXmlFilter;

        private readonly XmlHelperRootService _root;
        private readonly XmlBackupService _backup;

        private ObservableCollection<BackupFileListItem> _backups = new();
        private BackupFileListItem? _selectedBackup;
        private string _status = "";

        public BackupBrowserWindowViewModel(string xmlPath, AppearanceService appearance)
        {
            XmlPath = xmlPath;
            Appearance = appearance;

            _root = new XmlHelperRootService();
            _backup = new XmlBackupService(_root);
            BackupFolder = _root.GetOrCreateSubfolder(xmlPath, "BackupXMLs");
            XmlFileName = Path.GetFileName(xmlPath);
            XmlDirectory = Path.GetDirectoryName(xmlPath) ?? "";

            RefreshCommand = new RelayCommand(Refresh);
            RestoreCommand = new RelayCommand(RestoreSelected, () => SelectedBackup is not null && SelectedBackup.TargetExists);
            OpenBackupFolderCommand = new RelayCommand(OpenBackupFolder, () => Directory.Exists(BackupFolder));
            CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(this, false));

            Refresh();
            SelectDefaultFilter();
        }


        public event EventHandler<bool>? CloseRequested;

        public AppearanceService Appearance { get; }

        public string XmlPath { get; }
        public string XmlFileName { get; }
        public string BackupFolder { get; }

        public string XmlDirectory { get; }
        public string? RestoredXmlPath { get; private set; }

        public ObservableCollection<BackupXmlFilterItem> XmlFilters
        {
            get => _xmlFilters;
            private set => SetProperty(ref _xmlFilters, value);
        }

        public BackupXmlFilterItem? SelectedXmlFilter
        {
            get => _selectedXmlFilter;
            set
            {
                if (SetProperty(ref _selectedXmlFilter, value))
                {
                    ApplyFilter();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }


        public ObservableCollection<BackupFileListItem> Backups
        {
            get => _backups;
            private set => SetProperty(ref _backups, value);
        }

        public BackupFileListItem? SelectedBackup
        {
            get => _selectedBackup;
            set
            {
                if (SetProperty(ref _selectedBackup, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

        public RelayCommand RefreshCommand { get; }
        public RelayCommand RestoreCommand { get; }
        public RelayCommand OpenBackupFolderCommand { get; }
        public RelayCommand CancelCommand { get; }

        private void Refresh()
        {
            try
            {
                _allBackups.Clear();

                if (!Directory.Exists(BackupFolder))
                {
                    Backups = new ObservableCollection<BackupFileListItem>();
                    SelectedBackup = null;
                    XmlFilters = new ObservableCollection<BackupXmlFilterItem>(new[]
                    {
                        new BackupXmlFilterItem(AllFilterId, "All backups")
                    });
                    _selectedXmlFilter = XmlFilters.FirstOrDefault();
                    OnPropertyChanged(nameof(SelectedXmlFilter));
                    Status = "No backups folder found.";
                    CommandManager.InvalidateRequerySuggested();
                    return;
                }

                foreach (var p in Directory.EnumerateFiles(BackupFolder, "*.xml", SearchOption.TopDirectoryOnly))
                {
                    var baseName = TryParseSourceBaseNameFromBackupFile(p);
                    if (string.IsNullOrWhiteSpace(baseName))
                        continue;

                    var target = ResolveTargetXmlPath(baseName);
                    _allBackups.Add(new BackupFileListItem(p, baseName, target));
                }

                var oldId = SelectedXmlFilter?.Id;
                RebuildFilters();

                var match = !string.IsNullOrWhiteSpace(oldId)
                    ? XmlFilters.FirstOrDefault(x => string.Equals(x.Id, oldId, StringComparison.OrdinalIgnoreCase))
                    : null;

                _selectedXmlFilter = match ?? XmlFilters.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedXmlFilter));

                ApplyFilter();
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                Backups = new ObservableCollection<BackupFileListItem>();
                SelectedBackup = null;
                XmlFilters = new ObservableCollection<BackupXmlFilterItem>(new[]
                {
                    new BackupXmlFilterItem(AllFilterId, "All backups")
                });
                _selectedXmlFilter = XmlFilters.FirstOrDefault();
                OnPropertyChanged(nameof(SelectedXmlFilter));
                Status = ex.Message;
                CommandManager.InvalidateRequerySuggested();
            }
        }


        private void SelectDefaultFilter()
        {
            var baseName = Path.GetFileNameWithoutExtension(XmlPath);
            var found = XmlFilters.FirstOrDefault(x => string.Equals(x.Id, baseName, StringComparison.OrdinalIgnoreCase));
            _selectedXmlFilter = found ?? XmlFilters.FirstOrDefault();
            OnPropertyChanged(nameof(SelectedXmlFilter));
            ApplyFilter();
        }

        private void RebuildFilters()
        {
            var baseNames = _allBackups.Select(x => x.SourceBaseName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var items = new List<BackupXmlFilterItem>
            {
                new BackupXmlFilterItem(AllFilterId, "All backups")
            };

            foreach (var b in baseNames)
                items.Add(new BackupXmlFilterItem(b, $"{b}.xml"));

            XmlFilters = new ObservableCollection<BackupXmlFilterItem>(items);
        }

        private void ApplyFilter()
        {
            var filter = SelectedXmlFilter?.Id;
            IEnumerable<BackupFileListItem> items = _allBackups;

            if (!string.IsNullOrWhiteSpace(filter) && !string.Equals(filter, AllFilterId, StringComparison.Ordinal))
                items = items.Where(x => string.Equals(x.SourceBaseName, filter, StringComparison.OrdinalIgnoreCase));

            var list = items.OrderByDescending(x => x.LastWriteTime).ToList();
            Backups = new ObservableCollection<BackupFileListItem>(list);
            SelectedBackup = Backups.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(filter) || string.Equals(filter, AllFilterId, StringComparison.Ordinal))
                Status = Backups.Count == 0 ? "No backups found." : $"{Backups.Count} backup(s) found.";
            else
                Status = Backups.Count == 0 ? "No backups found." : $"{Backups.Count} backup(s) found for {filter}.xml.";
        }

        public void AddDroppedFiles(string[] paths)
        {
            if (paths is null || paths.Length == 0)
                return;

            var addedAny = false;

            foreach (var p in paths)
            {
                if (string.IsNullOrWhiteSpace(p))
                    continue;

                if (!File.Exists(p))
                    continue;

                if (!string.Equals(Path.GetExtension(p), ".xml", StringComparison.OrdinalIgnoreCase))
                    continue;

                if (_allBackups.Any(x => string.Equals(x.FullPath, p, StringComparison.OrdinalIgnoreCase)))
                    continue;

                var baseName = TryParseSourceBaseNameFromBackupFile(p);
                if (string.IsNullOrWhiteSpace(baseName))
                    continue;

                var target = ResolveTargetXmlPath(baseName);
                _allBackups.Add(new BackupFileListItem(p, baseName, target));
                addedAny = true;
            }

            if (addedAny)
            {
                RebuildFilters();
                ApplyFilter();
                CommandManager.InvalidateRequerySuggested();
            }
        }

        private string? ResolveTargetXmlPath(string baseName)
        {
            if (string.IsNullOrWhiteSpace(XmlDirectory))
                return null;

            var expected = Path.Combine(XmlDirectory, $"{baseName}.xml");
            if (File.Exists(expected))
                return expected;

            try
            {
                var match = Directory.EnumerateFiles(XmlDirectory, "*.xml", SearchOption.TopDirectoryOnly)
                    .FirstOrDefault(p => string.Equals(Path.GetFileNameWithoutExtension(p), baseName, StringComparison.OrdinalIgnoreCase));

                return match;
            }
            catch
            {
                return null;
            }
        }

        private static string? TryParseSourceBaseNameFromBackupFile(string backupPath)
        {
            var name = Path.GetFileNameWithoutExtension(backupPath);
            if (string.IsNullOrWhiteSpace(name))
                return null;

            var tokens = name.Split('_');
            if (tokens.Length < 2)
                return null;

            var end = tokens.Length;

            if (end >= 1 && int.TryParse(tokens[end - 1], out _))
                end--;

            if (end < 2)
                return null;

            end--;
            if (end < 1)
                return null;

            return string.Join("_", tokens.Take(end));
        }


        private void OpenBackupFolder()
        {
            try
            {
                if (!Directory.Exists(BackupFolder))
                    return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"\"{BackupFolder}\"",
                    UseShellExecute = true
                });
            }
            catch
            {
            }
        }

        private void RestoreSelected()
        {
            if (SelectedBackup is null)
                return;

            if (!SelectedBackup.TargetExists)
            {
                MessageBox.Show("The target XML file could not be found for this backup.", "Restore from Backup", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var targetPath = SelectedBackup.TargetXmlPath ?? "";
            var targetName = Path.GetFileName(targetPath);

            var msg = $"Restore the selected backup to {targetName}?\n\nThis will overwrite the current XML file.\nA safety backup of the current XML will be created before restoring.";
            var confirm = MessageBox.Show(msg, "Restore from Backup", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                _backup.Backup(targetPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create safety backup: {ex.Message}", "Restore failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                File.Copy(SelectedBackup.FullPath, targetPath, overwrite: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Restore failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            RestoredXmlPath = targetPath;
            MessageBox.Show("Restore completed.", "Restore from Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            CloseRequested?.Invoke(this, true);
        }
    }
}
