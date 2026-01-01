using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using LSR.XmlHelper.Wpf.Services.EditHistory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class SavedEditsWindowViewModel : ObservableObject
    {
        private readonly EditHistoryService _history;
        private readonly Func<string?> _getCurrentFilePath;
        private readonly Func<IEnumerable<EditHistoryItem>, bool> _tryApplyToCurrent;
        private readonly XmlBackupRequestService _backupRequest;

        private string? _selectedFilePath;
        private string _filterText = "";
        private DateTime? _dateFrom;
        private DateTime? _dateTo;

        private bool _showActive = true;
        private bool _showOutdated = true;
        private bool _showMissing = true;

        public SavedEditsWindowViewModel(EditHistoryService history, Func<string?> getCurrentFilePath, Func<IEnumerable<EditHistoryItem>, bool> tryApplyToCurrent, XmlBackupRequestService backupRequest)
        {
            _history = history;
            _getCurrentFilePath = getCurrentFilePath;
            _tryApplyToCurrent = tryApplyToCurrent;
            _backupRequest = backupRequest;

            ApplySelectedPendingCommand = new RelayCommand(ApplySelectedPending, CanApplySelectedPending);
            ApplyAllPendingCommand = new RelayCommand(ApplyAllPending, CanApplyAllPending);

            ApplySelectedCommittedCommand = new RelayCommand(ApplySelectedCommitted, CanApplySelectedCommitted);
            ApplyAllCommittedCommand = new RelayCommand(ApplyAllCommitted, CanApplyAllCommitted);

            DeleteSelectedPendingCommand = new RelayCommand(DeleteSelectedPending, CanDeleteSelectedPending);
            DeleteAllPendingCommand = new RelayCommand(DeleteAllPending, CanDeleteAllPending);

            DeleteSelectedCommittedCommand = new RelayCommand(DeleteSelectedCommitted, CanDeleteSelectedCommitted);
            DeleteAllCommittedCommand = new RelayCommand(DeleteAllCommitted, CanDeleteAllCommitted);

            PendingRowsView = CollectionViewSource.GetDefaultView(PendingRows);
            CommittedRowsView = CollectionViewSource.GetDefaultView(CommittedRows);

            PendingRowsView.Filter = PendingRowFilter;
            CommittedRowsView.Filter = CommittedRowFilter;

            Refresh();
        }

        public ObservableCollection<string> FilePaths { get; } = new ObservableCollection<string>();

        public string? SelectedFilePath
        {
            get => _selectedFilePath;
            set
            {
                if (SetProperty(ref _selectedFilePath, value))
                    BuildRows();
            }
        }

        public string FilterText
        {
            get => _filterText;
            set
            {
                if (SetProperty(ref _filterText, value ?? ""))
                    RefreshFilters();
            }
        }

        public DateTime? DateFrom
        {
            get => _dateFrom;
            set
            {
                if (SetProperty(ref _dateFrom, value))
                    RefreshFilters();
            }
        }

        public DateTime? DateTo
        {
            get => _dateTo;
            set
            {
                if (SetProperty(ref _dateTo, value))
                    RefreshFilters();
            }
        }

        public bool ShowActive
        {
            get => _showActive;
            set
            {
                if (SetProperty(ref _showActive, value))
                    RefreshFilters();
            }
        }

        public bool ShowOutdated
        {
            get => _showOutdated;
            set
            {
                if (SetProperty(ref _showOutdated, value))
                    RefreshFilters();
            }
        }

        public bool ShowMissing
        {
            get => _showMissing;
            set
            {
                if (SetProperty(ref _showMissing, value))
                    RefreshFilters();
            }
        }

        public ObservableCollection<SavedEditRowViewModel> PendingRows { get; } = new ObservableCollection<SavedEditRowViewModel>();
        public ObservableCollection<SavedEditRowViewModel> CommittedRows { get; } = new ObservableCollection<SavedEditRowViewModel>();

        public ICollectionView PendingRowsView { get; }
        public ICollectionView CommittedRowsView { get; }

        public RelayCommand ApplySelectedPendingCommand { get; }
        public RelayCommand ApplyAllPendingCommand { get; }

        public RelayCommand ApplySelectedCommittedCommand { get; }
        public RelayCommand ApplyAllCommittedCommand { get; }

        public RelayCommand DeleteSelectedPendingCommand { get; }
        public RelayCommand DeleteAllPendingCommand { get; }

        public RelayCommand DeleteSelectedCommittedCommand { get; }
        public RelayCommand DeleteAllCommittedCommand { get; }

        public void Refresh()
        {
            var files = _history.Pending
                .Concat(_history.Committed)
                .Select(e => e.FilePath)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            FilePaths.Clear();
            FilePaths.Add("(All files)");
            foreach (var f in files)
                FilePaths.Add(f!);

            var current = _getCurrentFilePath();
            if (!string.IsNullOrWhiteSpace(current) && files.Any(f => string.Equals(f, current, StringComparison.OrdinalIgnoreCase)))
                SelectedFilePath = current;
            else
                SelectedFilePath = "(All files)";

            BuildRows();
        }

        private void BuildRows()
        {
            PendingRows.Clear();
            CommittedRows.Clear();

            foreach (var e in _history.Pending.OrderByDescending(e => e.TimestampUtc))
                PendingRows.Add(new SavedEditRowViewModel(e));

            var committed = _history.Committed.OrderByDescending(e => e.TimestampUtc).ToList();

            var fileMap = BuildCommittedStatusMap(committed);

            foreach (var e in committed)
            {
                var row = new SavedEditRowViewModel(e);

                if (e.FilePath is not null && fileMap.TryGetValue(e.Id, out var status))
                {
                    row.Status = status.Status;
                    row.CurrentValue = status.CurrentValue;
                }

                CommittedRows.Add(row);
            }

            RefreshFilters();
            RaiseCanExecuteChanged();
        }

        private Dictionary<Guid, (string Status, string? CurrentValue)> BuildCommittedStatusMap(IReadOnlyList<EditHistoryItem> committed)
        {
            var map = new Dictionary<Guid, (string Status, string? CurrentValue)>();
            var byFile = committed
                .Where(e => !string.IsNullOrWhiteSpace(e.FilePath))
                .GroupBy(e => e.FilePath!, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var g in byFile)
            {
                if (!IsFileIncluded(g.Key))
                    continue;

                if (!_history.TryLoadXml(g.Key, out var xml, out _))
                {
                    foreach (var e in g)
                        map[e.Id] = ("Missing", null);

                    continue;
                }

                foreach (var e in g)
                {
                    if (e.Operation == EditHistoryOperation.DuplicateEntry)
                    {
                        if (_history.TryEntryExists(xml, e.CollectionTitle, e.EntryKey, e.EntryOccurrence))
                        {
                            map[e.Id] = ("Active", null);
                            continue;
                        }

                        var srcKey = e.SourceEntryKey ?? e.EntryKey;
                        var srcOcc = e.SourceEntryOccurrence ?? Math.Max(0, e.EntryOccurrence - 1);

                        if (_history.TryEntryExists(xml, e.CollectionTitle, srcKey, srcOcc))
                            map[e.Id] = ("Outdated", null);
                        else
                            map[e.Id] = ("Missing", null);

                        continue;
                    }

                    if (e.Operation == EditHistoryOperation.DeleteEntry)
                    {
                        if (_history.TryEntryExists(xml, e.CollectionTitle, e.EntryKey, e.EntryOccurrence))
                            map[e.Id] = ("Outdated", null);
                        else
                            map[e.Id] = ("Active", null);

                        continue;
                    }

                    if (!_history.TryGetCurrentFieldValue(xml, e.CollectionTitle, e.EntryKey, e.EntryOccurrence, e.FieldPath, out var current, out _))
                    {
                        map[e.Id] = ("Missing", null);
                        continue;
                    }

                    if (string.Equals(current ?? "", e.NewValue ?? "", StringComparison.Ordinal))
                        map[e.Id] = ("Active", current);
                    else
                        map[e.Id] = ("Outdated", current);
                }
            }

            return map;
        }

        private bool PendingRowFilter(object obj)
        {
            if (obj is not SavedEditRowViewModel row)
                return false;

            var e = row.Item;

            if (!IsFileIncluded(e.FilePath))
                return false;

            if (!IsDateIncluded(e.TimestampUtc.LocalDateTime))
                return false;

            if (!IsTextIncluded(e))
                return false;

            return true;
        }

        private bool CommittedRowFilter(object obj)
        {
            if (obj is not SavedEditRowViewModel row)
                return false;

            var e = row.Item;

            if (!IsFileIncluded(e.FilePath))
                return false;

            if (!IsDateIncluded(e.TimestampUtc.LocalDateTime))
                return false;

            if (!IsTextIncluded(e))
                return false;

            if (!IsCommittedStatusIncluded(row.Status))
                return false;

            return true;
        }

        private bool IsFileIncluded(string? filePath)
        {
            if (string.IsNullOrWhiteSpace(SelectedFilePath) || string.Equals(SelectedFilePath, "(All files)", StringComparison.OrdinalIgnoreCase))
                return true;

            return string.Equals(filePath ?? "", SelectedFilePath, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsDateIncluded(DateTime date)
        {
            if (DateFrom.HasValue && date.Date < DateFrom.Value.Date)
                return false;

            if (DateTo.HasValue && date.Date > DateTo.Value.Date)
                return false;

            return true;
        }

        private bool IsTextIncluded(EditHistoryItem e)
        {
            if (string.IsNullOrWhiteSpace(FilterText))
                return true;

            var needle = FilterText.Trim();
            return (e.EntryKey?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (e.FieldPath?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (e.CollectionTitle?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (e.OldValue?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false)
                   || (e.NewValue?.Contains(needle, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        private bool IsCommittedStatusIncluded(string status)
        {
            if (string.Equals(status, "Active", StringComparison.OrdinalIgnoreCase))
                return ShowActive;

            if (string.Equals(status, "Outdated", StringComparison.OrdinalIgnoreCase))
                return ShowOutdated;

            if (string.Equals(status, "Missing", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(status))
                return ShowMissing;

            return true;
        }

        private void RefreshFilters()
        {
            PendingRowsView.Refresh();
            CommittedRowsView.Refresh();
            RaiseCanExecuteChanged();
        }

        private void ApplySelectedPending()
        {
            Apply(PendingRows.Where(r => r.IsSelected).Select(r => r.Item).ToList());
        }

        private bool CanApplySelectedPending()
        {
            return PendingRows.Any(r => r.IsSelected) && IsSelectedFileCurrent();
        }

        private void ApplyAllPending()
        {
            Apply(PendingRows.Where(r => PendingRowFilter(r)).Select(r => r.Item).ToList());
        }

        private bool CanApplyAllPending()
        {
            return PendingRowsView.Cast<object>().Any() && IsSelectedFileCurrent();
        }

        private void ApplySelectedCommitted()
        {
            Apply(CommittedRows.Where(r => r.IsSelected).Select(r => r.Item).ToList());
        }

        private bool CanApplySelectedCommitted()
        {
            return CommittedRows.Any(r => r.IsSelected) && IsSelectedFileCurrent();
        }

        private void ApplyAllCommitted()
        {
            Apply(CommittedRows.Where(r => CommittedRowFilter(r)).Select(r => r.Item).ToList());
        }

        private bool CanApplyAllCommitted()
        {
            return CommittedRowsView.Cast<object>().Any() && IsSelectedFileCurrent();
        }

        private void DeleteSelectedPending()
        {
            var ids = PendingRows.Where(r => r.IsSelected).Select(r => r.Item.Id).ToList();
            if (ids.Count == 0)
                return;

            _history.DeletePending(ids);
            Refresh();
        }

        private bool CanDeleteSelectedPending()
        {
            return PendingRows.Any(r => r.IsSelected);
        }

        private void DeleteAllPending()
        {
            var ids = PendingRowsView.Cast<SavedEditRowViewModel>().Select(r => r.Item.Id).ToList();
            if (ids.Count == 0)
                return;

            _history.DeletePending(ids);
            Refresh();
        }

        private bool CanDeleteAllPending()
        {
            return PendingRowsView.Cast<object>().Any();
        }

        private void DeleteSelectedCommitted()
        {
            var ids = CommittedRows.Where(r => r.IsSelected).Select(r => r.Item.Id).ToList();
            if (ids.Count == 0)
                return;

            try
            {
                TryBackupBeforeCommittedDelete(CommittedRows.Where(r => r.IsSelected).Select(r => r.Item).ToList());
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _history.DeleteCommitted(ids);
            Refresh();
        }

        private bool CanDeleteSelectedCommitted()
        {
            return CommittedRows.Any(r => r.IsSelected);
        }

        private void DeleteAllCommitted()
        {
            var items = CommittedRowsView.Cast<SavedEditRowViewModel>().Select(r => r.Item).ToList();
            if (items.Count == 0)
                return;

            try
            {
                TryBackupBeforeCommittedDelete(items);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            _history.DeleteCommitted(items.Select(i => i.Id).ToList());
            Refresh();
        }

        private bool CanDeleteAllCommitted()
        {
            return CommittedRowsView.Cast<object>().Any();
        }

        private void TryBackupBeforeCommittedDelete(IReadOnlyList<EditHistoryItem> items)
        {
            var result = MessageBox.Show(
                "You are deleting committed edits. Backup affected XML files first?",
                "Saved Edits",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Cancel || result == MessageBoxResult.None)
                throw new OperationCanceledException();

            if (result != MessageBoxResult.Yes)
                return;

            var paths = items
                .Select(i => i.FilePath)
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var p in paths)
            {
                if (!_backupRequest.TryBackup(p!, out var err))
                {
                    MessageBox.Show(err ?? "Backup failed.", "Saved Edits", MessageBoxButton.OK, MessageBoxImage.Warning);
                    throw new OperationCanceledException();
                }
            }

            MessageBox.Show("Backup created successfully.", "Saved Edits", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void Apply(IReadOnlyList<EditHistoryItem> edits)
        {
            if (edits.Count == 0)
                return;

            if (!IsSelectedFileCurrent())
            {
                MessageBox.Show("Open the same file in the main window before applying edits.", "Saved Edits", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var ok = _tryApplyToCurrent(edits);
            if (!ok)
                MessageBox.Show("Edits could not be applied. See the main window status/error for details.", "Saved Edits", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private bool IsSelectedFileCurrent()
        {
            var current = _getCurrentFilePath();
            if (string.IsNullOrWhiteSpace(current))
                return false;

            if (string.IsNullOrWhiteSpace(SelectedFilePath) || string.Equals(SelectedFilePath, "(All files)", StringComparison.OrdinalIgnoreCase))
                return false;

            return string.Equals(current, SelectedFilePath, StringComparison.OrdinalIgnoreCase);
        }

        private void RaiseCanExecuteChanged()
        {
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
    }
}
