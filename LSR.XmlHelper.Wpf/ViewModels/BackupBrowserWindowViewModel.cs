using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using System;
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
    public sealed class BackupFileListItem
    {
        public BackupFileListItem(string fullPath)
        {
            FullPath = fullPath;
            FileName = Path.GetFileName(fullPath);
            var info = new FileInfo(fullPath);
            LastWriteTime = info.LastWriteTime;
            SizeBytes = info.Length;
        }

        public string FileName { get; }
        public string FullPath { get; }
        public DateTime LastWriteTime { get; }
        public long SizeBytes { get; }

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

            RefreshCommand = new RelayCommand(Refresh);
            RestoreCommand = new RelayCommand(RestoreSelected, () => SelectedBackup is not null);
            OpenBackupFolderCommand = new RelayCommand(OpenBackupFolder, () => Directory.Exists(BackupFolder));
            CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(this, false));

            Refresh();
        }

        public event EventHandler<bool>? CloseRequested;

        public AppearanceService Appearance { get; }

        public string XmlPath { get; }
        public string XmlFileName { get; }
        public string BackupFolder { get; }

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
                if (!Directory.Exists(BackupFolder))
                {
                    Backups = new ObservableCollection<BackupFileListItem>();
                    SelectedBackup = null;
                    Status = "No backups folder found.";
                    CommandManager.InvalidateRequerySuggested();
                    return;
                }

                var baseName = Path.GetFileNameWithoutExtension(XmlPath);
                var pattern = $"{baseName}_*.xml";
                var items = Directory.EnumerateFiles(BackupFolder, pattern, SearchOption.TopDirectoryOnly)
                    .Select(p => new BackupFileListItem(p))
                    .OrderByDescending(x => x.LastWriteTime)
                    .ToList();

                Backups = new ObservableCollection<BackupFileListItem>(items);
                SelectedBackup = Backups.FirstOrDefault();
                Status = Backups.Count == 0 ? "No backups found." : $"{Backups.Count} backup(s) found.";
                CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                Backups = new ObservableCollection<BackupFileListItem>();
                SelectedBackup = null;
                Status = ex.Message;
                CommandManager.InvalidateRequerySuggested();
            }
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

            var msg = "Restore the selected backup?\n\nThis will overwrite the current XML file.\nA safety backup of the current XML will be created before restoring.";
            var confirm = MessageBox.Show(msg, "Restore from Backup", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return;

            try
            {
                _backup.Backup(XmlPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to create safety backup: {ex.Message}", "Restore failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                File.Copy(SelectedBackup.FullPath, XmlPath, overwrite: true);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Restore failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show("Restore completed.", "Restore from Backup", MessageBoxButton.OK, MessageBoxImage.Information);
            CloseRequested?.Invoke(this, true);
        }
    }
}
