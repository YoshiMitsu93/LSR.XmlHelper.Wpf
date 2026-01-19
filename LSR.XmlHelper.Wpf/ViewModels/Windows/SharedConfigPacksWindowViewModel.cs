using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using LSR.XmlHelper.Wpf.Services.EditHistory;
using LSR.XmlHelper.Wpf.Services.SharedConfigs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class SharedConfigPacksWindowViewModel : ObservableObject
    {
        private readonly Func<string?> _getRootFolder;
        private readonly AppSettings _settings;
        private readonly SharedConfigPackService _packs;
        private readonly AppearanceService _appearance;
        private readonly EditHistoryService _editHistory;
        private readonly XmlBackupRequestService _backup;

        private string _packName = "ConfigPack";
        private string _description = "";
        private bool _includeAppearance = false;

        private string? _selectedPackPath;
        private SharedConfigPack? _selectedPack;

        private bool _importAppearance = true;
        private bool _importEdits = true;
        private bool _importEditsAsPending = true;
        private bool _applyNow = false;

        private string _previewText = "";

        public SharedConfigPacksWindowViewModel(
            Func<string?> getRootFolder,
            AppSettings settings,
            AppearanceService appearance,
            SharedConfigPackService packs,
            EditHistoryService editHistory,
            XmlBackupRequestService backup)
        {
            _getRootFolder = getRootFolder;
            _settings = settings;
            _appearance = appearance;
            _packs = packs;
            _editHistory = editHistory;
            _backup = backup;

            RefreshPacksCommand = new RelayCommand(RefreshPacks, CanUseRootFolder);
            OpenConfigsFolderCommand = new RelayCommand(OpenConfigsFolder, CanUseRootFolder);

            ExportSelectAllPendingCommand = new RelayCommand(() => SetAll(ExportPending, true));
            ExportSelectNonePendingCommand = new RelayCommand(() => SetAll(ExportPending, false));
            ExportSelectAllCommittedCommand = new RelayCommand(() => SetAll(ExportCommitted, true));
            ExportSelectNoneCommittedCommand = new RelayCommand(() => SetAll(ExportCommitted, false));

            ImportSelectAllPendingCommand = new RelayCommand(() => SetAll(ImportPending, true), () => SelectedPack?.EditHistory?.Pending?.Count > 0);
            ImportSelectNonePendingCommand = new RelayCommand(() => SetAll(ImportPending, false), () => SelectedPack?.EditHistory?.Pending?.Count > 0);
            ImportSelectAllCommittedCommand = new RelayCommand(() => SetAll(ImportCommitted, true), () => SelectedPack?.EditHistory?.Committed?.Count > 0);
            ImportSelectNoneCommittedCommand = new RelayCommand(() => SetAll(ImportCommitted, false), () => SelectedPack?.EditHistory?.Committed?.Count > 0);

            ExportCommand = new RelayCommand(Export, CanUseRootFolder);
            ExportSummaryCommand = new RelayCommand(ExportSummary, CanUseRootFolder);
            ImportCommand = new RelayCommand(ImportSelected, CanImportSelected);
            RebuildPreviewCommand = new RelayCommand(RebuildPreview);

            BuildExportLists();
            RefreshPacks();
        }

        public ObservableCollection<string> PackFiles { get; } = new ObservableCollection<string>();

        public ObservableCollection<SelectableEditHistoryItemViewModel> ExportPending { get; } = new ObservableCollection<SelectableEditHistoryItemViewModel>();
        public ObservableCollection<SelectableEditHistoryItemViewModel> ExportCommitted { get; } = new ObservableCollection<SelectableEditHistoryItemViewModel>();

        public ObservableCollection<SelectableEditHistoryItemViewModel> ImportPending { get; } = new ObservableCollection<SelectableEditHistoryItemViewModel>();
        public ObservableCollection<SelectableEditHistoryItemViewModel> ImportCommitted { get; } = new ObservableCollection<SelectableEditHistoryItemViewModel>();

        public string PackName
        {
            get => _packName;
            set => SetProperty(ref _packName, value ?? "");
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value ?? "");
        }

        public bool IncludeAppearance
        {
            get => _includeAppearance;
            set
            {
                if (SetProperty(ref _includeAppearance, value))
                    RebuildPreview();
            }
        }

        public AppearanceService Appearance => _appearance;

        public string? SelectedPackPath
        {
            get => _selectedPackPath;
            set
            {
                if (SetProperty(ref _selectedPackPath, value))
                {
                    LoadSelectedPack();
                    RebuildPreview();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public SharedConfigPack? SelectedPack
        {
            get => _selectedPack;
            private set => SetProperty(ref _selectedPack, value);
        }

        public bool ImportAppearance
        {
            get => _importAppearance;
            set
            {
                if (SetProperty(ref _importAppearance, value))
                {
                    RebuildPreview();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool ImportEdits
        {
            get => _importEdits;
            set
            {
                if (SetProperty(ref _importEdits, value))
                {
                    RebuildPreview();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool ImportEditsAsPending
        {
            get => _importEditsAsPending;
            set
            {
                if (SetProperty(ref _importEditsAsPending, value))
                    RebuildPreview();
            }
        }
        public bool ImportEditsAsCommitted
        {
            get => !_importEditsAsPending;
            set
            {
                var pending = !value;
                if (SetProperty(ref _importEditsAsPending, pending, nameof(ImportEditsAsPending)))
                    RebuildPreview();
            }
        }

        public bool ApplyNow
        {
            get => _applyNow;
            set
            {
                if (SetProperty(ref _applyNow, value))
                    RebuildPreview();
            }
        }

        public string PreviewText
        {
            get => _previewText;
            private set => SetProperty(ref _previewText, value);
        }

        public RelayCommand RefreshPacksCommand { get; }
        public RelayCommand OpenConfigsFolderCommand { get; }
        public RelayCommand ExportSelectAllPendingCommand { get; }
        public RelayCommand ExportSelectNonePendingCommand { get; }
        public RelayCommand ExportSelectAllCommittedCommand { get; }
        public RelayCommand ExportSelectNoneCommittedCommand { get; }
        public RelayCommand ImportSelectAllPendingCommand { get; }
        public RelayCommand ImportSelectNonePendingCommand { get; }
        public RelayCommand ImportSelectAllCommittedCommand { get; }
        public RelayCommand ImportSelectNoneCommittedCommand { get; }
        public RelayCommand ExportCommand { get; }
        public RelayCommand ExportSummaryCommand { get; }
        public RelayCommand ImportCommand { get; }
        public RelayCommand RebuildPreviewCommand { get; }

        private bool CanUseRootFolder()
        {
            return !string.IsNullOrWhiteSpace(_getRootFolder());
        }

        private void BuildExportLists()
        {
            ExportPending.Clear();
            ExportCommitted.Clear();

            foreach (var e in _settings.EditHistory.Pending.OrderByDescending(x => x.TimestampUtc))
                ExportPending.Add(new SelectableEditHistoryItemViewModel(e, true));

            foreach (var e in _settings.EditHistory.Committed.OrderByDescending(x => x.TimestampUtc))
                ExportCommitted.Add(new SelectableEditHistoryItemViewModel(e, true));
        }

        private void RefreshPacks()
        {
            PackFiles.Clear();

            var root = _getRootFolder();
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                return;

            foreach (var f in _packs.GetConfigPackFiles(root))
                PackFiles.Add(f);

            SelectedPackPath = PackFiles.FirstOrDefault();
        }

        private void OpenConfigsFolder()
        {
            var root = _getRootFolder();
            if (string.IsNullOrWhiteSpace(root))
                return;

            _packs.OpenSharedConfigsFolder(root);
        }

        private void Export()
        {
            var root = _getRootFolder();
            if (string.IsNullOrWhiteSpace(root))
            {
                System.Windows.MessageBox.Show("Open a folder first.", "Shared Config Packs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var pending = ExportPending.Where(x => x.IsSelected).Select(x => x.Item).ToList();
            var committed = ExportCommitted.Where(x => x.IsSelected).Select(x => x.Item).ToList();

            var path = _packs.Export(
                root,
                _settings,
                PackName,
                Description,
                IncludeAppearance,
                pending,
                committed);

            System.Windows.MessageBox.Show($"Exported:\n{path}", "Shared Config Packs", MessageBoxButton.OK, MessageBoxImage.Information);

            RefreshPacks();
            SelectedPackPath = path;
        }

        private void ExportSummary()
        {
            var root = _getRootFolder();
            if (string.IsNullOrWhiteSpace(root))
                return;

            var pending = ExportPending.Where(x => x.IsSelected).Select(x => x.Item).ToList();
            var committed = ExportCommitted.Where(x => x.IsSelected).Select(x => x.Item).ToList();

            if (pending.Count == 0 && committed.Count == 0)
            {
                System.Windows.MessageBox.Show("Select at least one Pending or Committed edit to export a summary.", "Config Packs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var summariesFolder = Path.Combine(root, "LSR-XML-Helper", "Summaries");
            Directory.CreateDirectory(summariesFolder);

            var defaultName = (PackName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(defaultName))
                defaultName = $"ConfigPackSummary_{DateTime.Now:yyyyMMdd_HHmmss}";

            var optVm = new LSR.XmlHelper.Wpf.ViewModels.Dialogs.EditSummaryExportOptionsViewModel
            {
                FileName = defaultName,
                Notes = Description ?? ""
            };

            var optWin = new LSR.XmlHelper.Wpf.Views.EditSummaryExportOptionsWindow
            {
                Owner = System.Windows.Application.Current?.MainWindow,
                DataContext = optVm
            };

            if (optWin.ShowDialog() != true)
                return;

            var safeName = (optVm.FileName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(safeName))
                safeName = defaultName;

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Edit Summary",
                Filter = "Text File (*.txt)|*.txt",
                FileName = $"{safeName}.txt",
                InitialDirectory = summariesFolder
            };

            if (dialog.ShowDialog() != true)
                return;

            var exporter = new LSR.XmlHelper.Wpf.Services.EditSummary.EditSummaryExportService();
            var text = exporter.BuildSummary(root, safeName, optVm.Notes, pending, committed);

            File.WriteAllText(dialog.FileName, text);

            var files = pending.Select(x => x.FilePath)
                .Concat(committed.Select(x => x.FilePath))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Count();

            var edits = pending.Count + committed.Count;

            System.Windows.MessageBox.Show($"Exported summary for {files} files / {edits} edits.", "Config Packs", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void LoadSelectedPack()
        {
            SelectedPack = null;
            ImportPending.Clear();
            ImportCommitted.Clear();

            if (string.IsNullOrWhiteSpace(SelectedPackPath) || !File.Exists(SelectedPackPath))
                return;

            try
            {
                SelectedPack = _packs.Load(SelectedPackPath);

                if (SelectedPack.EditHistory is not null)
                {
                    foreach (var e in SelectedPack.EditHistory.Pending.OrderByDescending(x => x.TimestampUtc))
                        ImportPending.Add(new SelectableEditHistoryItemViewModel(e, true));

                    foreach (var e in SelectedPack.EditHistory.Committed.OrderByDescending(x => x.TimestampUtc))
                        ImportCommitted.Add(new SelectableEditHistoryItemViewModel(e, true));
                }

                ImportAppearance = SelectedPack.Appearance is not null;
                ImportEdits = (SelectedPack.EditHistory?.Pending?.Count > 0) || (SelectedPack.EditHistory?.Committed?.Count > 0);

                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Failed to load config pack", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanImportSelected()
        {
            if (SelectedPack is null)
                return false;

            if (ImportAppearance && SelectedPack.Appearance is null)
                return false;

            if (ImportEdits)
            {
                var anySelected = ImportPending.Any(x => x.IsSelected) || ImportCommitted.Any(x => x.IsSelected);
                if (!anySelected)
                    return false;
            }

            return ImportAppearance || ImportEdits;
        }

        private void ImportSelected()
        {
            if (SelectedPack is null)
                return;

            var pending = ImportPending.Where(x => x.IsSelected).Select(x => x.Item).ToList();
            var committed = ImportCommitted.Where(x => x.IsSelected).Select(x => x.Item).ToList();

            if (ImportEdits && pending.Count == 0 && committed.Count == 0)
            {
                System.Windows.MessageBox.Show("Select at least one edit item to import.", "Shared Config Packs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (ApplyNow && ImportEdits)
            {
                var all = pending.Concat(committed).ToList();
                var ok = TryApplyEditsToDisk(all);
                if (!ok)
                    return;
            }

            try
            {
                _packs.ImportInto(
                    _settings,
                    SelectedPack,
                    ImportAppearance,
                    ImportEdits,
                    pending,
                    committed,
                    ImportEditsAsPending);

                System.Windows.MessageBox.Show("Imported successfully.", "Shared Config Packs", MessageBoxButton.OK, MessageBoxImage.Information);

                BuildExportLists();
                RebuildPreview();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Import failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool TryApplyEditsToDisk(IReadOnlyList<EditHistoryItem> edits)
        {
            var byFile = edits
                .Where(e => !string.IsNullOrWhiteSpace(e.FilePath))
                .GroupBy(e => e.FilePath!, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (byFile.Count == 0)
            {
                System.Windows.MessageBox.Show("No file paths found in selected edits.", "Shared Config Packs", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var confirm = System.Windows.MessageBox.Show(
                $"Apply {edits.Count} edit(s) to {byFile.Count} file(s) now?\n\nThis will modify XML files on disk (backups will be created).",
                "Apply Now",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (confirm != MessageBoxResult.Yes)
                return false;

            foreach (var g in byFile)
            {
                if (!_backup.TryBackup(g.Key, out var backupErr))
                {
                    System.Windows.MessageBox.Show(backupErr ?? "Backup failed.", "Apply Now", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                if (!_editHistory.TryLoadFileAndApply(g.Key, g.ToList(), out var updated, out var err))
                {
                    System.Windows.MessageBox.Show(err ?? "Apply failed.", "Apply Now", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                File.WriteAllText(g.Key, updated);
            }

            return true;
        }

        private void RebuildPreview()
        {
            PreviewText = string.Empty;
            if (SelectedPack is null)
            {
                PreviewText = "No pack selected.";
                return;
            }

            var lines = new List<string>();

            lines.Add($"Pack: {SelectedPack.Name}");
            lines.Add($"Created (UTC): {SelectedPack.CreatedUtc:u}");
            lines.Add("");

            if (ImportAppearance && SelectedPack.Appearance is not null)
            {
                var diffs = _packs.PreviewAppearanceChanges(_settings.Appearance, SelectedPack.Appearance);
                lines.Add($"Appearance: {diffs.Count} change(s)");
                foreach (var d in diffs.Take(50))
                    lines.Add($"- {d}");

                if (diffs.Count > 50)
                    lines.Add($"(and {diffs.Count - 50} more...)");

                lines.Add("");
            }
            else
            {
                lines.Add("Appearance: not importing");
                lines.Add("");
            }

            if (ImportEdits)
            {
                var pending = ImportPending.Where(x => x.IsSelected).ToList();
                var committed = ImportCommitted.Where(x => x.IsSelected).ToList();

                lines.Add($"Edits selected: {pending.Count + committed.Count}");
                lines.Add($"Import edits as: {(ImportEditsAsPending ? "Pending" : "Committed")}");
                lines.Add($"Apply now: {(ApplyNow ? "Yes (writes to disk)" : "No (history only)")}");
                lines.Add("");

                foreach (var e in pending.Select(x => x.Item).Concat(committed.Select(x => x.Item)).Take(100))
                    lines.Add($"- {e.EntryKey} | {e.FieldPath} | {e.OldValue} -> {e.NewValue} | {e.FilePath}");

                if (pending.Count + committed.Count > 100)
                    lines.Add($"(and {pending.Count + committed.Count - 100} more...)");
            }
            else
            {
                lines.Add("Edits: not importing");
            }

            PreviewText = string.Join(Environment.NewLine, lines);
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        private static void SetAll(ObservableCollection<SelectableEditHistoryItemViewModel> list, bool value)
        {
            foreach (var x in list)
                x.IsSelected = value;

            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }
}
