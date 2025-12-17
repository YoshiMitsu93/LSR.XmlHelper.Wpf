using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Infrastructure;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class MainWindowViewModel : ObservableObject
    {
        private readonly XmlDocumentService _xml;

        private string _title = "LSR XML Helper";
        private string _xmlText = "";
        private string _status = "Ready.";
        private string? _currentFilePath;

        public MainWindowViewModel()
        {
            _xml = new XmlDocumentService();

            OpenCommand = new RelayCommand(Open);
            FormatCommand = new RelayCommand(Format, () => !string.IsNullOrWhiteSpace(XmlText));
            ValidateCommand = new RelayCommand(Validate, () => !string.IsNullOrWhiteSpace(XmlText));
            SaveCommand = new RelayCommand(Save, () => !string.IsNullOrWhiteSpace(XmlText));
            SaveAsCommand = new RelayCommand(SaveAs, () => !string.IsNullOrWhiteSpace(XmlText));
            ClearCommand = new RelayCommand(Clear, () => !string.IsNullOrWhiteSpace(XmlText));
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string XmlText
        {
            get => _xmlText;
            set
            {
                if (SetProperty(ref _xmlText, value))
                {
                    Status = string.IsNullOrWhiteSpace(_xmlText) ? "Ready." : "Edited.";
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public string Status
        {
            get => _status;
            set => SetProperty(ref _status, value);
        }

        public RelayCommand OpenCommand { get; }
        public RelayCommand FormatCommand { get; }
        public RelayCommand ValidateCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand ClearCommand { get; }

        private void Open()
        {
            var dlg = new OpenFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Open XML"
            };

            if (dlg.ShowDialog() != true)
                return;

            _currentFilePath = dlg.FileName;
            XmlText = _xml.LoadFromFile(_currentFilePath);
            Status = $"Opened: {Path.GetFileName(_currentFilePath)}";
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
            if (string.IsNullOrWhiteSpace(_currentFilePath))
            {
                SaveAs();
                return;
            }

            _xml.SaveToFile(_currentFilePath, XmlText);
            Status = $"Saved: {Path.GetFileName(_currentFilePath)}";
        }

        private void SaveAs()
        {
            var dlg = new SaveFileDialog
            {
                Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
                Title = "Save XML As",
                FileName = string.IsNullOrWhiteSpace(_currentFilePath) ? "document.xml" : Path.GetFileName(_currentFilePath)
            };

            if (dlg.ShowDialog() != true)
                return;

            _currentFilePath = dlg.FileName;
            _xml.SaveToFile(_currentFilePath, XmlText);
            Status = $"Saved: {Path.GetFileName(_currentFilePath)}";
        }

        private void Clear()
        {
            XmlText = "";
            _currentFilePath = null;
            Status = "Cleared.";
        }
    }
}
