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

        private CancellationTokenSource? _loadCts;

        private string _title = "LSR XML Helper";
        private string _status = "Ready.";
        private string _xmlText = "";
        private string? _rootFolder;

        private XmlFileListItem? _selectedXmlFile;
        private XmlExplorerNode? _selectedTreeNode;

        private XmlListViewMode _viewMode = XmlListViewMode.Folders;
        private bool _includeSubfolders = true;

        public MainWindowViewModel()
        {
            _xml = new XmlDocumentService();
            _discovery = new XmlFileDiscoveryService();
            _loader = new XmlFileLoaderService();
            _saver = new XmlFileSaveService();

            XmlFiles = new ObservableCollection<XmlFileListItem>();
            XmlTree = new ObservableCollection<XmlExplorerNode>();

            OpenCommand = new RelayCommand(OpenFolder);
            FormatCommand = new RelayCommand(Format, () => !string.IsNullOrWhiteSpace(XmlText));
            ValidateCommand = new RelayCommand(Validate, () => !string.IsNullOrWhiteSpace(XmlText));
            SaveCommand = new RelayCommand(Save, () => GetSelectedFilePath() is not null);
            SaveAsCommand = new RelayCommand(SaveAs);
            ClearCommand = new RelayCommand(Clear);
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
            set => SetProperty(ref _xmlText, value);
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

                RefreshFileViews(false);
            }
        }

        public bool IncludeSubfolders
        {
            get => _includeSubfolders;
            set
            {
                if (!SetProperty(ref _includeSubfolders, value))
                    return;

                RefreshFileViews(false);
            }
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
            }
        }

        public XmlExplorerNode? SelectedTreeNode
        {
            get => _selectedTreeNode;
            set
            {
                if (!SetProperty(ref _selectedTreeNode, value))
                    return;

                if (value?.IsFile == true && value.FullPath is not null)
                {
                    SelectedXmlFile = null;
                    _ = LoadFileAsync(value.FullPath);
                }
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
            using var dialog = new WinForms.FolderBrowserDialog
            {
                Description = "Pick the folder that contains your XML files",
                ShowNewFolderButton = false
            };

            if (dialog.ShowDialog() != WinForms.DialogResult.OK)
                return;

            _rootFolder = dialog.SelectedPath;
            Title = $"LSR XML Helper - {_rootFolder}";
            RefreshFileViews(true);
        }

        private void RefreshFileViews(bool resetEditor)
        {
            CancelPendingLoad();

            XmlFiles.Clear();
            XmlTree.Clear();

            if (resetEditor)
            {
                XmlText = "";
                SelectedXmlFile = null;
                SelectedTreeNode = null;
            }

            if (string.IsNullOrWhiteSpace(_rootFolder))
                return;

            var include = ViewMode == XmlListViewMode.Folders || IncludeSubfolders;
            var paths = _discovery.GetXmlFiles(_rootFolder, include);

            foreach (var p in paths)
                XmlFiles.Add(new XmlFileListItem(p, Path.GetRelativePath(_rootFolder, p)));

            BuildTree(paths);

            Status = $"Found {paths.Count} XML file(s).";
        }

        private void BuildTree(IReadOnlyList<string> paths)
        {
            var map = new Dictionary<string, XmlFolderNode>(StringComparer.OrdinalIgnoreCase);

            foreach (var path in paths)
            {
                var rel = Path.GetRelativePath(_rootFolder!, path);
                var parts = rel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

                if (parts.Length == 1)
                {
                    XmlTree.Add(new XmlFileNode(parts[0], path));
                    continue;
                }

                XmlFolderNode? current = null;
                var key = "";

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    key = key.Length == 0 ? parts[i] : $"{key}\\{parts[i]}";

                    if (!map.TryGetValue(key, out var folder))
                    {
                        folder = new XmlFolderNode(parts[i]);
                        map[key] = folder;

                        if (current == null)
                            XmlTree.Add(folder);
                        else
                            current.Children.Add(folder);
                    }

                    current = folder;
                }

                current!.Children.Add(new XmlFileNode(parts[^1], path));
            }
        }

        private async Task LoadFileAsync(string path)
        {
            CancelPendingLoad();
            _loadCts = new CancellationTokenSource();

            Status = $"Loading: {Path.GetFileName(path)}";

            var result = await _loader.LoadAsync(path, _loadCts.Token);

            if (!result.Success)
            {
                Status = "Open failed.";
                if (result.Error != null)
                {
                    MessageBox.Show(result.Error, "Open failed",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                return;
            }

            XmlText = result.Text!;
            Status = $"Opened: {Path.GetFileName(path)}";
        }

        private void CancelPendingLoad()
        {
            _loadCts?.Cancel();
            _loadCts?.Dispose();
            _loadCts = null;
        }

        private string? GetSelectedFilePath()
        {
            return SelectedXmlFile?.FullPath
                ?? (SelectedTreeNode?.IsFile == true ? SelectedTreeNode.FullPath : null);
        }

        private void Format()
        {
            XmlText = _xml.Format(XmlText);
            Status = "Formatted.";
        }

        private void Validate()
        {
            var (ok, msg) = _xml.ValidateWellFormed(XmlText);
            MessageBox.Show(msg, "Validate",
                MessageBoxButton.OK,
                ok ? MessageBoxImage.Information : MessageBoxImage.Error);
        }

        private void Save()
        {
            var path = GetSelectedFilePath();
            if (path == null)
                return;

            var result = _saver.Save(path, XmlText);
            if (!result.Success)
            {
                MessageBox.Show(result.Error!, "Save failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Status = $"Saved: {Path.GetFileName(path)}";
        }

        private void SaveAs()
        {
            var dlg = new WpfSaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                FileName = "document.xml"
            };

            if (dlg.ShowDialog() != true)
                return;

            var result = _saver.Save(dlg.FileName, XmlText);
            if (!result.Success)
            {
                MessageBox.Show(result.Error!, "Save As failed",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Clear()
        {
            CancelPendingLoad();
            XmlText = "";
            SelectedXmlFile = null;
            SelectedTreeNode = null;
            Status = "Cleared.";
        }
    }
}
