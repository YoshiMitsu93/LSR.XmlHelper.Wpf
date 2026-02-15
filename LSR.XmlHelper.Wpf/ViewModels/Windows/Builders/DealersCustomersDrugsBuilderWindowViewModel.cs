using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Core.Services.Builders;
using LSR.XmlHelper.Core.Services.IO;
using LSR.XmlHelper.Core.Services.Reading;
using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.ViewModels.Builders;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace LSR.XmlHelper.Wpf.ViewModels.Windows
{
    public sealed class DealersCustomersDrugsBuilderWindowViewModel : ObservableObject
    {
        private readonly string _rootFolderPath;
        private readonly Services.AppearanceService _appearance;
        private bool _showCustomerMenus;
        private ShopMenusFileOptionViewModel? _selectedShopMenusFile;
        private ShopMenuGroupOptionViewModel? _selectedShopMenuGroup;
        private string _manualShopMenuGroupId = "";

        public DealersCustomersDrugsBuilderWindowViewModel(Services.AppearanceService appearance, string rootFolderPath)
        {
            _appearance = appearance;
            _rootFolderPath = rootFolderPath;

            ShopMenusFiles = new ObservableCollection<ShopMenusFileOptionViewModel>();
            ShopMenuGroups = new ObservableCollection<ShopMenuGroupOptionViewModel>();
            DealerMenuGroupItemsEditor = new DealerMenuGroupItemsEditorViewModel(_rootFolderPath);

            RefreshShopMenuGroupsCommand = new RelayCommand(RefreshShopMenuGroups);
            SaveShopMenusCommand = new RelayCommand(SaveShopMenus);
            ReloadCommand = new RelayCommand(Reload);

            RefreshShopMenusFiles();
        }

        public Services.AppearanceService Appearance => _appearance;
        public ObservableCollection<ShopMenusFileOptionViewModel> ShopMenusFiles { get; }
        public ObservableCollection<ShopMenuGroupOptionViewModel> ShopMenuGroups { get; }
        public DealerMenuGroupItemsEditorViewModel DealerMenuGroupItemsEditor { get; }

        public RelayCommand RefreshShopMenuGroupsCommand { get; }
        public RelayCommand SaveShopMenusCommand { get; }
        public RelayCommand ReloadCommand { get; }

        public bool ShowCustomerMenus
        {
            get => _showCustomerMenus;
            set
            {
                if (!SetProperty(ref _showCustomerMenus, value))
                    return;

                RefreshShopMenuGroups();
            }
        }

        public ShopMenusFileOptionViewModel? SelectedShopMenusFile
        {
            get => _selectedShopMenusFile;
            set
            {
                if (!SetProperty(ref _selectedShopMenusFile, value))
                    return;

                var file = _selectedShopMenusFile?.FilePath;
                DealerMenuGroupItemsEditor.SetShopMenusFile(file);
                RefreshShopMenuGroups();
                OnPropertyChanged(nameof(SelectedShopMenusFilePath));
                OnPropertyChanged(nameof(IsBaseShopMenusFileSelected));
            }
        }
        public string SelectedShopMenusFilePath
        {
            get
            {
                return _selectedShopMenusFile?.FilePath ?? "";
            }
        }

        public bool IsBaseShopMenusFileSelected
        {
            get
            {
                var path = SelectedShopMenusFilePath;
                if (string.IsNullOrWhiteSpace(path))
                    return false;

                var name = Path.GetFileName(path);
                return string.Equals(name, "ShopMenus.xml", StringComparison.OrdinalIgnoreCase);
            }
        }

        public ShopMenuGroupOptionViewModel? SelectedShopMenuGroup
        {
            get => _selectedShopMenuGroup;
            set
            {
                if (!SetProperty(ref _selectedShopMenuGroup, value))
                    return;

                var id = _selectedShopMenuGroup?.Id ?? "";
                ManualShopMenuGroupId = id;
                DealerMenuGroupItemsEditor.LoadGroup(id);
            }
        }

        public string ManualShopMenuGroupId
        {
            get => _manualShopMenuGroupId;
            set
            {
                if (!SetProperty(ref _manualShopMenuGroupId, value))
                    return;

                DealerMenuGroupItemsEditor.LoadGroup(_manualShopMenuGroupId);
            }
        }

        private void RefreshShopMenusFiles()
        {
            ShopMenusFiles.Clear();

            if (string.IsNullOrWhiteSpace(_rootFolderPath) || !Directory.Exists(_rootFolderPath))
                return;

            var files = Directory.EnumerateFiles(_rootFolderPath, "ShopMenus*.xml", SearchOption.TopDirectoryOnly)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var f in files)
            {
                var display = Path.GetFileName(f);
                ShopMenusFiles.Add(new ShopMenusFileOptionViewModel(display, f));
            }

            var preferred = ShopMenusFiles.FirstOrDefault(x => string.Equals(x.DisplayName, "ShopMenus.xml", StringComparison.OrdinalIgnoreCase));
            SelectedShopMenusFile = preferred ?? ShopMenusFiles.FirstOrDefault();
        }

        private void RefreshShopMenuGroups()
        {
            ShopMenuGroups.Clear();

            var file = SelectedShopMenusFile?.FilePath;
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                return;

            XDocument doc;
            try
            {
                doc = XDocument.Load(file, LoadOptions.None);
            }
            catch
            {
                return;
            }

            var groups = doc.Descendants("ShopMenuGroup")
                .Select(x =>
                {
                    var id = ((string?)x.Element("ID") ?? "").Trim();
                    var name = ((string?)x.Element("Name") ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        name = id;
                    return (Id: id, Name: name);
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Id))
                .ToArray();

            foreach (var g in groups)
            {
                var isCustomer = g.Id.IndexOf("Customer", StringComparison.OrdinalIgnoreCase) >= 0
                    || g.Name.IndexOf("Customer", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!ShowCustomerMenus && isCustomer)
                    continue;

                ShopMenuGroups.Add(new ShopMenuGroupOptionViewModel(g.Id, g.Name));
            }

            var selectId = (ManualShopMenuGroupId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(selectId))
            {
                SelectedShopMenuGroup = ShopMenuGroups.FirstOrDefault();
                return;
            }

            SelectedShopMenuGroup = ShopMenuGroups.FirstOrDefault(x => string.Equals(x.Id, selectId, StringComparison.OrdinalIgnoreCase))
                ?? ShopMenuGroups.FirstOrDefault();
        }

        private void SaveShopMenus()
        {
            var file = SelectedShopMenusFile?.FilePath;
            if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
            {
                System.Windows.MessageBox.Show("No ShopMenus*.xml file is selected.", "Save", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var groupId = (ManualShopMenuGroupId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(groupId))
            {
                System.Windows.MessageBox.Show("No ShopMenuGroup ID is selected.", "Save", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            XDocument doc;
            try
            {
                doc = XDocument.Load(file, LoadOptions.None);
            }
            catch
            {
                System.Windows.MessageBox.Show("Failed to read the selected ShopMenus file.", "Save", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            var edits = DealerMenuGroupItemsEditor.GetMenuEditsForSave();
            if (edits.Count == 0)
            {
                System.Windows.MessageBox.Show("No menu items were edited.", "Save", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                return;
            }

            var applier = new ShopMenuGroupMenuItemsApplyService();
            foreach (var e in edits)
                applier.ApplyItemsToGroupMenuIndex(doc, groupId, e.MenuIndex, e.Items);

            var root = new XmlHelperRootService();
            var backupService = new XmlBackupService(root);
            backupService.Backup(file);

            try
            {
                doc.Save(file);
            }
            catch
            {
                System.Windows.MessageBox.Show("Failed to write the ShopMenus file. It may be locked or read-only.", "Save", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }

            System.Windows.MessageBox.Show("Saved. A backup was created before writing.", "Save", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void Reload()
        {
            RefreshShopMenusFiles();
            RefreshShopMenuGroups();
        }
    }

    public sealed class ShopMenusFileOptionViewModel
    {
        public ShopMenusFileOptionViewModel(string displayName, string filePath)
        {
            DisplayName = displayName;
            FilePath = filePath;
        }

        public string DisplayName { get; }
        public string FilePath { get; }

        public override string ToString() => DisplayName;
    }
}
