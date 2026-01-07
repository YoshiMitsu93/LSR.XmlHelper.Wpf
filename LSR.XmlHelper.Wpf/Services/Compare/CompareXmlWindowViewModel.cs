using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services.Compare;
using LSR.XmlHelper.Wpf.Services.EditHistory;
using LSR.XmlHelper.Wpf.ViewModels;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace LSR.XmlHelper.Wpf.ViewModels.Windows
{
    public sealed class CompareXmlWindowViewModel : ObservableObject
    {
        private readonly XmlCompareService _comparer;
        private readonly CompareEditsImportService _importer;
        private readonly CompareEditsApplyService _applier;
        private readonly string? _currentOpenPathSnapshot;
        private readonly string _currentOpenXmlTextSnapshot;
        private readonly ObservableCollection<SelectableEditHistoryItemViewModel> _rows;

        private XmlFileListItem? _selectedTarget;
        private string? _externalFilePath;
        private string _status = "";

        public CompareXmlWindowViewModel(
            List<XmlFileListItem> targetFiles,
            string? preferredTargetPath,
            string? initialExternalFilePath,
            string? currentOpenPathSnapshot,
            string currentOpenXmlTextSnapshot,
            XmlCompareService comparer,
            CompareEditsImportService importer,
            CompareEditsApplyService applier)
        {
            _comparer = comparer;
            _importer = importer;
            _applier = applier;
            _currentOpenPathSnapshot = currentOpenPathSnapshot;
            _currentOpenXmlTextSnapshot = currentOpenXmlTextSnapshot ?? "";

            TargetFiles = new ObservableCollection<XmlFileListItem>(targetFiles ?? new List<XmlFileListItem>());
            _rows = new ObservableCollection<SelectableEditHistoryItemViewModel>();

            if (!string.IsNullOrWhiteSpace(preferredTargetPath))
                SelectedTarget = TargetFiles.FirstOrDefault(x => string.Equals(x.FullPath, preferredTargetPath, StringComparison.OrdinalIgnoreCase));

            SelectedTarget ??= TargetFiles.FirstOrDefault();

            BrowseExternalCommand = new RelayCommand(BrowseExternal, () => true);
            ClearExternalCommand = new RelayCommand(ClearExternal, () => !string.IsNullOrWhiteSpace(ExternalFilePath));
            SelectAllCommand = new RelayCommand(SelectAll, () => Rows.Count > 0);
            SelectNoneCommand = new RelayCommand(SelectNone, () => Rows.Count > 0);
            ImportAsPendingCommand = new RelayCommand(ImportAsPending, () => SelectedItems.Count > 0);
            ImportAsCommittedCommand = new RelayCommand(ImportAsCommitted, () => SelectedItems.Count > 0);
            ImportAndApplyCommand = new RelayCommand(ImportAndApply, () => SelectedItems.Count > 0);

            ExternalFilePath = initialExternalFilePath;
        }

        public ObservableCollection<XmlFileListItem> TargetFiles { get; }

        public XmlFileListItem? SelectedTarget
        {
            get => _selectedTarget;
            set
            {
                if (!SetProperty(ref _selectedTarget, value))
                    return;

                RebuildDiffs();
            }
        }

        public string? ExternalFilePath
        {
            get => _externalFilePath;
            set
            {
                if (!SetProperty(ref _externalFilePath, value))
                    return;

                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                RebuildDiffs();
            }
        }

        public ObservableCollection<SelectableEditHistoryItemViewModel> Rows => _rows;

        public RelayCommand BrowseExternalCommand { get; }
        public RelayCommand ClearExternalCommand { get; }
        public RelayCommand SelectAllCommand { get; }
        public RelayCommand SelectNoneCommand { get; }
        public RelayCommand ImportAsPendingCommand { get; }
        public RelayCommand ImportAsCommittedCommand { get; }
        public RelayCommand ImportAndApplyCommand { get; }

        public List<EditHistoryItem> SelectedItems => Rows.Where(r => r.IsSelected).Select(r => r.Item).ToList();

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public void SetExternalXmlPath(string path)
        {
            ExternalFilePath = path;
        }

        private void BrowseExternal()
        {
            var dlg = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "XML Files (*.xml)|*.xml|All Files (*.*)|*.*",
                Title = "Pick an XML to compare"
            };

            var ok = dlg.ShowDialog();
            if (ok != true)
                return;

            ExternalFilePath = dlg.FileName;
        }

        private void ClearExternal()
        {
            ExternalFilePath = null;
            Status = "";
        }

        private void RebuildDiffs()
        {
            Rows.Clear();
            Status = "";

            var targetPath = SelectedTarget?.FullPath;
            if (string.IsNullOrWhiteSpace(targetPath) || !File.Exists(targetPath))
            {
                Status = "Pick a target XML from the opened folder.";
                return;
            }

            if (string.IsNullOrWhiteSpace(ExternalFilePath))
            {
                Status = "Browse for an external XML or drag and drop one into this window.";
                return;
            }

            if (!File.Exists(ExternalFilePath))
            {
                Status = "External XML path is not valid.";
                return;
            }

            string targetText;
            if (!string.IsNullOrWhiteSpace(_currentOpenPathSnapshot) &&
                string.Equals(_currentOpenPathSnapshot, targetPath, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(_currentOpenXmlTextSnapshot))
            {
                targetText = _currentOpenXmlTextSnapshot;
            }
            else
            {
                targetText = File.ReadAllText(targetPath);
            }

            var edits = _comparer.BuildEdits(targetText, targetPath, ExternalFilePath, out var compareError);
            if (!string.IsNullOrWhiteSpace(compareError))
                Status = compareError;

            foreach (var item in edits)
            {
                var row = new SelectableEditHistoryItemViewModel(item, true);
                row.PropertyChanged += (_, _) => System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                Rows.Add(row);
            }

            System.Windows.Input.CommandManager.InvalidateRequerySuggested();

            if (Rows.Count == 0 && string.IsNullOrWhiteSpace(Status))
                Status = "No differences were found that can be imported as Saved Edits.";
        }

        private void SelectAll()
        {
            foreach (var r in Rows)
                r.IsSelected = true;

            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        private void SelectNone()
        {
            foreach (var r in Rows)
                r.IsSelected = false;

            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        private void ImportAsPending()
        {
            var selected = SelectedItems;
            _importer.ImportAsPending(selected);
            Status = $"Imported {selected.Count} edit(s) as Pending.";
        }

        private void ImportAsCommitted()
        {
            var selected = SelectedItems;
            _importer.ImportAsCommitted(selected);
            Status = $"Imported {selected.Count} edit(s) as Committed.";
        }

        private void ImportAndApply()
        {
            var targetPath = SelectedTarget?.FullPath;
            if (string.IsNullOrWhiteSpace(targetPath) || !File.Exists(targetPath))
            {
                Status = "Pick a valid target XML from the opened folder.";
                return;
            }

            var selected = SelectedItems;
            if (selected.Count == 0)
                return;

            string targetText;
            if (!string.IsNullOrWhiteSpace(_currentOpenPathSnapshot) &&
                string.Equals(_currentOpenPathSnapshot, targetPath, StringComparison.OrdinalIgnoreCase) &&
                !string.IsNullOrWhiteSpace(_currentOpenXmlTextSnapshot))
            {
                targetText = _currentOpenXmlTextSnapshot;
            }
            else
            {
                targetText = File.ReadAllText(targetPath);
            }

            if (!_applier.TryApplyAndSave(targetPath, targetText, selected, out var err))
            {
                Status = err ?? "Apply failed.";
                return;
            }

            _importer.ImportAsCommitted(selected);
            Status = $"Applied {selected.Count} change(s) and recorded them as Committed.";
        }
    }
}
