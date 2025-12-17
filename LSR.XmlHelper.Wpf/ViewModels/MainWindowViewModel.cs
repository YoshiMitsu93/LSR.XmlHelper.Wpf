using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using WinForms = System.Windows.Forms;
using WpfSaveFileDialog = Microsoft.Win32.SaveFileDialog;
using MessageBox = System.Windows.MessageBox;
using MessageBoxButton = System.Windows.MessageBoxButton;
using MessageBoxImage = System.Windows.MessageBoxImage;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class MainWindowViewModel : ObservableObject
    {
        private readonly XmlDocumentService _xml;
        private readonly XmlFileDiscoveryService _discovery;
        private readonly XmlFileLoaderService _loader;
        private readonly XmlFileSaveService _saver;
        private readonly XmlFriendlyViewService _friendly;

        private CancellationTokenSource? _loadCts;

        private string _title = "LSR XML Helper";
        private string _status = "Ready.";
        private string _xmlText = "";
        private string? _rootFolder;

        private XmlFileListItem? _selectedXmlFile;
        private XmlExplorerNode? _selectedTreeNode;

        private XmlListViewMode _viewMode = XmlListViewMode.Flat;
        private bool _includeSubfolders;

        private bool _isFriendlyView;
        private XmlFriendlyDocument? _friendlyDocument;

        private ObservableCollection<XmlFriendlyCollectionViewModel> _friendlyCollections = new();
        private XmlFriendlyCollectionViewModel? _selectedFriendlyCollection;
        private XmlFriendlyEntryViewModel? _selectedFriendlyEntry;
        private ObservableCollection<XmlFriendlyFieldViewModel> _friendlyFields = new();

        public MainWindowViewModel()
        {
            _xml = new XmlDocumentService();
            _discovery = new XmlFileDiscoveryService();
            _loader = new XmlFileLoaderService();
            _saver = new XmlFileSaveService();
            _friendly = new XmlFriendlyViewService();

            XmlFiles = new ObservableCollection<XmlFileListItem>();
            XmlTree = new ObservableCollection<XmlExplorerNode>();

            OpenCommand = new RelayCommand(OpenFolder);
            FormatCommand = new RelayCommand(Format, () => !string.IsNullOrWhiteSpace(XmlText));
            ValidateCommand = new RelayCommand(Validate, () => !string.IsNullOrWhiteSpace(XmlText));
            SaveCommand = new RelayCommand(Save, () => GetSelectedFilePath() is not null && !string.IsNullOrWhiteSpace(XmlText));
            SaveAsCommand = new RelayCommand(SaveAs, () => !string.IsNullOrWhiteSpace(XmlText));
            ClearCommand = new RelayCommand(Clear, () => GetSelectedFilePath() is not null || !string.IsNullOrWhiteSpace(XmlText));
        }

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

        public string XmlText
        {
            get => _xmlText;
            set
            {
                if (SetProperty(ref _xmlText, value))
                {
                    Status = string.IsNullOrWhiteSpace(_xmlText) ? "Ready." : "Edited.";
                    RefreshFriendlyFromXml();
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
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

        public bool IsFriendlyView
        {
            get => _isFriendlyView;
            set => SetProperty(ref _isFriendlyView, value);
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

        public XmlFileListItem? SelectedXmlFile
        {
            get => _selectedXmlFile;
            set
            {
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

        private void OpenFolder()
        {
            string? picked = null;

            try
            {
                using var dialog = new WinForms.FolderBrowserDialog
                {
                    Description = "Pick the folder that contains your XML files",
                    ShowNewFolderButton = false
                };

                if (dialog.ShowDialog() == WinForms.DialogResult.OK &&
                    !string.IsNullOrWhiteSpace(dialog.SelectedPath))
                {
                    picked = dialog.SelectedPath;
                }
            }
            catch
            {
                picked = null;
            }

            if (string.IsNullOrWhiteSpace(picked))
                return;

            _rootFolder = picked;

            RefreshFileViews(resetEditorAndSelection: true);

            Title = $"LSR XML Helper - {_rootFolder}";
            Status = "Ready.";
        }

        private void RefreshFileViews(bool resetEditorAndSelection)
        {
            var previouslySelectedPath = resetEditorAndSelection ? null : GetSelectedFilePath();

            XmlFiles.Clear();
            XmlTree.Clear();

            if (resetEditorAndSelection)
            {
                CancelPendingLoad();
                SelectedXmlFile = null;
                SelectedTreeNode = null;
                XmlText = "";
            }

            if (string.IsNullOrWhiteSpace(_rootFolder))
                return;

            var include = ViewMode == XmlListViewMode.Folders || IncludeSubfolders;

            IReadOnlyList<string> paths;

            try
            {
                paths = _discovery.GetXmlFiles(_rootFolder, include);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Folder scan failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Folder scan failed.";
                return;
            }

            foreach (var p in paths)
            {
                var display = Path.GetRelativePath(_rootFolder, p);
                XmlFiles.Add(new XmlFileListItem(p, display));
            }

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
                var match = XmlFiles.FirstOrDefault(x => string.Equals(x.FullPath, previouslySelectedPath, StringComparison.OrdinalIgnoreCase));
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

            var name = Path.GetFileName(path);
            Status = $"Loading: {name}";

            var result = await _loader.LoadAsync(path, _loadCts.Token);

            if (!result.Success)
            {
                if (result.Error is not null)
                {
                    MessageBox.Show(result.Error, "Open failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Status = "Open failed.";
                }

                return;
            }

            XmlText = result.Text ?? "";
            Status = $"Opened: {name}";
        }

        private void RefreshFriendlyFromXml()
        {
            _friendlyDocument = _friendly.TryBuild(XmlText);

            if (_friendlyDocument is null)
            {
                FriendlyCollections = new ObservableCollection<XmlFriendlyCollectionViewModel>();
                SelectedFriendlyCollection = null;
                SelectedFriendlyEntry = null;
                FriendlyFields = new ObservableCollection<XmlFriendlyFieldViewModel>();
                return;
            }

            var cols = _friendlyDocument.Collections.Select(c => new XmlFriendlyCollectionViewModel(c)).ToList();
            FriendlyCollections = new ObservableCollection<XmlFriendlyCollectionViewModel>(cols);
            SelectedFriendlyCollection = FriendlyCollections.FirstOrDefault();

            if (FriendlyCollections.Count > 0)
                IsFriendlyView = true;
        }

        private void RebuildFieldsForSelectedEntry()
        {
            var entry = SelectedFriendlyEntry?.Entry;
            if (entry is null)
            {
                FriendlyFields = new ObservableCollection<XmlFriendlyFieldViewModel>();
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

            if (!entry.TrySetField(field.Name, field.Value))
                return;

            if (_friendlyDocument is null)
                return;

            var updatedXml = _friendly.ToXml(_friendlyDocument);
            _xmlText = updatedXml;
            OnPropertyChanged(nameof(XmlText));

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
            var path = GetSelectedFilePath();
            if (path is null)
            {
                SaveAs();
                return;
            }

            try
            {
                _saver.Save(path, XmlText);
                Status = $"Saved: {Path.GetFileName(path)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Save failed.";
            }
        }

        private void SaveAs()
        {
            var current = GetSelectedFilePath();

            var dlg = new WpfSaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Save XML As",
                FileName = current is null ? "document.xml" : Path.GetFileName(current),
                InitialDirectory = string.IsNullOrWhiteSpace(_rootFolder) ? null : _rootFolder
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                _saver.Save(dlg.FileName, XmlText);
                Status = $"Saved: {Path.GetFileName(dlg.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save As failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Save As failed.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(_rootFolder))
                RefreshFileViews(resetEditorAndSelection: false);
        }

        private void Clear()
        {
            CancelPendingLoad();
            XmlText = "";
            SelectedXmlFile = null;
            SelectedTreeNode = null;
            IsFriendlyView = false;
            Status = "Cleared.";
        }
    }
}
