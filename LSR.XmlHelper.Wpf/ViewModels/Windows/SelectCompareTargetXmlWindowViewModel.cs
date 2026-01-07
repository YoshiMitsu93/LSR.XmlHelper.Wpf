using LSR.XmlHelper.Wpf.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.ViewModels.Windows
{
    public sealed class SelectCompareTargetXmlWindowViewModel : ObservableObject
    {
        private XmlFileListItem? _selectedXmlFile;

        public SelectCompareTargetXmlWindowViewModel(List<XmlFileListItem> files, string? preferredFullPath)
        {
            Files = new ObservableCollection<XmlFileListItem>(files ?? new List<XmlFileListItem>());

            if (!string.IsNullOrWhiteSpace(preferredFullPath))
                SelectedXmlFile = Files.FirstOrDefault(x => string.Equals(x.FullPath, preferredFullPath, StringComparison.OrdinalIgnoreCase));

            SelectedXmlFile ??= Files.FirstOrDefault();

            OkCommand = new RelayCommand(() => CloseRequested?.Invoke(this, true), () => SelectedXmlFile is not null);
            CancelCommand = new RelayCommand(() => CloseRequested?.Invoke(this, false));
        }

        public event EventHandler<bool>? CloseRequested;

        public ObservableCollection<XmlFileListItem> Files { get; }

        public XmlFileListItem? SelectedXmlFile
        {
            get => _selectedXmlFile;
            set
            {
                if (!SetProperty(ref _selectedXmlFile, value))
                    return;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public RelayCommand OkCommand { get; }
        public RelayCommand CancelCommand { get; }
    }
}
