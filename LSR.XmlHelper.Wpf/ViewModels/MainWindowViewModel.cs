using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;

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

        private string _title = "LSR XML Helper";
        private string _status = "Ready.";
        private string _xmlText = "";
        private string? _rootFolder;
        private XmlFileListItem? _selectedXmlFile;

        public MainWindowViewModel()
        {
            _xml = new XmlDocumentService();
            _discovery = new XmlFileDiscoveryService();

            XmlFiles = new ObservableCollection<XmlFileListItem>();

            OpenCommand = new RelayCommand(OpenFolder);
            FormatCommand = new RelayCommand(Format, () => !string.IsNullOrWhiteSpace(XmlText));
            ValidateCommand = new RelayCommand(Validate, () => !string.IsNullOrWhiteSpace(XmlText));
            SaveCommand = new RelayCommand(Save, () => SelectedXmlFile is not null && !string.IsNullOrWhiteSpace(XmlText));
            SaveAsCommand = new RelayCommand(SaveAs, () => !string.IsNullOrWhiteSpace(XmlText));
            ClearCommand = new RelayCommand(Clear, () => SelectedXmlFile is not null || !string.IsNullOrWhiteSpace(XmlText));
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
                    System.Windows.Input.CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public ObservableCollection<XmlFileListItem> XmlFiles { get; }

        public XmlFileListItem? SelectedXmlFile
        {
            get => _selectedXmlFile;
            set
            {
                if (!SetProperty(ref _selectedXmlFile, value))
                    return;

                LoadSelectedFile();
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

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
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

            RefreshFileList(includeSubfolders: true);

            Title = string.IsNullOrWhiteSpace(_rootFolder) ? "LSR XML Helper" : $"LSR XML Helper - {_rootFolder}";
            Status = XmlFiles.Count == 0 ? "No XML files found." : $"Found {XmlFiles.Count} XML file(s).";

            if (XmlFiles.Count > 0)
                SelectedXmlFile = XmlFiles[0];
        }

        private void RefreshFileList(bool includeSubfolders)
        {
            XmlFiles.Clear();

            if (string.IsNullOrWhiteSpace(_rootFolder))
                return;

            IReadOnlyList<string> paths;

            try
            {
                paths = _discovery.GetXmlFiles(_rootFolder, includeSubfolders);
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
        }

        private void LoadSelectedFile()
        {
            if (SelectedXmlFile is null)
            {
                XmlText = "";
                return;
            }

            try
            {
                XmlText = _xml.LoadFromFile(SelectedXmlFile.FullPath);
                Status = $"Opened: {Path.GetFileName(SelectedXmlFile.FullPath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Open failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Open failed.";
            }
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
            if (SelectedXmlFile is null)
            {
                SaveAs();
                return;
            }

            try
            {
                _xml.SaveToFile(SelectedXmlFile.FullPath, XmlText);
                Status = $"Saved: {Path.GetFileName(SelectedXmlFile.FullPath)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Save failed.";
            }
        }

        private void SaveAs()
        {
            var dlg = new WpfSaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Save XML As",
                FileName = SelectedXmlFile is null ? "document.xml" : Path.GetFileName(SelectedXmlFile.FullPath),
                InitialDirectory = string.IsNullOrWhiteSpace(_rootFolder) ? null : _rootFolder
            };

            if (dlg.ShowDialog() != true)
                return;

            try
            {
                _xml.SaveToFile(dlg.FileName, XmlText);
                Status = $"Saved: {Path.GetFileName(dlg.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Save As failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Status = "Save As failed.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(_rootFolder))
            {
                RefreshFileList(includeSubfolders: true);

                var match = XmlFiles.FirstOrDefault(x => string.Equals(x.FullPath, dlg.FileName, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                    SelectedXmlFile = match;
            }
        }

        private void Clear()
        {
            XmlText = "";
            SelectedXmlFile = null;
            Status = "Cleared.";
        }
    }
}
