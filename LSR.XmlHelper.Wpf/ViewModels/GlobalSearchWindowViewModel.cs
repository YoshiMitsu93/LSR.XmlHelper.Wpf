using LSR.XmlHelper.Core.Models;
using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class GlobalSearchWindowViewModel : ObservableObject
    {
        private readonly XmlFileDiscoveryService _discovery;
        private readonly XmlGlobalSearchService _search;
        private readonly Func<string?> _getRootFolder;
        private readonly Func<bool> _getIncludeSubfolders;
        private readonly Action<GlobalSearchHit> _openHit;

        private CancellationTokenSource? _cts;

        private string _query = "";
        private bool _caseSensitive;
        private bool _isSearching;
        private string _status = "Ready.";
        private GlobalSearchHit? _selectedHit;

        public GlobalSearchWindowViewModel(
            XmlFileDiscoveryService discovery,
            XmlGlobalSearchService search,
            Func<string?> getRootFolder,
            Func<bool> getIncludeSubfolders,
            Action<GlobalSearchHit> openHit)
        {
            _discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
            _search = search ?? throw new ArgumentNullException(nameof(search));
            _getRootFolder = getRootFolder ?? throw new ArgumentNullException(nameof(getRootFolder));
            _getIncludeSubfolders = getIncludeSubfolders ?? throw new ArgumentNullException(nameof(getIncludeSubfolders));
            _openHit = openHit ?? throw new ArgumentNullException(nameof(openHit));

            Hits = new ObservableCollection<GlobalSearchHit>();

            SearchCommand = new RelayCommand(StartSearch, () => !IsSearching);
            CancelCommand = new RelayCommand(CancelSearch, () => IsSearching);
            OpenSelectedCommand = new RelayCommand(OpenSelected, () => SelectedHit is not null);
        }

        public RelayCommand SearchCommand { get; }
        public RelayCommand CancelCommand { get; }
        public RelayCommand OpenSelectedCommand { get; }

        public ObservableCollection<GlobalSearchHit> Hits { get; }

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

        public bool IsSearching
        {
            get => _isSearching;
            private set
            {
                if (!SetProperty(ref _isSearching, value))
                    return;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string Status
        {
            get => _status;
            private set => SetProperty(ref _status, value);
        }

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

        private void StartSearch()
        {
            _ = SearchAsync();
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
            Status = "Scanning...";

            Hits.Clear();
            SelectedHit = null;

            IReadOnlyList<string> paths;
            try
            {
                paths = _discovery.GetXmlFiles(root, _getIncludeSubfolders());
            }
            catch (Exception ex)
            {
                IsSearching = false;
                Status = "Scan failed.";
                System.Windows.MessageBox.Show(ex.Message, "Folder scan failed", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var results = await _search.SearchAsync(paths, Query, CaseSensitive, maxResults: 2000, token);

                foreach (var hit in results)
                    Hits.Add(hit);

                Status = $"Found {Hits.Count} match(es) in {paths.Count} file(s).";
            }
            catch (OperationCanceledException)
            {
                Status = "Canceled.";
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
            }
            catch
            {
            }
        }

        private void OpenSelected()
        {
            if (SelectedHit is null)
                return;

            _openHit(SelectedHit);
        }
    }
}
