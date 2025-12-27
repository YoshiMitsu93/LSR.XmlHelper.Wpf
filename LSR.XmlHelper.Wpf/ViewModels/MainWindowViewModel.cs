using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using LSR.XmlHelper.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Media = System.Windows.Media;
using WinForms = System.Windows.Forms;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class MainWindowViewModel : ObservableObject
    {
        private readonly XmlDocumentService _xml;
        private readonly XmlFileDiscoveryService _discovery;
        private readonly XmlFileLoaderService _loader;
        private readonly XmlFileSaveService _saver;
        private readonly XmlFriendlyViewService _friendly;

        private readonly AppSettingsService _settingsService;
        private AppSettings _settings;

        private readonly AppearanceService _appearance;

        private CancellationTokenSource? _loadCts;

        private CancellationTokenSource? _friendlyBuildCts;
        private CancellationTokenSource? _friendlyUiBuildCts;
        private int _xmlVersion;
        private bool _suppressFriendlyRebuild;

        private bool _suppressDirtyTracking;

        private string _title = "LSR XML Helper";
        private string _status = "Ready.";
        private string _xmlText = "";
        private string? _rootFolder;

        private bool _isDirty;

        private XmlFileListItem? _selectedXmlFile;
        private XmlExplorerNode? _selectedTreeNode;

        private XmlListViewMode _viewMode = XmlListViewMode.Flat;
        private bool _includeSubfolders;

        private bool _hasFriendlyView;
        private bool _isFriendlyView;
        private XmlFriendlyDocument? _friendlyDocument;

        private bool _isDarkMode = true;

        private ObservableCollection<XmlFriendlyCollectionViewModel> _friendlyCollections = new();
        private XmlFriendlyCollectionViewModel? _selectedFriendlyCollection;
        private XmlFriendlyEntryViewModel? _selectedFriendlyEntry;

        private ObservableCollection<XmlFriendlyFieldViewModel> _friendlyFields = new();
        private ObservableCollection<XmlFriendlyFieldGroupViewModel> _friendlyFieldGroups = new();
        private ObservableCollection<object> _friendlyGroups = new();

        public MainWindowViewModel()
        {
            _xml = new XmlDocumentService();
            _discovery = new XmlFileDiscoveryService();
            _loader = new XmlFileLoaderService();
            _saver = new XmlFileSaveService();
            _friendly = new XmlFriendlyViewService();

            _settingsService = new AppSettingsService();
            _settings = _settingsService.Load();

            ApplySettingsToState();

            _appearance = new AppearanceService(_settings.Appearance, _isDarkMode, _isFriendlyView);
            _appearance.PropertyChanged += AppearanceOnPropertyChanged;
            SyncThemeTogglesFromAppearance();

            XmlFiles = new ObservableCollection<XmlFileListItem>();
            XmlTree = new ObservableCollection<XmlExplorerNode>();

            OpenCommand = new RelayCommand(OpenFolder);
            FormatCommand = new RelayCommand(Format, () => !string.IsNullOrWhiteSpace(XmlText));
            ValidateCommand = new RelayCommand(Validate, () => !string.IsNullOrWhiteSpace(XmlText));
            SaveCommand = new RelayCommand(Save, () => GetSelectedFilePath() is not null && !string.IsNullOrWhiteSpace(XmlText));
            SaveAsCommand = new RelayCommand(SaveAs, () => !string.IsNullOrWhiteSpace(XmlText));
            ClearCommand = new RelayCommand(Clear, () => GetSelectedFilePath() is not null || !string.IsNullOrWhiteSpace(XmlText));

            OpenAppearanceCommand = new RelayCommand(OpenAppearance);

            if (!string.IsNullOrWhiteSpace(_rootFolder))
            {
                RefreshFileViews(resetEditorAndSelection: true);
                Title = $"LSR XML Helper - {_rootFolder}";
            }
        }

        public AppearanceService Appearance => _appearance;

        public RelayCommand OpenAppearanceCommand { get; }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public bool IsDirty
        {
            get => _isDirty;
            private set => SetProperty(ref _isDirty, value);
        }

        public string XmlText
        {
            get => _xmlText;
            set
            {
                if (_xmlText == value)
                    return;

                _xmlText = value;
                OnPropertyChanged();

                if (!_suppressDirtyTracking)
                    IsDirty = true;

                if (_suppressFriendlyRebuild)
                    return;

                if (IsFriendlyView)
                    ScheduleFriendlyRebuild();
            }
        }

        public ObservableCollection<XmlFileListItem> XmlFiles { get; }
        public ObservableCollection<XmlExplorerNode> XmlTree { get; }

        public XmlListViewMode ViewMode
        {
            get => _viewMode;
            set
            {
                if (!SetProperty(ref _viewMode, value))
                    return;

                OnPropertyChanged(nameof(IsFlatMode));
                OnPropertyChanged(nameof(IsFoldersMode));

                _settings.ViewMode = value.ToString();
                SaveSettings();

                RefreshFileViews(resetEditorAndSelection: false);
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IncludeSubfolders
        {
            get => _includeSubfolders;
            set
            {
                if (!SetProperty(ref _includeSubfolders, value))
                    return;

                _settings.IncludeSubfolders = value;
                SaveSettings();

                RefreshFileViews(resetEditorAndSelection: false);
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public bool IsFlatMode
        {
            get => ViewMode == XmlListViewMode.Flat;
            set
            {
                if (value)
                {
                    ViewMode = XmlListViewMode.Flat;
                    OnPropertyChanged(nameof(IsFoldersMode));
                }
            }
        }

        public bool IsFoldersMode
        {
            get => ViewMode == XmlListViewMode.Folders;
            set
            {
                if (value)
                {
                    ViewMode = XmlListViewMode.Folders;
                    OnPropertyChanged(nameof(IsFlatMode));
                }
            }
        }

        public bool HasFriendlyView
        {
            get => _hasFriendlyView;
            private set => SetProperty(ref _hasFriendlyView, value);
        }

        public bool IsFriendlyView
        {
            get => _isFriendlyView;
            set
            {
                if (!SetProperty(ref _isFriendlyView, value))
                    return;

                _settings.IsFriendlyView = _isFriendlyView;
                _appearance.IsFriendlyView = _isFriendlyView;

                SaveSettings();
            }
        }

        public bool IsDarkMode
        {
            get => _isDarkMode;
            set
            {
                if (!SetProperty(ref _isDarkMode, value))
                    return;

                _settings.IsDarkMode = _isDarkMode;
                _appearance.IsDarkMode = _isDarkMode;

                SaveSettings();
            }
        }

        public ObservableCollection<XmlFriendlyCollectionViewModel> FriendlyCollections
        {
            get => _friendlyCollections;
            private set => SetProperty(ref _friendlyCollections, value);
        }

        public XmlFriendlyCollectionViewModel? SelectedFriendlyCollection
        {
            get => _selectedFriendlyCollection;
            set
            {
                if (!SetProperty(ref _selectedFriendlyCollection, value))
                    return;

                SelectedFriendlyEntry = value?.Entries.FirstOrDefault();
            }
        }

        public XmlFriendlyEntryViewModel? SelectedFriendlyEntry
        {
            get => _selectedFriendlyEntry;
            set
            {
                if (!SetProperty(ref _selectedFriendlyEntry, value))
                    return;

                RebuildFieldsForSelectedEntry();
            }
        }

        public ObservableCollection<XmlFriendlyFieldViewModel> FriendlyFields
        {
            get => _friendlyFields;
            private set => SetProperty(ref _friendlyFields, value);
        }

        public ObservableCollection<XmlFriendlyFieldGroupViewModel> FriendlyFieldGroups
        {
            get => _friendlyFieldGroups;
            private set => SetProperty(ref _friendlyFieldGroups, value);
        }

        public ObservableCollection<object> FriendlyGroups
        {
            get => _friendlyGroups;
            private set => SetProperty(ref _friendlyGroups, value);
        }

        public XmlFileListItem? SelectedXmlFile
        {
            get => _selectedXmlFile;
            set
            {
                if (ReferenceEquals(value, _selectedXmlFile))
                    return;

                if (value is not null && !TryConfirmDiscardOrSaveIfDirty())
                {
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    return;
                }

                if (!SetProperty(ref _selectedXmlFile, value))
                    return;

                if (value is not null)
                {
                    SelectedTreeNode = null;
                    _ = LoadFileAsync(value.FullPath);
                }

                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public XmlExplorerNode? SelectedTreeNode
        {
            get => _selectedTreeNode;
            set
            {
                if (ReferenceEquals(value, _selectedTreeNode))
                    return;

                if (value is not null && value.IsFile && value.FullPath is not null && !TryConfirmDiscardOrSaveIfDirty())
                {
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                    return;
                }

                if (!SetProperty(ref _selectedTreeNode, value))
                    return;

                if (value is not null && value.IsFile && value.FullPath is not null)
                {
                    SelectedXmlFile = null;
                    _ = LoadFileAsync(value.FullPath);
                }

                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }

        public RelayCommand OpenCommand { get; }
        public RelayCommand FormatCommand { get; }
        public RelayCommand ValidateCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand ClearCommand { get; }

        private void OpenAppearance()
        {
            var vm = new AppearanceWindowViewModel(
                _settingsService,
                _settings,
                _appearance,
                _appearance.IsDarkMode,
                _appearance.IsFriendlyView);

            var win = new AppearanceWindow
            {
                Owner = System.Windows.Application.Current?.MainWindow,
                DataContext = vm
            };

            win.ShowDialog();
        }

        private void AppearanceOnPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppearanceService.IsDarkMode) ||
                e.PropertyName == nameof(AppearanceService.IsFriendlyView))
            {
                SyncThemeTogglesFromAppearance();
            }
        }

        private void SyncThemeTogglesFromAppearance()
        {
            if (_isDarkMode != _appearance.IsDarkMode)
            {
                _isDarkMode = _appearance.IsDarkMode;
                OnPropertyChanged(nameof(IsDarkMode));
            }

            if (_isFriendlyView != _appearance.IsFriendlyView)
            {
                _isFriendlyView = _appearance.IsFriendlyView;
                OnPropertyChanged(nameof(IsFriendlyView));
            }
        }

        public bool TryConfirmClose()
        {
            return TryConfirmDiscardOrSaveIfDirty();
        }

        private bool TryConfirmDiscardOrSaveIfDirty()
        {
            if (!IsDirty || string.IsNullOrWhiteSpace(XmlText))
                return true;

            var result = MessageBox.Show(
                "You have unsaved changes.\n\nSave before continuing?",
                "Unsaved changes",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
                return false;

            if (result == MessageBoxResult.No)
                return true;

            var ok = TrySaveCurrent();
            return ok;
        }

        private bool TrySaveCurrent()
        {
            var path = GetSelectedFilePath();
            if (path is null)
                return TrySaveAsCurrent();

            var (ok, err) = _saver.Save(path, XmlText);
            if (!ok)
            {
                MessageBox.Show(err ?? "Save failed.", "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Save failed.";
                return false;
            }

            IsDirty = false;
            Status = $"Saved: {Path.GetFileName(path)}";
            return true;
        }

        private bool TrySaveAsCurrent()
        {
            var current = GetSelectedFilePath();

            var dlg = new WpfSaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Save XML As",
                FileName = current is null ? "document.xml" : Path.GetFileName(current),
                InitialDirectory = string.IsNullOrWhiteSpace(_rootFolder)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : _rootFolder
            };

            if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.FileName))
                return false;

            var (ok, err) = _saver.Save(dlg.FileName, XmlText);
            if (!ok)
            {
                MessageBox.Show(err ?? "Save failed.", "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Save failed.";
                return false;
            }

            IsDirty = false;
            Status = $"Saved: {Path.GetFileName(dlg.FileName)}";
            return true;
        }

        private void ScheduleFriendlyRebuild()
        {
            var myVersion = Interlocked.Increment(ref _xmlVersion);

            try
            {
                _friendlyBuildCts?.Cancel();
                _friendlyBuildCts?.Dispose();
            }
            catch
            {
            }

            _friendlyBuildCts = new CancellationTokenSource();
            var token = _friendlyBuildCts.Token;

            _ = Task.Run(() =>
            {
                try
                {
                    token.ThrowIfCancellationRequested();
                    var doc = _friendly.TryBuild(XmlText);
                    token.ThrowIfCancellationRequested();
                    return (Version: myVersion, Doc: doc);
                }
                catch
                {
                    return (Version: myVersion, Doc: (XmlFriendlyDocument?)null);
                }
            }, token).ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted)
                    return;

                if (t.Result.Version != _xmlVersion)
                    return;

                _friendlyDocument = t.Result.Doc;
                RefreshFriendlyFromXml();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void RefreshFriendlyFromXml()
        {
            CancelPendingFriendlyUiBuild();

            if (_friendlyDocument is null)
            {
                HasFriendlyView = false;

                FriendlyCollections = new ObservableCollection<XmlFriendlyCollectionViewModel>();
                _selectedFriendlyCollection = null;
                _selectedFriendlyEntry = null;
                OnPropertyChanged(nameof(SelectedFriendlyCollection));
                OnPropertyChanged(nameof(SelectedFriendlyEntry));

                ClearFriendlyFields();
                return;
            }

            _friendlyUiBuildCts = new CancellationTokenSource();
            var token = _friendlyUiBuildCts.Token;

            var myVersion = _xmlVersion;
            var doc = _friendlyDocument;

            _ = Task.Run(() => BuildFriendlyUiState(doc, token), token).ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted)
                    return;

                if (myVersion != _xmlVersion)
                    return;

                ApplyFriendlyUiState(t.Result);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void CancelPendingFriendlyUiBuild()
        {
            try
            {
                _friendlyUiBuildCts?.Cancel();
                _friendlyUiBuildCts?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _friendlyUiBuildCts = null;
            }
        }

        private sealed record FriendlyUiState(
            ObservableCollection<XmlFriendlyCollectionViewModel> Collections,
            XmlFriendlyCollectionViewModel? SelectedCollection,
            XmlFriendlyEntryViewModel? SelectedEntry,
            ObservableCollection<XmlFriendlyFieldViewModel> Fields,
            ObservableCollection<XmlFriendlyFieldGroupViewModel> FieldGroups,
            ObservableCollection<object> Groups);

        private void ApplyFriendlyUiState(FriendlyUiState state)
        {
            HasFriendlyView = state.Collections.Count > 0;

            FriendlyCollections = state.Collections;

            _selectedFriendlyCollection = state.SelectedCollection;
            _selectedFriendlyEntry = state.SelectedEntry;
            OnPropertyChanged(nameof(SelectedFriendlyCollection));
            OnPropertyChanged(nameof(SelectedFriendlyEntry));

            DetachFriendlyFieldHandlers(_friendlyFields);

            foreach (var f in state.Fields)
                f.PropertyChanged += FriendlyField_PropertyChanged;

            FriendlyFields = state.Fields;
            FriendlyFieldGroups = state.FieldGroups;
            FriendlyGroups = state.Groups;
        }

        private static FriendlyUiState BuildFriendlyUiState(XmlFriendlyDocument doc, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var cols = doc.Collections.Select(c => new XmlFriendlyCollectionViewModel(c)).ToList();
            var collections = new ObservableCollection<XmlFriendlyCollectionViewModel>(cols);

            var selectedCollection = collections.FirstOrDefault();
            var selectedEntry = selectedCollection?.Entries.FirstOrDefault();

            if (selectedEntry?.Entry is null)
            {
                return new FriendlyUiState(
                    collections,
                    selectedCollection,
                    selectedEntry,
                    new ObservableCollection<XmlFriendlyFieldViewModel>(),
                    new ObservableCollection<XmlFriendlyFieldGroupViewModel>(),
                    new ObservableCollection<object>());
            }

            token.ThrowIfCancellationRequested();

            var entry = selectedEntry.Entry;

            var fields = entry.Fields
                .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                .Select(k => new XmlFriendlyFieldViewModel(k.Key, k.Value.Value ?? ""))
                .ToList();

            token.ThrowIfCancellationRequested();

            var fieldsVm = new ObservableCollection<XmlFriendlyFieldViewModel>(fields);
            var fieldGroupsVm = BuildGroupsFromFields(fieldsVm);
            var groupsVm = BuildUnifiedGroups(fieldsVm);

            return new FriendlyUiState(
                collections,
                selectedCollection,
                selectedEntry,
                fieldsVm,
                fieldGroupsVm,
                groupsVm);
        }

        private void ClearFriendlyFields()
        {
            DetachFriendlyFieldHandlers(_friendlyFields);

            FriendlyFields = new ObservableCollection<XmlFriendlyFieldViewModel>();
            FriendlyFieldGroups = new ObservableCollection<XmlFriendlyFieldGroupViewModel>();
            FriendlyGroups = new ObservableCollection<object>();
        }

        private static void DetachFriendlyFieldHandlers(ObservableCollection<XmlFriendlyFieldViewModel> fields)
        {
            foreach (var f in fields)
                f.PropertyChanged -= null;
        }

        private void RebuildFieldsForSelectedEntry()
        {
            var entry = SelectedFriendlyEntry?.Entry;
            if (entry is null)
            {
                ClearFriendlyFields();
                return;
            }

            var fields = entry.Fields
                .OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase)
                .Select(k => new XmlFriendlyFieldViewModel(k.Key, k.Value.Value ?? ""))
                .ToList();

            var newFields = new ObservableCollection<XmlFriendlyFieldViewModel>(fields);

            foreach (var f in newFields)
                f.PropertyChanged += FriendlyField_PropertyChanged;

            FriendlyFields = newFields;
            FriendlyFieldGroups = BuildGroupsFromFields(newFields);
            FriendlyGroups = BuildUnifiedGroups(newFields);
        }

        private static ObservableCollection<object> BuildUnifiedGroups(ObservableCollection<XmlFriendlyFieldViewModel> fields)
        {
            static bool TryParseLookupField(string name, out string groupTitle, out string itemName, out string leafField)
            {
                groupTitle = "";
                itemName = "";
                leafField = "";

                if (string.IsNullOrWhiteSpace(name))
                    return false;

                var parts = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                    return false;

                var first = parts[0];
                var second = parts[1];
                var third = parts[2];

                if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second) || string.IsNullOrWhiteSpace(third))
                    return false;

                var lb = second.IndexOf('[');
                var rb = second.EndsWith("]", StringComparison.Ordinal);

                if (lb <= 0 || !rb)
                    return false;

                groupTitle = first;
                itemName = second;
                leafField = third;
                return true;
            }

            static string GetGroupTitle(string fieldName)
            {
                if (string.IsNullOrWhiteSpace(fieldName))
                    return "General";

                var slash = fieldName.IndexOf('/');
                if (slash <= 0)
                    return "General";

                return fieldName.Substring(0, slash);
            }

            var lookupBuckets = new Dictionary<string, Dictionary<string, List<XmlFriendlyLookupItemViewModel>>>(StringComparer.OrdinalIgnoreCase);
            var normalBuckets = new Dictionary<string, List<XmlFriendlyFieldViewModel>>(StringComparer.OrdinalIgnoreCase);

            foreach (var f in fields)
            {
                if (TryParseLookupField(f.Name, out var lookupGroup, out var itemName, out var leaf))
                {
                    if (!lookupBuckets.TryGetValue(lookupGroup, out var itemsByName))
                    {
                        itemsByName = new Dictionary<string, List<XmlFriendlyLookupItemViewModel>>(StringComparer.OrdinalIgnoreCase);
                        lookupBuckets[lookupGroup] = itemsByName;
                    }

                    if (!itemsByName.TryGetValue(itemName, out var list))
                    {
                        list = new List<XmlFriendlyLookupItemViewModel>();
                        itemsByName[itemName] = list;
                    }

                    list.Add(new XmlFriendlyLookupItemViewModel(itemName, leaf, f));
                    continue;
                }

                var normalGroup = GetGroupTitle(f.Name);
                if (!normalBuckets.TryGetValue(normalGroup, out var normalList))
                {
                    normalList = new List<XmlFriendlyFieldViewModel>();
                    normalBuckets[normalGroup] = normalList;
                }

                normalList.Add(f);
            }

            var output = new List<object>();

            if (normalBuckets.TryGetValue("General", out var general))
            {
                var generalVm = new XmlFriendlyFieldGroupViewModel(
                    "General",
                    new ObservableCollection<XmlFriendlyFieldViewModel>(general));
                output.Add(generalVm);
                normalBuckets.Remove("General");
            }

            foreach (var lookupGroup in lookupBuckets.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                var rows = new List<XmlFriendlyLookupItemViewModel>();

                foreach (var item in lookupGroup.Value.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
                    rows.AddRange(item.Value.OrderBy(x => x.Field, StringComparer.OrdinalIgnoreCase));

                output.Add(new XmlFriendlyLookupGroupViewModel(
                    lookupGroup.Key,
                    new ObservableCollection<XmlFriendlyLookupItemViewModel>(rows)));
            }

            foreach (var g in normalBuckets.OrderBy(k => k.Key, StringComparer.OrdinalIgnoreCase))
            {
                output.Add(new XmlFriendlyFieldGroupViewModel(
                    g.Key,
                    new ObservableCollection<XmlFriendlyFieldViewModel>(g.Value)));
            }

            return new ObservableCollection<object>(output);
        }

        private static ObservableCollection<XmlFriendlyFieldGroupViewModel> BuildGroupsFromFields(ObservableCollection<XmlFriendlyFieldViewModel> fields)
        {
            static string GetGroupTitle(string fieldName)
            {
                if (string.IsNullOrWhiteSpace(fieldName))
                    return "General";

                var slash = fieldName.IndexOf('/');
                if (slash <= 0)
                    return "General";

                return fieldName.Substring(0, slash);
            }

            var grouped = fields
                .GroupBy(f => GetGroupTitle(f.Name), StringComparer.OrdinalIgnoreCase)
                .Select(g => new
                {
                    Title = g.Key,
                    Fields = new ObservableCollection<XmlFriendlyFieldViewModel>(g.ToList())
                })
                .ToList();

            var ordered = grouped
                .OrderBy(g => !string.Equals(g.Title, "General", StringComparison.OrdinalIgnoreCase))
                .ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase)
                .Select(g => new XmlFriendlyFieldGroupViewModel(g.Title, g.Fields))
                .ToList();

            return new ObservableCollection<XmlFriendlyFieldGroupViewModel>(ordered);
        }

        private void FriendlyField_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(XmlFriendlyFieldViewModel.Value))
                return;

            if (sender is not XmlFriendlyFieldViewModel field)
                return;

            var entry = SelectedFriendlyEntry?.Entry;
            if (entry is null)
                return;

            if (!entry.TrySetField(field.Name, field.Value, out _))
                return;

            if (_friendlyDocument is null)
                return;

            var updatedXml = _friendly.ToXml(_friendlyDocument);

            _suppressFriendlyRebuild = true;
            try
            {
                _suppressDirtyTracking = true;
                try
                {
                    _xmlText = updatedXml;
                    OnPropertyChanged(nameof(XmlText));
                }
                finally
                {
                    _suppressDirtyTracking = false;
                }
            }
            finally
            {
                _suppressFriendlyRebuild = false;
            }

            IsDirty = true;
            Status = "Edited.";
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        private string? GetSelectedFilePath()
        {
            if (SelectedXmlFile is not null)
                return SelectedXmlFile.FullPath;

            if (SelectedTreeNode is not null && SelectedTreeNode.IsFile && SelectedTreeNode.FullPath is not null)
                return SelectedTreeNode.FullPath;

            return null;
        }

        private void Format()
        {
            try
            {
                XmlText = _xml.Format(XmlText);
                Status = "Formatted.";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Format failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Format failed.";
            }
        }

        private void Validate()
        {
            var (ok, msg) = _xml.ValidateWellFormed(XmlText);

            MessageBox.Show(msg, ok ? "Validate" : "Validate failed",
                MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Error);

            Status = ok ? "Valid." : "Invalid.";
        }

        private void Save()
        {
            if (!IsDirty)
            {
                Status = "No changes to save.";
                return;
            }

            var path = GetSelectedFilePath();
            if (path is null)
            {
                SaveAs();
                return;
            }

            var existed = File.Exists(path);
            var backupDir = existed
                ? new XmlHelperRootService().GetOrCreateSubfolder(path, "BackupXMLs")
                : null;

            var (ok, err) = _saver.Save(path, XmlText);
            if (!ok)
            {
                MessageBox.Show(err ?? "Save failed.", "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Save failed.";
                return;
            }

            IsDirty = false;

            if (existed && backupDir is not null)
            {
                Status = $"Saved: {Path.GetFileName(path)} | Backup: {backupDir}";
                MessageBox.Show($"Backup created:\n{backupDir}", "Backup created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Status = $"Saved: {Path.GetFileName(path)}";
            }
        }

        private void SaveAs()
        {
            if (!IsDirty)
            {
                Status = "No changes to save.";
                return;
            }

            var current = GetSelectedFilePath();

            var dlg = new WpfSaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Save XML As",
                FileName = current is null ? "document.xml" : Path.GetFileName(current),
                InitialDirectory = string.IsNullOrWhiteSpace(_rootFolder)
                    ? Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                    : _rootFolder
            };

            if (dlg.ShowDialog() != true || string.IsNullOrWhiteSpace(dlg.FileName))
                return;

            var existed = File.Exists(dlg.FileName);
            var backupDir = existed
                ? new XmlHelperRootService().GetOrCreateSubfolder(dlg.FileName, "BackupXMLs")
                : null;

            var (ok, err) = _saver.Save(dlg.FileName, XmlText);
            if (!ok)
            {
                MessageBox.Show(err ?? "Save failed.", "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Save failed.";
                return;
            }

            IsDirty = false;

            if (existed && backupDir is not null)
            {
                Status = $"Saved: {Path.GetFileName(dlg.FileName)} | Backup: {backupDir}";
                MessageBox.Show($"Backup created:\n{backupDir}", "Backup created", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Status = $"Saved: {Path.GetFileName(dlg.FileName)}";
            }
        }

        private void Clear()
        {
            CancelPendingLoad();
            SelectedXmlFile = null;
            SelectedTreeNode = null;
            XmlText = "";
            Status = "Ready.";
        }

        private void OpenFolder()
        {
            if (!TryConfirmDiscardOrSaveIfDirty())
                return;

            string? picked = null;

            using (var dlg = new WinForms.FolderBrowserDialog())
            {
                dlg.Description = "Select folder containing XML files";
                dlg.ShowNewFolderButton = false;

                if (!string.IsNullOrWhiteSpace(_rootFolder) && Directory.Exists(_rootFolder))
                    dlg.SelectedPath = _rootFolder;

                var result = dlg.ShowDialog();
                if (result == WinForms.DialogResult.OK)
                    picked = dlg.SelectedPath;
            }

            if (string.IsNullOrWhiteSpace(picked) || !Directory.Exists(picked))
                return;

            _rootFolder = picked;
            _settings.LastFolder = picked;
            SaveSettings();

            RefreshFileViews(resetEditorAndSelection: true);
            Title = $"LSR XML Helper - {_rootFolder}";
        }

        private void RefreshFileViews(bool resetEditorAndSelection)
        {
            XmlFiles.Clear();
            XmlTree.Clear();

            var previouslySelectedPath = GetSelectedFilePath();

            if (resetEditorAndSelection)
            {
                SelectedXmlFile = null;
                SelectedTreeNode = null;

                _suppressDirtyTracking = true;
                try { XmlText = ""; }
                finally { _suppressDirtyTracking = false; }

                IsDirty = false;
            }

            if (string.IsNullOrWhiteSpace(_rootFolder) || !Directory.Exists(_rootFolder))
            {
                Status = "Pick a folder.";
                return;
            }

            IReadOnlyList<string> paths;
            try
            {
                paths = _discovery.GetXmlFiles(_rootFolder, IncludeSubfolders);
            }
            catch (Exception ex)
            {
                Status = "Failed to scan folder.";
                MessageBox.Show(ex.Message, "Folder scan failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            foreach (var p in paths)
            {
                var display = string.IsNullOrWhiteSpace(_rootFolder)
                    ? p
                    : Path.GetRelativePath(_rootFolder, p);

                XmlFiles.Add(new XmlFileListItem(p, display));
            }

            if (ViewMode == XmlListViewMode.Folders)
                BuildTree(paths);

            Status = paths.Count == 0 ? "No XML files found." : $"Found {paths.Count} XML file(s).";

            if (resetEditorAndSelection)
            {
                if (ViewMode == XmlListViewMode.Flat && XmlFiles.Count > 0)
                    SelectedXmlFile = XmlFiles[0];

                return;
            }

            if (previouslySelectedPath is null)
                return;

            if (ViewMode == XmlListViewMode.Flat)
            {
                var match = XmlFiles.FirstOrDefault(x =>
                    string.Equals(x.FullPath, previouslySelectedPath, StringComparison.OrdinalIgnoreCase));

                if (match is not null)
                    SelectedXmlFile = match;
            }
        }

        private void BuildTree(IReadOnlyList<string> paths)
        {
            if (string.IsNullOrWhiteSpace(_rootFolder))
                return;

            var folderMap = new Dictionary<string, XmlFolderNode>(StringComparer.OrdinalIgnoreCase);

            foreach (var fullPath in paths)
            {
                var relative = Path.GetRelativePath(_rootFolder, fullPath);
                var parts = relative.Split(new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar }, StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 0)
                    continue;

                if (parts.Length == 1)
                {
                    XmlTree.Add(new XmlFileNode(parts[0], fullPath));
                    continue;
                }

                XmlFolderNode? currentFolder = null;
                var currentKey = "";

                for (var i = 0; i < parts.Length - 1; i++)
                {
                    currentKey = string.IsNullOrEmpty(currentKey) ? parts[i] : $"{currentKey}\\{parts[i]}";

                    if (!folderMap.TryGetValue(currentKey, out var folderNode))
                    {
                        folderNode = new XmlFolderNode(parts[i]);
                        folderMap[currentKey] = folderNode;

                        if (currentFolder is null)
                            XmlTree.Add(folderNode);
                        else
                            currentFolder.Children.Add(folderNode);
                    }

                    currentFolder = folderNode;
                }

                currentFolder?.Children.Add(new XmlFileNode(parts[^1], fullPath));
            }

            SortTree(XmlTree);
        }

        private static void SortTree(ObservableCollection<XmlExplorerNode> nodes)
        {
            var ordered = nodes
                .OrderBy(n => n.IsFile)
                .ThenBy(n => n.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();

            nodes.Clear();
            foreach (var n in ordered)
                nodes.Add(n);

            foreach (var folder in nodes.OfType<XmlFolderNode>())
                SortTree(folder.Children);
        }

        private void CancelPendingLoad()
        {
            try
            {
                _loadCts?.Cancel();
                _loadCts?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _loadCts = null;
            }
        }

        private async Task LoadFileAsync(string path)
        {
            CancelPendingLoad();
            _loadCts = new CancellationTokenSource();

            var token = _loadCts.Token;
            var name = Path.GetFileName(path);

            Status = $"Loading: {name}";

            var result = await _loader.LoadAsync(path, token);

            if (!result.Success)
            {
                if (result.Error is not null)
                {
                    MessageBox.Show(result.Error, "Open failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Status = "Open failed.";
                }

                return;
            }

            try
            {
                _suppressDirtyTracking = true;
                _suppressFriendlyRebuild = true;

                XmlText = result.Text ?? "";
                IsDirty = false;
            }
            finally
            {
                _suppressDirtyTracking = false;
                _suppressFriendlyRebuild = false;
            }

            await RebuildFriendlyFromCurrentXmlAsync(token);
            Status = $"Opened: {name}";
        }

        private async Task RebuildFriendlyFromCurrentXmlAsync(CancellationToken token)
        {
            var xml = XmlText ?? "";
            XmlFriendlyDocument? doc = null;

            try
            {
                doc = await Task.Run(() =>
                {
                    token.ThrowIfCancellationRequested();
                    return _friendly.TryBuild(xml);
                }, token);
            }
            catch
            {
                doc = null;
            }

            _friendlyDocument = doc;
            RefreshFriendlyFromXml();
        }

        private void ApplySettingsToState()
        {
            _rootFolder = string.IsNullOrWhiteSpace(_settings.LastFolder) ? null : _settings.LastFolder;

            if (Enum.TryParse<XmlListViewMode>(_settings.ViewMode ?? "", ignoreCase: true, out var parsed))
                _viewMode = parsed;

            _includeSubfolders = _settings.IncludeSubfolders;
            _isDarkMode = _settings.IsDarkMode;
            _isFriendlyView = _settings.IsFriendlyView;
        }

        private void SaveSettings()
        {
            try { _settingsService.Save(_settings); }
            catch { }
        }
    }
}