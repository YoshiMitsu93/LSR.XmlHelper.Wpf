using LSR.XmlHelper.Core.Models;
using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class GlobalSearchWindowViewModel : ObservableObject
    {
        private readonly XmlFileDiscoveryService _discovery;
        private readonly XmlGlobalSearchService _search;
        private readonly XmlGlobalFriendlySearchService _friendlySearch;
        private readonly Func<string?> _getRootFolder;
        private readonly Func<bool> _getIncludeSubfolders;
        private readonly Action<GlobalSearchHit, string, bool> _openRawHit;
        private readonly Action<bool> _setIncludeSubfolders;
        private readonly Func<bool> _getUseParallelProcessing;
        private readonly Action<bool> _setUseParallelProcessing;
        private readonly Action<GlobalFriendlySearchHit, string, bool> _openFriendlyHit;
        private readonly Action<GlobalSearchScope> _onScopeChanged;

        private CancellationTokenSource? _cts;

        private string _query = "";
        private bool _caseSensitive;
        private bool _isSearching;
        private string _status = "Ready.";
        private GlobalSearchHit? _selectedHit;
        private GlobalFriendlySearchHit? _selectedFriendlyHit;
        private int _selectedTabIndex;
        private GlobalSearchScope _searchScope;
        private bool _includeSubfolders;
        private bool _useParallelProcessing;
        private int _filesProcessed;
        private int _totalWork;
        private string _currentSearchFile = string.Empty;
        private long _lastProgressUiUpdateMs;

        public GlobalSearchWindowViewModel(
    XmlFileDiscoveryService discovery,
    XmlGlobalSearchService search,
    Func<string?> getRootFolder,
    Func<bool> getIncludeSubfolders,
    Action<bool> setIncludeSubfolders,
    Func<bool> getUseParallelProcessing,
    Action<bool> setUseParallelProcessing,
    Action<GlobalSearchHit, string, bool> openRawHit,
    Action<GlobalFriendlySearchHit, string, bool> openFriendlyHit,
    int initialTabIndex,
    GlobalSearchScope initialScope,
    Action<GlobalSearchScope> onScopeChanged)
        {
            _discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
            _search = search ?? throw new ArgumentNullException(nameof(search));
            _friendlySearch = new XmlGlobalFriendlySearchService();
            _getRootFolder = getRootFolder ?? throw new ArgumentNullException(nameof(getRootFolder));
            _getIncludeSubfolders = getIncludeSubfolders ?? throw new ArgumentNullException(nameof(getIncludeSubfolders));
            _setIncludeSubfolders = setIncludeSubfolders ?? throw new ArgumentNullException(nameof(setIncludeSubfolders));
            _getUseParallelProcessing = getUseParallelProcessing ?? throw new ArgumentNullException(nameof(getUseParallelProcessing));
            _setUseParallelProcessing = setUseParallelProcessing ?? throw new ArgumentNullException(nameof(setUseParallelProcessing));
            _openRawHit = openRawHit ?? throw new ArgumentNullException(nameof(openRawHit));
            _openFriendlyHit = openFriendlyHit ?? throw new ArgumentNullException(nameof(openFriendlyHit));
            _onScopeChanged = onScopeChanged ?? throw new ArgumentNullException(nameof(onScopeChanged));

            Hits = new ObservableCollection<GlobalSearchHit>();
            FriendlyHits = new ObservableCollection<GlobalFriendlySearchHit>();

            _selectedTabIndex = initialTabIndex;
            _searchScope = initialScope;

            _includeSubfolders = _getIncludeSubfolders();
            _useParallelProcessing = _getUseParallelProcessing();

            SearchCommand = new RelayCommand(async () => await SearchAsync(), () => !IsSearching);
            CancelCommand = new RelayCommand(CancelSearch, () => IsSearching);
            OpenSelectedCommand = new RelayCommand(OpenSelected, CanOpenSelected);
        }



        public ObservableCollection<GlobalSearchHit> Hits { get; }
        public ObservableCollection<GlobalFriendlySearchHit> FriendlyHits { get; }

        public RelayCommand SearchCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand OpenSelectedCommand { get; }
        public string RootFolderForDisplay => _getRootFolder() ?? "";

        public GlobalSearchScope SearchScope
        {
            get => _searchScope;
            set
            {
                if (!SetProperty(ref _searchScope, value))
                    return;

                _onScopeChanged(value);

                if (value == GlobalSearchScope.RawXml)
                    SelectedTabIndex = 0;
                else if (value == GlobalSearchScope.FriendlyView)
                    SelectedTabIndex = 1;

                OnPropertyChanged(nameof(ScopeHint));
                OnPropertyChanged(nameof(IsParallelProcessingToggleEnabled));
            }
        }

        public string ScopeHint
        {
            get
            {
                var include = IncludeSubfolders ? "On" : "Off";
                var parallel = UseParallelProcessing ? "On" : "Off";

                return SearchScope switch
                {
                    GlobalSearchScope.RawXml => $"Search mode: Raw XML - Scans XML text only. (Fast search) | Include subfolders: {include}",
                    GlobalSearchScope.FriendlyView => $"Search mode: Friendly view - Parses XML into Friendly View fields. (Slow search) | Include subfolders: {include} | Parallel: {parallel}",
                    _ => $"Search mode: Both - Searches in both Raw XML & Friendly view. (Slow search) | Include subfolders: {include} | Parallel: {parallel}"
                };
            }
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set
            {
                if (!SetProperty(ref _selectedTabIndex, value))
                    return;

                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged(nameof(RawTabHeader));
                OnPropertyChanged(nameof(FriendlyTabHeader));
            }
        }

        public string RawTabHeader => $"Raw XML ({Hits.Count})";
        public string FriendlyTabHeader => $"Friendly View ({FriendlyHits.Count})";

        public GlobalSearchHit? SelectedHit
        {
            get => _selectedHit;
            set
            {
                if (!SetProperty(ref _selectedHit, value))
                    return;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public GlobalFriendlySearchHit? SelectedFriendlyHit
        {
            get => _selectedFriendlyHit;
            set
            {
                if (!SetProperty(ref _selectedFriendlyHit, value))
                    return;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Query
        {
            get => _query;
            set => SetProperty(ref _query, value);
        }

        public bool CaseSensitive
        {
            get => _caseSensitive;
            set => SetProperty(ref _caseSensitive, value);
        }

        public bool IncludeSubfolders
        {
            get => _includeSubfolders;
            set
            {
                if (!SetProperty(ref _includeSubfolders, value))
                    return;

                _setIncludeSubfolders(value);
                OnPropertyChanged(nameof(ScopeHint));
            }
        }

        public bool UseParallelProcessing
        {
            get => _useParallelProcessing;
            set
            {
                if (!SetProperty(ref _useParallelProcessing, value))
                    return;

                _setUseParallelProcessing(value);
                OnPropertyChanged(nameof(ScopeHint));
                OnPropertyChanged(nameof(SearchProgressText));
            }
        }

        public bool IsSearchOptionsEnabled => !IsSearching;

        public bool IsParallelProcessingToggleEnabled => !IsSearching && SearchScope != GlobalSearchScope.RawXml;

        public bool IsSearching
        {
            get => _isSearching;
            private set
            {
                if (!SetProperty(ref _isSearching, value))
                    return;

                CommandManager.InvalidateRequerySuggested();
                OnPropertyChanged(nameof(IsSearchProgressIndeterminate));
                OnPropertyChanged(nameof(SearchProgressPercent));
                OnPropertyChanged(nameof(SearchProgressText));
                OnPropertyChanged(nameof(IsSearchOptionsEnabled));
                OnPropertyChanged(nameof(IsParallelProcessingToggleEnabled));
            }
        }

        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }
        public bool IsSearchProgressIndeterminate => IsSearching && _totalWork <= 0;

        public double SearchProgressPercent
        {
            get
            {
                if (_totalWork <= 0)
                    return 0;

                var value = (double)_filesProcessed / _totalWork * 100d;
                return value > 100d ? 100d : value;
            }
        }

        public string SearchProgressText
        {
            get
            {
                if (_totalWork <= 0)
                    return string.Empty;

                var baseText = $"{(int)SearchProgressPercent}% ({_filesProcessed}/{_totalWork})";
                var fileName = string.IsNullOrWhiteSpace(_currentSearchFile) ? "" : System.IO.Path.GetFileName(_currentSearchFile);
                if (string.IsNullOrWhiteSpace(fileName))
                    return baseText;

                if (UseParallelProcessing && SearchScope != GlobalSearchScope.RawXml && IsSearching)
                {
                    var workers = Math.Max(1, Environment.ProcessorCount / 2);
                    return $"{baseText}  •  Workers: {workers}  •  Last started: {fileName}";
                }

                return $"{baseText}  •  {fileName}";
            }
        }

        private void ResetSearchProgress(int totalWork)
        {
            _totalWork = totalWork;
            _filesProcessed = 0;
            _currentSearchFile = string.Empty;
            _lastProgressUiUpdateMs = 0;
            OnPropertyChanged(nameof(IsSearchProgressIndeterminate));
            OnPropertyChanged(nameof(SearchProgressPercent));
            OnPropertyChanged(nameof(SearchProgressText));
        }

        private void AddToSearchProgress(int delta)
        {
            if (_totalWork <= 0)
                return;

            var next = _filesProcessed + delta;
            _filesProcessed = next > _totalWork ? _totalWork : next;

            var now = Environment.TickCount64;
            var shouldUpdate = _filesProcessed == _totalWork || _lastProgressUiUpdateMs == 0 || now - _lastProgressUiUpdateMs >= 100;

            if (shouldUpdate)
            {
                _lastProgressUiUpdateMs = now;
                OnPropertyChanged(nameof(SearchProgressPercent));
                OnPropertyChanged(nameof(SearchProgressText));
            }
        }

        private void SetCurrentSearchFile(string path)
        {
            if (string.Equals(_currentSearchFile, path, StringComparison.Ordinal))
                return;

            _currentSearchFile = path ?? string.Empty;

            var now = Environment.TickCount64;
            if (_lastProgressUiUpdateMs == 0 || now - _lastProgressUiUpdateMs >= 100)
            {
                _lastProgressUiUpdateMs = now;
                OnPropertyChanged(nameof(SearchProgressText));
            }
        }

        private async Task SearchAsync()
        {
            var root = _getRootFolder();
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
            {
                Status = "Pick a folder first.";
                return;
            }

            if (string.IsNullOrWhiteSpace(Query))
            {
                Status = "Type a search term.";
                return;
            }

            CancelSearch();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            IsSearching = true;
            Hits.Clear();
            FriendlyHits.Clear();
            OnPropertyChanged(nameof(RawTabHeader));
            OnPropertyChanged(nameof(FriendlyTabHeader));

            try
            {
                var isFriendly = SearchScope == GlobalSearchScope.FriendlyView || SearchScope == GlobalSearchScope.Both;
                Status = isFriendly
                    ? "Searching... (Friendly View parses XML and can take a while on large files)"
                    : "Searching...";

                var includeSubfolders = IncludeSubfolders;
                var paths = _discovery.GetXmlFiles(root, includeSubfolders).ToList();

                var doRaw = SearchScope == GlobalSearchScope.RawXml || SearchScope == GlobalSearchScope.Both;
                var doFriendly = SearchScope == GlobalSearchScope.FriendlyView || SearchScope == GlobalSearchScope.Both;
                var taskCount = (doRaw ? 1 : 0) + (doFriendly ? 1 : 0);
                var totalWork = paths.Count * taskCount;

                ResetSearchProgress(totalWork);

                var fileCountProgress = new Progress<int>(AddToSearchProgress);
                var currentFileProgress = new Progress<string>(SetCurrentSearchFile);

                Task<IReadOnlyList<GlobalSearchHit>>? rawTask = null;
                Task<IReadOnlyList<GlobalFriendlySearchHit>>? friendlyTask = null;

                if (doRaw)
                    rawTask = _search.SearchAsync(paths, Query, CaseSensitive, maxResults: 5000, token, fileCountProgress, currentFileProgress);

                if (doFriendly)
                    friendlyTask = _friendlySearch.SearchAsync(paths, Query, CaseSensitive, maxResults: 5000, token, fileCountProgress, currentFileProgress, UseParallelProcessing);

                if (rawTask is not null)
                {
                    var raw = await rawTask;
                    foreach (var hit in raw)
                        Hits.Add(hit);
                }

                if (friendlyTask is not null)
                {
                    var friendly = await friendlyTask;
                    foreach (var hit in friendly)
                        FriendlyHits.Add(hit);
                }

                if (Hits.Count > 0)
                    SelectedHit = Hits[0];

                if (FriendlyHits.Count > 0)
                    SelectedFriendlyHit = FriendlyHits[0];

                if (_totalWork > 0)
                {
                    _filesProcessed = _totalWork;
                    OnPropertyChanged(nameof(SearchProgressPercent));
                    OnPropertyChanged(nameof(SearchProgressText));
                }

                Status = Hits.Count == 0 && FriendlyHits.Count == 0 ? "No results." : "Done.";
            }
            catch (OperationCanceledException)
            {
                Status = "Canceled.";
            }
            catch
            {
                Status = "Search failed.";
            }
            finally
            {
                IsSearching = false;
            }
        }

        private void CancelSearch()
        {
            try
            {
                _cts?.Cancel();
                _cts?.Dispose();
            }
            catch
            {
            }
            finally
            {
                _cts = null;
            }
        }

        private bool CanOpenSelected()
        {
            if (SelectedTabIndex == 0)
                return SelectedHit is not null;

            return SelectedFriendlyHit is not null;
        }

        private void OpenSelected()
        {
            if (SelectedTabIndex == 0)
            {
                if (SelectedHit is null)
                    return;

                _openRawHit(SelectedHit, Query, CaseSensitive);
                return;
            }

            if (SelectedFriendlyHit is null)
                return;

            _openFriendlyHit(SelectedFriendlyHit, Query, CaseSensitive);
        }
    }
}
