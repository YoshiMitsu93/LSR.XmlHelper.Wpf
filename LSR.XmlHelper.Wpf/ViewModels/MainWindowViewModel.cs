using LSR.XmlHelper.Core.Models;
using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using LSR.XmlHelper.Wpf.Services.EditHistory;
using LSR.XmlHelper.Wpf.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Media = System.Windows.Media;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;
using MessageBoxResult = System.Windows.MessageBoxResult;
using WinForms = System.Windows.Forms;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class MainWindowViewModel : ObservableObject
    {
        private readonly XmlDocumentService _xml;
        private readonly XmlFileDiscoveryService _discovery;
        private readonly XmlFileLoaderService _loader;
        private readonly XmlFileSaveService _saver;
        private readonly XmlFriendlyViewService _friendly;
        private readonly XmlGlobalSearchService _globalSearch;
        private readonly AppSettingsService _settingsService;
        private readonly bool _isFirstRun;
        private AppSettings _settings;
        private readonly EditHistoryService _editHistory;
        private readonly XmlBackupRequestService _backupRequest;
        private readonly AppearanceService _appearance;
        private CancellationTokenSource? _loadCts;
        private CancellationTokenSource? _friendlyBuildCts;
        private CancellationTokenSource? _friendlyUiBuildCts;
        private CancellationTokenSource? _friendlyFieldsBuildCts;
        private int _xmlVersion;
        private bool _suppressFriendlyRebuild;
        private bool _suppressDirtyTracking;
        private string _title = "LSR XML Helper";
        private string _status = "Ready.";
        private string _xmlText = "";
        private string? _rootFolder;
        private RawNavigationRequest? _pendingRawNavigation;
        private string _friendlySearchQuery = "";
        private bool _friendlySearchCaseSensitive;
        private int _friendlySearchCollectionIndex;
        private int _friendlySearchEntryIndex;
        private int _friendlySearchFieldIndex = -1;
        private string? _friendlySearchFieldName;
        private string? _pendingFriendlySearchQuery;
        private bool _pendingFriendlySearchCaseSensitive;
        private string? _pendingFriendlySearchFieldName;
        private string? _pendingGlobalFriendlyNavigateCollectionTitle;
        private string? _pendingGlobalFriendlyNavigateEntryKey;
        private int? _pendingGlobalFriendlyNavigateEntryOccurrence;
        private string? _pendingGlobalFriendlyNavigateFieldName;    
        private GlobalSearchWindow? _globalSearchWindow;
        private GlobalSearchScope _globalSearchScope = GlobalSearchScope.Both;
        private bool _globalSearchUseParallelProcessing = true;
        public event EventHandler<RawNavigationRequest>? RawNavigationRequested;
        public event Action<string>? LookupGridGroupExpandRequested;
        private bool _isDirty;
        private XmlFileListItem? _selectedXmlFile;
        private XmlExplorerNode? _selectedTreeNode;
        private string? _currentFilePath;
        private XmlListViewMode _viewMode = XmlListViewMode.Flat;
        private bool _includeSubfolders;
        private bool _hasFriendlyView;
        private bool _isFriendlyView;
        private XmlFriendlyDocument? _friendlyDocument;
        private bool _isDarkMode = false;
        private ObservableCollection<XmlFriendlyCollectionViewModel> _friendlyCollections = new();
        private XmlFriendlyCollectionViewModel? _selectedFriendlyCollection;
        private XmlFriendlyEntryViewModel? _selectedFriendlyEntry;
        private XmlFriendlyLookupItemViewModel? _selectedFriendlyLookupItem;
        private int _friendlyLookupScrollRequestId;
        private string? _pendingLookupGroupTitle;
        private string? _pendingLookupItemName;
        private string? _pendingLookupField;
        private readonly FriendlyGroupExpansionStateStore _friendlyExpansionStateStore = new();
        private readonly LookupGridGroupExpansionStateStore _lookupGridGroupExpansionStateStore = new();
        private ObservableCollection<XmlFriendlyFieldViewModel> _friendlyFields = new();
        private ObservableCollection<XmlFriendlyFieldGroupViewModel> _friendlyFieldGroups = new();
        private ObservableCollection<object> _friendlyGroups = new();
        private object? _selectedFriendlyGroup;

        public MainWindowViewModel()
        {
            _xml = new XmlDocumentService();
            _discovery = new XmlFileDiscoveryService();
            _loader = new XmlFileLoaderService();
            _saver = new XmlFileSaveService();
            _friendly = new XmlFriendlyViewService();
            _globalSearch = new XmlGlobalSearchService();

            _settingsService = new AppSettingsService();
            _settings = _settingsService.Load(out _isFirstRun);
            _editHistory = new EditHistoryService(_settings, _settingsService, _friendly, _xml);
            _backupRequest = new XmlBackupRequestService();

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
            DuplicateFriendlyEntryCommand = new RelayCommand(DuplicateSelectedFriendlyEntry, () => IsFriendlyView && _friendlyDocument is not null && SelectedFriendlyEntry is not null);
            DuplicateFriendlyLookupItemCommand = new RelayCommand(DuplicateSelectedFriendlyLookupItem, () => IsFriendlyView && _friendlyDocument is not null && SelectedFriendlyEntry is not null && SelectedFriendlyLookupItem is not null);
            DeleteFriendlyLookupItemCommand = new RelayCommand(DeleteSelectedFriendlyLookupItem, () => IsFriendlyView && _friendlyDocument is not null && SelectedFriendlyEntry is not null && SelectedFriendlyLookupItem is not null);
            DeleteFriendlyEntryCommand = new RelayCommand(DeleteSelectedFriendlyEntry, () => IsFriendlyView && _friendlyDocument is not null && SelectedFriendlyEntry is not null);

            OpenAppearanceCommand = new RelayCommand(OpenAppearance);
            OpenGlobalSearchCommand = new RelayCommand(OpenGlobalSearch);
            OpenSavedEditsCommand = new RelayCommand(OpenSavedEdits);
            OpenSharedConfigPacksCommand = new RelayCommand(OpenSharedConfigPacks, () => !string.IsNullOrWhiteSpace(_rootFolder));

            OpenBackupBrowserCommand = new RelayCommand(OpenBackupBrowser, () =>
            {
                var p = GetSelectedFilePath();
                if (p is not null && File.Exists(p))
                    return true;

                return !string.IsNullOrWhiteSpace(_rootFolder) && Directory.Exists(_rootFolder) && XmlFiles.Any();
            });

            if (!string.IsNullOrWhiteSpace(_rootFolder))
            {
                RefreshFileViews(resetEditorAndSelection: true);
                Title = $"LSR XML Helper - {_rootFolder}  |  Created by Y0sh1M1tsu";
            }
        }

        public AppearanceService Appearance => _appearance;
        public bool IsFirstRun => _isFirstRun;
        public string? RootFolderPath => _rootFolder;
        public RelayCommand OpenAppearanceCommand { get; }
        public RelayCommand OpenBackupBrowserCommand { get; }
        public RelayCommand OpenGlobalSearchCommand { get; }
        public RelayCommand OpenSavedEditsCommand { get; }
        public RelayCommand OpenSharedConfigPacksCommand { get; }

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
        public bool GlobalSearchUseParallelProcessing
        {
            get => _globalSearchUseParallelProcessing;
            set
            {
                if (!SetProperty(ref _globalSearchUseParallelProcessing, value))
                    return;

                _settings.GlobalSearchUseParallelProcessing = value;
                SaveSettings();
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

                SelectedFriendlyEntry = null;
            }
        }

        public XmlFriendlyEntryViewModel? SelectedFriendlyEntry
        {
            get => _selectedFriendlyEntry;
            set
            {
                if (!SetProperty(ref _selectedFriendlyEntry, value))
                    return;

                SelectedFriendlyLookupItem = null;
                QueueRebuildFieldsForSelectedEntry();
                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
            }
        }
        public int FriendlyLookupScrollRequestId
        {
            get => _friendlyLookupScrollRequestId;
            private set => SetProperty(ref _friendlyLookupScrollRequestId, value);
        }

        public void RequestFriendlyLookupScroll()
        {
            FriendlyLookupScrollRequestId++;
        }
        public XmlFriendlyLookupItemViewModel? SelectedFriendlyLookupItem
        {
            get => _selectedFriendlyLookupItem;
            set
            {
                if (!SetProperty(ref _selectedFriendlyLookupItem, value))
                    return;

                System.Windows.Input.CommandManager.InvalidateRequerySuggested();
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

        public object? SelectedFriendlyGroup
        {
            get => _selectedFriendlyGroup;
            set => SetProperty(ref _selectedFriendlyGroup, value);
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
                    _friendlyExpansionStateStore.Clear();
                    _lookupGridGroupExpansionStateStore.Clear();
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
        public RelayCommand DuplicateFriendlyEntryCommand { get; }
        public RelayCommand DuplicateFriendlyLookupItemCommand { get; }
        public RelayCommand DeleteFriendlyLookupItemCommand { get; }
        public RelayCommand DeleteFriendlyEntryCommand { get; }

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

        private void OpenGlobalSearch()
        {
            var desiredTab = IsFriendlyView ? 1 : 0;
            var desiredScope = _globalSearchScope == GlobalSearchScope.Both
                ? GlobalSearchScope.Both
                : (IsFriendlyView ? GlobalSearchScope.FriendlyView : GlobalSearchScope.RawXml);

            if (_globalSearchWindow is null)
            {
                void onScopeChanged(GlobalSearchScope scope)
                {
                    _globalSearchScope = scope;
                    _settings.GlobalSearchScope = scope.ToString();
                    SaveSettings();
                }

                var vm = new GlobalSearchWindowViewModel(
                    _discovery,
                    _globalSearch,
                    () => RootFolderPath,
                    () => IncludeSubfolders,
                    v => IncludeSubfolders = v,
                    () => GlobalSearchUseParallelProcessing,
                    v => GlobalSearchUseParallelProcessing = v,
                    NavigateToGlobalHit,
                    NavigateToGlobalFriendlyHit,
                    desiredTab,
                    desiredScope,
                    onScopeChanged);

                _globalSearchWindow = new GlobalSearchWindow
                {
                    Owner = System.Windows.Application.Current?.MainWindow,
                    DataContext = vm
                };

                _globalSearchWindow.Closed += (_, _) => _globalSearchWindow = null;
            }
            else
            {
                if (_globalSearchWindow.DataContext is GlobalSearchWindowViewModel vm)
                {
                    vm.SelectedTabIndex = desiredTab;
                    vm.SearchScope = desiredScope;
                }
            }

            if (!_globalSearchWindow.IsVisible)
                _globalSearchWindow.Show();

            if (_globalSearchWindow.WindowState == WindowState.Minimized)
                _globalSearchWindow.WindowState = WindowState.Normal;

            _globalSearchWindow.Activate();
        }

        private void NavigateToGlobalHit(GlobalSearchHit hit, string query, bool caseSensitive)
        {
            if (hit is null)
                return;

            if (IsFriendlyView)
                IsFriendlyView = false;

            NavigateToRawHit(hit);
        }

        private void NavigateToRawHit(GlobalSearchHit hit)
        {
            if (hit is null)
                return;

            _pendingRawNavigation = new RawNavigationRequest(hit.FilePath, hit.Offset, hit.Length);

            if (IsFoldersMode)
            {
                var node = FindFileNodeByPath(XmlTree, hit.FilePath);
                if (node is not null)
                {
                    if (ReferenceEquals(node, SelectedTreeNode) && node.FullPath is not null &&
                        string.Equals(_currentFilePath, node.FullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        TryRaiseRawNavigationForLoadedFile(node.FullPath);
                        return;
                    }

                    SelectedTreeNode = node;
                    return;
                }
            }
            else
            {
                var match = XmlFiles.FirstOrDefault(x =>
                    string.Equals(x.FullPath, hit.FilePath, StringComparison.OrdinalIgnoreCase));

                if (match is not null)
                {
                    if (ReferenceEquals(match, SelectedXmlFile) &&
                        string.Equals(_currentFilePath, match.FullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        TryRaiseRawNavigationForLoadedFile(match.FullPath);
                        return;
                    }

                    SelectedXmlFile = match;
                    return;
                }
            }

            _ = LoadFileAsync(hit.FilePath);
        }

        private void NavigateToGlobalFriendlyHit(GlobalFriendlySearchHit hit, string query, bool caseSensitive)
        {
            if (hit is null)
                return;

            _pendingFriendlySearchQuery = query;
            _pendingFriendlySearchCaseSensitive = caseSensitive;

            _pendingGlobalFriendlyNavigateCollectionTitle = hit.CollectionTitle;
            _pendingGlobalFriendlyNavigateEntryKey = hit.EntryKey;
            _pendingGlobalFriendlyNavigateEntryOccurrence = hit.EntryOccurrence;
            _pendingGlobalFriendlyNavigateFieldName = hit.FieldKey;

            if (!IsFriendlyView)
                IsFriendlyView = true;

            if (IsFoldersMode)
            {
                var node = FindFileNodeByPath(XmlTree, hit.FilePath);
                if (node is not null)
                {
                    var fullPath = node.FullPath;

                    if (!string.IsNullOrWhiteSpace(fullPath) &&
                        ReferenceEquals(node, SelectedTreeNode) &&
                        string.Equals(_currentFilePath, fullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        TryApplyGlobalFriendlyNavigateForLoadedFile(fullPath);
                        return;
                    }

                    SelectedTreeNode = node;
                    return;
                }
            }
            else
            {
                var match = XmlFiles.FirstOrDefault(x =>
                    string.Equals(x.FullPath, hit.FilePath, StringComparison.OrdinalIgnoreCase));

                if (match is not null)
                {
                    if (ReferenceEquals(match, SelectedXmlFile) &&
                        string.Equals(_currentFilePath, match.FullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        TryApplyGlobalFriendlyNavigateForLoadedFile(match.FullPath);
                        return;
                    }

                    SelectedXmlFile = match;
                    return;
                }
            }

            _ = LoadFileAsync(hit.FilePath);
        }

        private XmlExplorerNode? FindFileNodeByPath(IEnumerable<XmlExplorerNode> nodes, string fullPath)
        {
            foreach (var node in nodes)
            {
                if (node.IsFile && node.FullPath is not null &&
                    string.Equals(node.FullPath, fullPath, StringComparison.OrdinalIgnoreCase))
                    return node;

                if (node.Children.Count > 0)
                {
                    var match = FindFileNodeByPath(node.Children, fullPath);
                    if (match is not null)
                        return match;
                }
            }

            return null;
        }

        private void OpenSavedEdits()
        {
            var vm = new SavedEditsWindowViewModel(
                _editHistory,
                GetSelectedFilePath,
                TryApplySavedEditsToCurrent,
                _backupRequest,
                () => RootFolderPath);

            var win = new SavedEditsWindow
            {
                Owner = System.Windows.Application.Current?.MainWindow,
                DataContext = vm
            };

            win.ShowDialog();
        }

        private bool TryApplySavedEditsToCurrent(IEnumerable<EditHistoryItem> edits)
        {
            var list = edits?.ToList() ?? new List<EditHistoryItem>();
            if (list.Count == 0)
                return true;

            if (!_editHistory.TryApplyToXmlText(XmlText, list, out var updated, out var error))
            {
                Status = error ?? "Saved edits could not be applied.";
                return false;
            }

            XmlText = updated;
            IsDirty = true;
            Status = $"Applied {list.Count} saved edit(s).";
            return true;
        }
        private void OpenSharedConfigPacks()
        {
            if (string.IsNullOrWhiteSpace(_rootFolder))
            {
                MessageBox.Show("Open a folder first.", "Shared Config Packs", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var packs = new Services.SharedConfigs.SharedConfigPackService(_settingsService, new Services.SharedConfigs.SettingsCopyService());
            var vm = new SharedConfigPacksWindowViewModel(() => _rootFolder, _settings, packs, _editHistory, _backupRequest);

            var win = new SharedConfigPacksWindow
            {
                Owner = System.Windows.Application.Current?.MainWindow,
                DataContext = vm
            };

            win.ShowDialog();
        }

        private void OpenBackupBrowser()
        {
            if (!TryConfirmDiscardOrSaveIfDirty())
                return;

            var path = GetSelectedFilePath();
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                var first = XmlFiles.FirstOrDefault();
                if (first is null || string.IsNullOrWhiteSpace(first.FullPath) || !File.Exists(first.FullPath))
                {
                    MessageBox.Show("No XML files were found in the opened folder.", "Restore from Backup", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                path = first.FullPath;
            }

            var vm = new BackupBrowserWindowViewModel(path, _appearance);
            var win = new BackupBrowserWindow
            {
                Owner = System.Windows.Application.Current?.MainWindow,
                DataContext = vm
            };

            var result = win.ShowDialog();
            if (result == true)
            {
                var restored = vm.RestoredXmlPath;
                if (!string.IsNullOrWhiteSpace(restored) && File.Exists(restored))
                {
                    _ = LoadFileAsync(restored);
                    Status = $"Restored backup: {Path.GetFileName(restored)}";
                }
                else
                {
                    _ = LoadFileAsync(path);
                    Status = $"Restored backup: {Path.GetFileName(path)}";
                }
            }
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
            _editHistory.CommitForFile(path);
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
            _editHistory.CommitForFile(dlg.FileName);
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
                _friendlyExpansionStateStore.Clear();
                _lookupGridGroupExpansionStateStore.Clear();
                ClearFriendlyFields();
                return;
            }

            var selectedCollectionTitle = _pendingGlobalFriendlyNavigateCollectionTitle ?? SelectedFriendlyCollection?.Collection.Title;
            var selectedEntryKey = _pendingGlobalFriendlyNavigateEntryKey ?? SelectedFriendlyEntry?.Entry.Key;
            var selectedEntryOccurrence = _pendingGlobalFriendlyNavigateEntryOccurrence;
            _pendingFriendlySearchFieldName = _pendingGlobalFriendlyNavigateFieldName;
            _pendingGlobalFriendlyNavigateCollectionTitle = null;
            _pendingGlobalFriendlyNavigateEntryKey = null;
            _pendingGlobalFriendlyNavigateEntryOccurrence = null;
            _pendingGlobalFriendlyNavigateFieldName = null;

            _friendlyExpansionStateStore.Capture(FriendlyGroups);
            _friendlyUiBuildCts = new CancellationTokenSource();
            var token = _friendlyUiBuildCts.Token;
            var myVersion = _xmlVersion;
            var doc = _friendlyDocument;

            _ = Task.Run(() => BuildFriendlyUiState(doc, selectedCollectionTitle, selectedEntryKey, selectedEntryOccurrence, token), token).ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted || myVersion != _xmlVersion)
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

        private void CancelPendingFriendlyFieldsBuild()
        {
            try
            {
                _friendlyFieldsBuildCts?.Cancel();
                _friendlyFieldsBuildCts?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _friendlyFieldsBuildCts = null;
            }
        }

        public bool TryGetLookupGridGroupIsExpanded(string groupName, out bool expanded)
        {
            expanded = false;

            var collectionTitle = SelectedFriendlyCollection?.Collection.Title;
            var entryKey = SelectedFriendlyEntry?.Entry.Key;

            if (string.IsNullOrWhiteSpace(collectionTitle) || string.IsNullOrWhiteSpace(entryKey))
                return false;

            var key = $"{collectionTitle}||{entryKey}||{groupName}";
            return _lookupGridGroupExpansionStateStore.TryGet(key, out expanded);
        }

        public void SetLookupGridGroupIsExpanded(string groupName, bool expanded)
        {
            var collectionTitle = SelectedFriendlyCollection?.Collection.Title;
            var entryKey = SelectedFriendlyEntry?.Entry.Key;

            if (string.IsNullOrWhiteSpace(collectionTitle) || string.IsNullOrWhiteSpace(entryKey))
                return;

            var key = $"{collectionTitle}||{entryKey}||{groupName}";
            _lookupGridGroupExpansionStateStore.Set(key, expanded);
        }


        private void ApplyFriendlyUiState(FriendlyUiState state)
        {
            HasFriendlyView = state.Collections.Count > 0;

            FriendlyCollections = state.Collections;

            SelectedFriendlyCollection = state.SelectedCollection;
            SelectedFriendlyEntry = state.SelectedEntry;

            DetachFriendlyFieldHandlers(_friendlyFields);

            foreach (var f in state.Fields)
                f.PropertyChanged += FriendlyField_PropertyChanged;

            FriendlyFields = state.Fields;
            FriendlyFieldGroups = state.FieldGroups;
            _friendlyExpansionStateStore.Apply(state.Groups);
            FriendlyGroups = state.Groups;

            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        private static FriendlyUiState BuildFriendlyUiState(
            XmlFriendlyDocument doc,
            string? selectedCollectionTitle,
            string? selectedEntryKey,
            int? selectedEntryOccurrence,
            CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var cols = doc.Collections.Select(c => new XmlFriendlyCollectionViewModel(c)).ToList();
            var collections = new ObservableCollection<XmlFriendlyCollectionViewModel>(cols);

            XmlFriendlyCollectionViewModel? selectedCollection = null;
            if (!string.IsNullOrWhiteSpace(selectedCollectionTitle))
                selectedCollection = collections.FirstOrDefault(c => string.Equals(c.Collection.Title, selectedCollectionTitle, StringComparison.OrdinalIgnoreCase));
            selectedCollection ??= collections.FirstOrDefault();

            XmlFriendlyEntryViewModel? selectedEntry = null;
            if (selectedCollection is not null && !string.IsNullOrWhiteSpace(selectedEntryKey))
            {
                if (selectedEntryOccurrence.HasValue)
                    selectedEntry = selectedCollection.Entries.FirstOrDefault(e => string.Equals(e.Entry.Key, selectedEntryKey, StringComparison.OrdinalIgnoreCase) && e.Entry.Occurrence == selectedEntryOccurrence.Value);
                if (selectedEntry is null)
                    selectedEntry = selectedCollection.Entries.FirstOrDefault(e => string.Equals(e.Entry.Key, selectedEntryKey, StringComparison.OrdinalIgnoreCase));
            }

            return new FriendlyUiState(collections, selectedCollection, selectedEntry, new ObservableCollection<XmlFriendlyFieldViewModel>(), new ObservableCollection<XmlFriendlyFieldGroupViewModel>(), new ObservableCollection<object>());
        }

        private void ClearFriendlyFields()
        {
            DetachFriendlyFieldHandlers(_friendlyFields);

            FriendlyFields = new ObservableCollection<XmlFriendlyFieldViewModel>();
            FriendlyFieldGroups = new ObservableCollection<XmlFriendlyFieldGroupViewModel>();
            FriendlyGroups = new ObservableCollection<object>();
        }

        private void DetachFriendlyFieldHandlers(ObservableCollection<XmlFriendlyFieldViewModel> fields)
        {
            foreach (var f in fields)
                f.PropertyChanged -= FriendlyField_PropertyChanged;
        }

        private sealed record FriendlyFieldsState(
            ObservableCollection<XmlFriendlyFieldViewModel> Fields,
            ObservableCollection<XmlFriendlyFieldGroupViewModel> FieldGroups,
            ObservableCollection<object> Groups);

        private void QueueRebuildFieldsForSelectedEntry()
        {
            CancelPendingFriendlyFieldsBuild();

            var entry = SelectedFriendlyEntry?.Entry;
            if (entry is null)
            {
                _friendlyExpansionStateStore.Capture(FriendlyGroups);
                ClearFriendlyFields();
                return;
            }

            _friendlyExpansionStateStore.Capture(FriendlyGroups);
            ClearFriendlyFields();

            var myVersion = _xmlVersion;

            _friendlyFieldsBuildCts = new CancellationTokenSource();
            var token = _friendlyFieldsBuildCts.Token;

            _ = Task.Run(() => BuildFriendlyFieldsState(entry, token), token).ContinueWith(t =>
            {
                if (t.IsCanceled || t.IsFaulted)
                    return;

                if (myVersion != _xmlVersion)
                    return;

                ApplyFriendlyFieldsState(t.Result);
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private static FriendlyFieldsState BuildFriendlyFieldsState(XmlFriendlyEntry entry, CancellationToken token)
        {
            static int CompareSegment(string a, string b)
            {
                static (string Name, int? Index) Parse(string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return ("", null);

                    var lb = s.LastIndexOf('[');
                    if (lb > 0 && s.EndsWith("]", StringComparison.Ordinal))
                    {
                        var indexText = s.Substring(lb + 1, s.Length - lb - 2);
                        if (int.TryParse(indexText, out var idx) && idx >= 0)
                            return (s.Substring(0, lb), idx);
                    }

                    return (s, null);
                }

                var pa = Parse(a);
                var pb = Parse(b);

                var nameCompare = StringComparer.OrdinalIgnoreCase.Compare(pa.Name, pb.Name);
                if (nameCompare != 0)
                    return nameCompare;

                if (pa.Index.HasValue && pb.Index.HasValue)
                    return pa.Index.Value.CompareTo(pb.Index.Value);

                if (pa.Index.HasValue)
                    return 1;

                if (pb.Index.HasValue)
                    return -1;

                return StringComparer.OrdinalIgnoreCase.Compare(a, b);
            }

            static int CompareFieldPath(string? a, string? b)
            {
                if (ReferenceEquals(a, b))
                    return 0;

                if (a is null)
                    return -1;

                if (b is null)
                    return 1;

                var aParts = a.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var bParts = b.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var len = Math.Min(aParts.Length, bParts.Length);

                for (var i = 0; i < len; i++)
                {
                    var cmp = CompareSegment(aParts[i], bParts[i]);
                    if (cmp != 0)
                        return cmp;
                }

                return aParts.Length.CompareTo(bParts.Length);
            }

            token.ThrowIfCancellationRequested();

            var source = entry.Fields;

            IEnumerable<KeyValuePair<string, XmlFriendlyField>> ordered =
                source.Count <= 5000
                    ? source.OrderBy(k => k.Key, Comparer<string>.Create(CompareFieldPath))
                    : source;

            var fields = new List<XmlFriendlyFieldViewModel>(source.Count);

            foreach (var kv in ordered)
            {
                token.ThrowIfCancellationRequested();
                fields.Add(new XmlFriendlyFieldViewModel(kv.Key, kv.Value.Value ?? ""));
            }

            var fieldsVm = new ObservableCollection<XmlFriendlyFieldViewModel>(fields);
            var fieldGroupsVm = BuildGroupsFromFields(fieldsVm);
            var groupsVm = BuildUnifiedGroups(fieldsVm);

            return new FriendlyFieldsState(fieldsVm, fieldGroupsVm, groupsVm);
        }

        private void ApplyFriendlyFieldsState(FriendlyFieldsState state)
        {
            var pendingGroup = _pendingLookupGroupTitle;
            var pendingItem = _pendingLookupItemName;
            var pendingField = _pendingLookupField;

            _pendingLookupGroupTitle = null;
            _pendingLookupItemName = null;
            _pendingLookupField = null;

            var previous = SelectedFriendlyLookupItem;
            var previousItem = previous?.Item;
            var previousField = previous?.Field;

            FriendlyFields = state.Fields;
            FriendlyFieldGroups = state.FieldGroups;
            _friendlyExpansionStateStore.Apply(state.Groups);
            FriendlyGroups = state.Groups;

            XmlFriendlyLookupItemViewModel? match = null;

            if (!string.IsNullOrWhiteSpace(pendingItem) && !string.IsNullOrWhiteSpace(pendingField))
            {
                match = state.Groups
                    .OfType<XmlFriendlyLookupGroupViewModel>()
                    .Where(g => string.IsNullOrWhiteSpace(pendingGroup) || string.Equals(g.Title, pendingGroup, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(g => g.Items)
                    .FirstOrDefault(i =>
                        string.Equals(i.Item, pendingItem, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(i.Field, pendingField, StringComparison.OrdinalIgnoreCase));
            }

            if (match is null && !string.IsNullOrWhiteSpace(previousItem) && !string.IsNullOrWhiteSpace(previousField))
            {
                match = state.Groups
                    .OfType<XmlFriendlyLookupGroupViewModel>()
                    .SelectMany(g => g.Items)
                    .FirstOrDefault(i =>
                        string.Equals(i.Item, previousItem, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(i.Field, previousField, StringComparison.OrdinalIgnoreCase));
            }

            if (match is not null)
            {
                SelectedFriendlyLookupItem = match;
                FriendlyLookupScrollRequestId++;
            }

            ApplyFriendlySearchHighlight(state);
        }

        private void ApplyFriendlySearchHighlight(FriendlyFieldsState state)
        {
            var query = _pendingFriendlySearchQuery;
            var caseSensitive = _pendingFriendlySearchCaseSensitive;
            var targetFieldName = _pendingFriendlySearchFieldName;

            _pendingFriendlySearchQuery = null;
            _pendingFriendlySearchFieldName = null;

            foreach (var f in state.Fields)
                f.IsSearchMatch = false;

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            XmlFriendlyFieldViewModel? target = null;

            if (!string.IsNullOrWhiteSpace(targetFieldName))
                target = state.Fields.FirstOrDefault(f => string.Equals(f.Name, targetFieldName, StringComparison.OrdinalIgnoreCase));

            if (target is null && !string.IsNullOrWhiteSpace(query))
            {
                foreach (var f in state.Fields)
                {
                    if (f.Name.IndexOf(query, comparison) >= 0 || (f.Value ?? "").IndexOf(query, comparison) >= 0)
                    {
                        target = f;
                        break;
                    }
                }
            }

            if (target is null)
                return;

            target.IsSearchMatch = true;

            foreach (var obj in state.Groups)
            {
                if (obj is XmlFriendlyFieldGroupViewModel group)
                {
                    if (!group.Fields.Contains(target))
                        continue;

                    group.IsExpanded = true;
                    group.SelectedField = null;
                    group.SelectedField = target;

                    SelectedFriendlyGroup = null;
                    SelectedFriendlyGroup = group;
                    return;
                }

                if (obj is XmlFriendlyLookupGroupViewModel lookupGroup)
                {
                    var lookupItem = lookupGroup.Items.FirstOrDefault(i =>
                        string.Equals(i.FullName, target.Name, StringComparison.OrdinalIgnoreCase));

                    if (lookupItem is null)
                        continue;

                    lookupGroup.IsExpanded = true;
                    SetLookupGridGroupIsExpanded(lookupItem.Item, true);
                    LookupGridGroupExpandRequested?.Invoke(lookupItem.Item);

                    SelectedFriendlyGroup = null;
                    SelectedFriendlyGroup = lookupGroup;

                    SelectedFriendlyLookupItem = null;
                    SelectedFriendlyLookupItem = lookupItem;
                    return;
                }
            }
        }
        private static ObservableCollection<object> BuildUnifiedGroups(ObservableCollection<XmlFriendlyFieldViewModel> fields)
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

            static bool TrySplitLookup(string name, out string section, out string item, out string leafField)
            {
                section = "";
                item = "";
                leafField = "";

                if (string.IsNullOrWhiteSpace(name))
                    return false;

                var parts = name.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length < 3)
                    return false;

                var first = parts[0];
                var second = parts[1];

                if (string.IsNullOrWhiteSpace(first) || string.IsNullOrWhiteSpace(second))
                    return false;

                var lb = second.IndexOf('[', StringComparison.Ordinal);
                var rb = second.EndsWith("]", StringComparison.Ordinal);

                if (lb <= 0 || !rb)
                    return false;

                section = first;
                item = second;
                leafField = string.Join("/", parts.Skip(2));
                return true;
            }

            var lookupItemCounts = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var f in fields)
            {
                if (TrySplitLookup(f.Name, out var section, out var item, out _))
                {
                    if (!lookupItemCounts.TryGetValue(section, out var set))
                    {
                        set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        lookupItemCounts[section] = set;
                    }

                    set.Add(item);
                }
            }

            var lookupSections = new HashSet<string>(
                lookupItemCounts
                    .Where(kvp => kvp.Value.Count >= 2)
                    .Select(kvp => kvp.Key),
                StringComparer.OrdinalIgnoreCase);

            var lookupGroupsInOrder = new List<string>();
            var lookupItemsInOrder = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            var lookupRows = new Dictionary<string, Dictionary<string, List<XmlFriendlyLookupItemViewModel>>>(StringComparer.OrdinalIgnoreCase);

            var normalGroupsInOrder = new List<string>();
            var normalRows = new Dictionary<string, List<XmlFriendlyFieldViewModel>>(StringComparer.OrdinalIgnoreCase);

            foreach (var f in fields)
            {
                if (TrySplitLookup(f.Name, out var section, out var itemName, out var leaf) && lookupSections.Contains(section))
                {
                    if (!lookupRows.TryGetValue(section, out var itemsByName))
                    {
                        itemsByName = new Dictionary<string, List<XmlFriendlyLookupItemViewModel>>(StringComparer.OrdinalIgnoreCase);
                        lookupRows[section] = itemsByName;
                        lookupGroupsInOrder.Add(section);
                        lookupItemsInOrder[section] = new List<string>();
                    }

                    if (!itemsByName.TryGetValue(itemName, out var list))
                    {
                        list = new List<XmlFriendlyLookupItemViewModel>();
                        itemsByName[itemName] = list;
                        lookupItemsInOrder[section].Add(itemName);
                    }

                    list.Add(new XmlFriendlyLookupItemViewModel(itemName, leaf, f));
                    continue;
                }

                var normalGroup = GetGroupTitle(f.Name);

                if (!normalRows.TryGetValue(normalGroup, out var normalList))
                {
                    normalList = new List<XmlFriendlyFieldViewModel>();
                    normalRows[normalGroup] = normalList;
                    normalGroupsInOrder.Add(normalGroup);
                }

                normalList.Add(f);
            }

            var output = new List<object>();

            if (normalRows.TryGetValue("General", out var generalList))
            {
                output.Add(new XmlFriendlyFieldGroupViewModel("General", new ObservableCollection<XmlFriendlyFieldViewModel>(generalList)));
                normalRows.Remove("General");
                normalGroupsInOrder.RemoveAll(x => string.Equals(x, "General", StringComparison.OrdinalIgnoreCase));
            }

            foreach (var lookupGroup in lookupGroupsInOrder)
            {
                if (!lookupRows.TryGetValue(lookupGroup, out var itemsByName))
                    continue;

                var rows = new List<XmlFriendlyLookupItemViewModel>();

                foreach (var item in lookupItemsInOrder[lookupGroup])
                {
                    if (itemsByName.TryGetValue(item, out var list))
                        rows.AddRange(list);
                }

                output.Add(new XmlFriendlyLookupGroupViewModel(
                    lookupGroup,
                    new ObservableCollection<XmlFriendlyLookupItemViewModel>(rows)));
            }

            foreach (var g in normalGroupsInOrder)
            {
                if (!normalRows.TryGetValue(g, out var list))
                    continue;

                output.Add(new XmlFriendlyFieldGroupViewModel(
                    g,
                    new ObservableCollection<XmlFriendlyFieldViewModel>(list)));
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

            entry.Fields.TryGetValue(field.Name, out var existingField);
            var previousValue = existingField?.Value;

            var entryKeyBeforeEdit = entry.Key;

            var occurrenceBeforeEdit = 0;
            var collectionBeforeEdit = SelectedFriendlyCollection?.Collection;
            if (collectionBeforeEdit is not null)
            {
                var matchesBefore = 0;

                foreach (var e2 in collectionBeforeEdit.Entries)
                {
                    if (!string.Equals(e2.Key, entryKeyBeforeEdit, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (ReferenceEquals(e2, entry))
                    {
                        occurrenceBeforeEdit = matchesBefore;
                        break;
                    }

                    matchesBefore++;
                }
            }

            if (!entry.TrySetField(field.Name, field.Value, out _))
                return;

            entry.InvalidateFields();
            SelectedFriendlyEntry?.RefreshDisplay();

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

            var filePath = GetSelectedFilePath();
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                var occurrence = occurrenceBeforeEdit;

                _editHistory.AddPending(
                    filePath,
                    SelectedFriendlyCollection?.Title,
                    entryKeyBeforeEdit,
                    occurrence,
                    field.Name,
                    previousValue,
                    field.Value);
            }

            IsDirty = true;
            Status = "Edited.";
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
        public void FindNextFriendly(string query, bool caseSensitive)
        {
            if (!IsFriendlyView)
                return;

            if (_friendlyDocument is null)
                return;

            if (string.IsNullOrWhiteSpace(query))
                return;

            static int CompareSegment(string a, string b)
            {
                static (string Name, int? Index) Parse(string s)
                {
                    if (string.IsNullOrWhiteSpace(s))
                        return ("", null);

                    var lb = s.LastIndexOf('[');
                    if (lb > 0 && s.EndsWith("]", StringComparison.Ordinal))
                    {
                        var indexText = s.Substring(lb + 1, s.Length - lb - 2);
                        if (int.TryParse(indexText, out var idx) && idx >= 0)
                            return (s.Substring(0, lb), idx);
                    }

                    return (s, null);
                }

                var pa = Parse(a);
                var pb = Parse(b);

                var nameCompare = StringComparer.OrdinalIgnoreCase.Compare(pa.Name, pb.Name);
                if (nameCompare != 0)
                    return nameCompare;

                if (pa.Index.HasValue && pb.Index.HasValue)
                    return pa.Index.Value.CompareTo(pb.Index.Value);

                if (pa.Index.HasValue)
                    return 1;

                if (pb.Index.HasValue)
                    return -1;

                return StringComparer.OrdinalIgnoreCase.Compare(a, b);
            }

            static int CompareFieldPath(string? a, string? b)
            {
                if (ReferenceEquals(a, b))
                    return 0;

                if (a is null)
                    return -1;

                if (b is null)
                    return 1;

                var aParts = a.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                var bParts = b.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                var len = Math.Min(aParts.Length, bParts.Length);

                for (var i = 0; i < len; i++)
                {
                    var cmp = CompareSegment(aParts[i], bParts[i]);
                    if (cmp != 0)
                        return cmp;
                }

                return aParts.Length.CompareTo(bParts.Length);
            }

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            if (!string.Equals(_friendlySearchQuery, query, StringComparison.Ordinal) || _friendlySearchCaseSensitive != caseSensitive)
            {
                _friendlySearchQuery = query;
                _friendlySearchCaseSensitive = caseSensitive;

                _friendlySearchCollectionIndex = 0;
                _friendlySearchEntryIndex = 0;
                _friendlySearchFieldIndex = -1;
                _friendlySearchFieldName = null;
            }

            var matches = new List<(int CollectionIndex, int EntryIndex, int FieldIndex, string? FieldName)>();

            for (var c = 0; c < _friendlyDocument.Collections.Count; c++)
            {
                var col = _friendlyDocument.Collections[c];
                for (var e = 0; e < col.Entries.Count; e++)
                {
                    var entry = col.Entries[e];

                    var ordered = entry.Fields
                        .OrderBy(kv => kv.Key, Comparer<string>.Create(CompareFieldPath))
                        .ToList();

                    for (var f = 0; f < ordered.Count; f++)
                    {
                        var fieldKey = ordered[f].Key ?? "";
                        var fieldValue = ordered[f].Value?.Value ?? "";

                        if (fieldKey.IndexOf(query, comparison) < 0 && fieldValue.IndexOf(query, comparison) < 0)
                            continue;

                        matches.Add((CollectionIndex: c, EntryIndex: e, FieldIndex: f, FieldName: fieldKey));
                    }
                }
            }

            if (matches.Count == 0)
            {
                MessageBox.Show("No matches found.", "Find", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var currentIndex = -1;
            var currentFieldName = _friendlySearchFieldName;

            for (var i = 0; i < matches.Count; i++)
            {
                var m = matches[i];

                if (m.CollectionIndex != _friendlySearchCollectionIndex || m.EntryIndex != _friendlySearchEntryIndex)
                    continue;

                if (!string.IsNullOrWhiteSpace(currentFieldName) &&
                    !string.IsNullOrWhiteSpace(m.FieldName) &&
                    string.Equals(m.FieldName, currentFieldName, StringComparison.OrdinalIgnoreCase))
                {
                    currentIndex = i;
                    break;
                }

                if (m.FieldIndex == _friendlySearchFieldIndex)
                {
                    currentIndex = i;
                    break;
                }
            }

            var nextIndex = currentIndex + 1;
            if (nextIndex >= matches.Count)
                nextIndex = 0;

            var next = matches[nextIndex];

            _friendlySearchCollectionIndex = next.CollectionIndex;
            _friendlySearchEntryIndex = next.EntryIndex;
            _friendlySearchFieldIndex = next.FieldIndex;
            _friendlySearchFieldName = next.FieldName;

            _pendingFriendlySearchQuery = query;
            _pendingFriendlySearchCaseSensitive = caseSensitive;
            _pendingFriendlySearchFieldName = next.FieldName;

            NavigateFriendlyTo(next.CollectionIndex, next.EntryIndex, next.FieldIndex, next.FieldName, query, caseSensitive);

            if (FriendlyFields is not null && FriendlyFields.Count > 0)
                QueueRebuildFieldsForSelectedEntry();
        }

        private void NavigateFriendlyTo(int collectionIndex, int entryIndex, int fieldIndex, string? fieldName, string query, bool caseSensitive)
        {
            if (_friendlyDocument is null)
                return;

            if (collectionIndex < 0 || collectionIndex >= _friendlyDocument.Collections.Count)
                return;

            var col = FriendlyCollections.ElementAtOrDefault(collectionIndex);
            if (col is null)
                return;

            var entryVm = col.Entries.ElementAtOrDefault(entryIndex);
            if (entryVm is null)
                return;

            _friendlySearchCollectionIndex = collectionIndex;
            _friendlySearchEntryIndex = entryIndex;
            _friendlySearchFieldIndex = fieldIndex;
            _friendlySearchFieldName = fieldName;

            _pendingFriendlySearchQuery = query;
            _pendingFriendlySearchCaseSensitive = caseSensitive;
            _pendingFriendlySearchFieldName = fieldName;

            var sameCollection = ReferenceEquals(col, SelectedFriendlyCollection);
            if (!sameCollection)
                SelectedFriendlyCollection = col;

            if (ReferenceEquals(entryVm, SelectedFriendlyEntry))
            {
                QueueRebuildFieldsForSelectedEntry();
                return;
            }

            SelectedFriendlyEntry = entryVm;
        }
        public void DuplicateSelectedFriendlyEntry()
        {
            if (!IsFriendlyView)
                return;

            if (_friendlyDocument is null)
                return;

            var sourceEntry = SelectedFriendlyEntry?.Entry;
            if (sourceEntry is null)
                return;

            var filePath = GetSelectedFilePath();

            var sourceOccurrence = 0;
            var collection = SelectedFriendlyCollection?.Collection;
            if (collection is not null)
            {
                var matchesBefore = 0;

                foreach (var e2 in collection.Entries)
                {
                    if (!string.Equals(e2.Key, sourceEntry.Key, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (ReferenceEquals(e2, sourceEntry))
                    {
                        sourceOccurrence = matchesBefore;
                        break;
                    }

                    matchesBefore++;
                }
            }

            if (!_friendly.TryDuplicateEntry(_friendlyDocument, sourceEntry, insertAfter: true, out _, out var error))
            {
                if (!string.IsNullOrWhiteSpace(error))
                    MessageBox.Show(error, "Duplicate failed", MessageBoxButton.OK, MessageBoxImage.Error);

                Status = "Duplicate failed.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            _editHistory.AddPendingDuplicateEntry(filePath, SelectedFriendlyCollection?.Title, sourceEntry.Key, sourceOccurrence, sourceEntry.Display);
            XmlText = _friendly.ToXml(_friendlyDocument);
            RefreshFriendlyFromXml();
            QueueRebuildFieldsForSelectedEntry();

            Status = "Duplicated entry.";
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
        public void DeleteSelectedFriendlyEntry()
        {
            if (!IsFriendlyView)
                return;

            if (_friendlyDocument is null)
                return;

            var entry = SelectedFriendlyEntry?.Entry;
            if (entry is null)
                return;

            var filePath = GetSelectedFilePath();

            var result = MessageBox.Show(
                $"Delete entry '{entry.Display}'?\n\nYes: Backup file then delete\nNo: Delete without backup",
                "Delete Entry",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
                return;

            if (result == MessageBoxResult.Yes)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    MessageBox.Show("No file is selected to back up.", "Delete Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!_backupRequest.TryBackup(filePath, out var backupErr))
                {
                    MessageBox.Show(backupErr ?? "Backup failed.", "Delete Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (result != MessageBoxResult.Yes && result != MessageBoxResult.No)
                return;

            var entryKeyBeforeDelete = entry.Key;

            var occurrenceBeforeDelete = 0;
            var collectionBeforeDelete = SelectedFriendlyCollection?.Collection;
            if (collectionBeforeDelete is not null)
            {
                var matchesBefore = 0;

                foreach (var e2 in collectionBeforeDelete.Entries)
                {
                    if (!string.Equals(e2.Key, entryKeyBeforeDelete, StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (ReferenceEquals(e2, entry))
                    {
                        occurrenceBeforeDelete = matchesBefore;
                        break;
                    }

                    matchesBefore++;
                }
            }

            if (!_friendly.TryDeleteEntry(_friendlyDocument, entry, out var err))
            {
                MessageBox.Show(err ?? "Delete failed.", "Delete Entry", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Delete failed.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(filePath))
                _editHistory.AddPendingDeleteEntry(filePath, SelectedFriendlyCollection?.Title, entryKeyBeforeDelete, occurrenceBeforeDelete, entry.Display);
                XmlText = _friendly.ToXml(_friendlyDocument);
                RefreshFriendlyFromXml();
                QueueRebuildFieldsForSelectedEntry();

            Status = "Deleted entry.";
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        public void DuplicateSelectedFriendlyLookupItem()
        {
            if (!IsFriendlyView)
                return;

            if (_friendlyDocument is null)
                return;

            var sourceEntry = SelectedFriendlyEntry?.Entry;
            if (sourceEntry is null)
                return;

            var selectedItem = SelectedFriendlyLookupItem;
            if (selectedItem is null)
                return;

            static bool TryParseIndexedItem(string itemName, out string elementName, out int index)
            {
                elementName = "";
                index = 0;

                if (string.IsNullOrWhiteSpace(itemName))
                    return false;

                var lb = itemName.LastIndexOf('[');
                if (lb <= 0 || !itemName.EndsWith("]", StringComparison.Ordinal))
                    return false;

                elementName = itemName.Substring(0, lb);
                var indexText = itemName.Substring(lb + 1, itemName.Length - lb - 2);

                return int.TryParse(indexText, out index) && index > 0;
            }

            if (!LookupFieldPathParser.TryParseLookupField(selectedItem.FullName, out var groupTitle, out var itemName, out _))
            {
                Status = "Duplicate item failed.";
                return;
            }

            if (!TryParseIndexedItem(itemName, out var elementName, out var index))
            {
                Status = "Duplicate item failed.";
                return;
            }

            _pendingLookupGroupTitle = groupTitle;
            _pendingLookupItemName = $"{elementName}[{index + 1}]";
            _pendingLookupField = selectedItem.Field;

            if (!_friendly.TryDuplicateChildBlock(_friendlyDocument, sourceEntry, groupTitle, itemName, insertAfter: true, out var error))
            {
                _pendingLookupGroupTitle = null;
                _pendingLookupItemName = null;
                _pendingLookupField = null;

                if (!string.IsNullOrWhiteSpace(error))
                    MessageBox.Show(error, "Duplicate failed", MessageBoxButton.OK, MessageBoxImage.Error);

                Status = "Duplicate failed.";
                return;
            }

            var filePath = GetSelectedFilePath();
            if (!string.IsNullOrWhiteSpace(filePath))
            {
                _editHistory.AddPendingDuplicateChildBlock(filePath, SelectedFriendlyCollection?.Title, sourceEntry.Key, sourceEntry.Occurrence, selectedItem.FullName, sourceEntry.Display);
            }

            XmlText = _friendly.ToXml(_friendlyDocument);
            Status = "Duplicated item.";
            QueueRebuildFieldsForSelectedEntry();
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }

        public void DeleteSelectedFriendlyLookupItem()
        {
            if (!IsFriendlyView)
                return;

            if (_friendlyDocument is null)
                return;

            var sourceEntry = SelectedFriendlyEntry?.Entry;
            if (sourceEntry is null)
                return;

            var selectedItem = SelectedFriendlyLookupItem;
            if (selectedItem is null)
                return;

            if (!LookupFieldPathParser.TryParseLookupField(selectedItem.FullName, out var groupTitle, out var itemName, out _))
            {
                Status = "Delete item failed.";
                return;
            }

            var result = MessageBox.Show(
                $"Delete item '{itemName}'?\n\nYes: Backup file then delete\nNo: Delete without backup",
                "Delete Item",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Cancel)
                return;

            var filePath = GetSelectedFilePath();

            if (result == MessageBoxResult.Yes)
            {
                if (string.IsNullOrWhiteSpace(filePath))
                {
                    MessageBox.Show("No file is selected to back up.", "Delete Item", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                if (!_backupRequest.TryBackup(filePath, out var backupErr))
                {
                    MessageBox.Show(backupErr ?? "Backup failed.", "Delete Item", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            if (!_friendly.TryDeleteChildBlock(_friendlyDocument, sourceEntry, groupTitle, itemName, out var error))
            {
                if (!string.IsNullOrWhiteSpace(error))
                    MessageBox.Show(error, "Delete failed", MessageBoxButton.OK, MessageBoxImage.Error);

                Status = "Delete item failed.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(filePath))
            {
                _editHistory.AddPendingDeleteChildBlock(filePath, SelectedFriendlyCollection?.Title, sourceEntry.Key, sourceEntry.Occurrence, selectedItem.FullName, sourceEntry.Display);
            }

            XmlText = _friendly.ToXml(_friendlyDocument);
            Status = "Deleted item.";
            QueueRebuildFieldsForSelectedEntry();
            System.Windows.Input.CommandManager.InvalidateRequerySuggested();
        }
        private string? GetSelectedFilePath()
        {
            if (SelectedXmlFile is not null)
            {
                _currentFilePath = SelectedXmlFile.FullPath;
                return SelectedXmlFile.FullPath;
            }

            if (SelectedTreeNode is not null && SelectedTreeNode.IsFile && SelectedTreeNode.FullPath is not null)
            {
                _currentFilePath = SelectedTreeNode.FullPath;
                return SelectedTreeNode.FullPath;
            }

            return _currentFilePath;
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
            _editHistory.CommitForFile(path);

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
            _editHistory.CommitForFile(dlg.FileName);

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
            Title = $"LSR XML Helper - {_rootFolder}  |  Created by Y0sh1M1tsu";
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
            _currentFilePath = path;
            TryRaiseRawNavigationForLoadedFile(path);
        }

        private void TryRaiseRawNavigationForLoadedFile(string loadedPath)
        {
            if (_pendingRawNavigation is null)
                return;

            if (!string.Equals(_pendingRawNavigation.FilePath, loadedPath, StringComparison.OrdinalIgnoreCase))
                return;

            var req = _pendingRawNavigation;
            _pendingRawNavigation = null;

            RawNavigationRequested?.Invoke(this, req);
        }

        private void TryApplyGlobalFriendlyNavigateForLoadedFile(string loadedPath)
        {
            if (!string.Equals(_currentFilePath, loadedPath, StringComparison.OrdinalIgnoreCase))
                return;

            if (_friendlyDocument is null)
                return;

            RefreshFriendlyFromXml();
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
            _globalSearchUseParallelProcessing = _settings.GlobalSearchUseParallelProcessing;
            _isDarkMode = _settings.IsDarkMode;
            _isFriendlyView = _settings.IsFriendlyView;

            if (Enum.TryParse<GlobalSearchScope>(_settings.GlobalSearchScope ?? "", ignoreCase: true, out var scope))
                _globalSearchScope = scope;
            else
                _globalSearchScope = GlobalSearchScope.Both;
        }

        private void SaveSettings()
        {
            try { _settingsService.Save(_settings); }
            catch { }
        }
    }
}