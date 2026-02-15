using LSR.XmlHelper.Core.Models;
using LSR.XmlHelper.Core.Services.Builders;
using LSR.XmlHelper.Core.Services.Reading;
using LSR.XmlHelper.Wpf.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class DealerMenuGroupItemsEditorViewModel : ObservableObject
    {
        private readonly string _rootFolderPath;
        private string _shopMenusFilePath = "";
        private readonly Dictionary<string, string> _categoryByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, DenInventoryMenuItem[]> _itemsByMenuIndex = new Dictionary<int, DenInventoryMenuItem[]>();

        private string _currentGroupId = "";
        private string _searchText = "";
        private string _selectedItemName = "";
        private string _selectedCategory = "All";
        private bool _isEnabled;
        private bool _suppressMenuSwitch;

        private DenInventoryMenuItemViewModel? _selectedItem;
        private EmbeddedDealerMenuOptionViewModel? _selectedEmbeddedMenu;

        public ObservableCollection<EmbeddedDealerMenuOptionViewModel> EmbeddedMenus { get; } = new ObservableCollection<EmbeddedDealerMenuOptionViewModel>();

        public ObservableCollection<DenInventoryMenuItemViewModel> Items { get; } = new ObservableCollection<DenInventoryMenuItemViewModel>();
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();
        public ObservableCollection<string> AvailableItemNames { get; } = new ObservableCollection<string>();

        public ICollectionView ItemsView { get; }
        public ICollectionView AvailableItemNamesView { get; }

        public ICommand AddItemCommand { get; }
        public ICommand RemoveSelectedItemCommand { get; }

        public DealerMenuGroupItemsEditorViewModel(string rootFolderPath)
        {
            _rootFolderPath = rootFolderPath;

            ItemsView = CollectionViewSource.GetDefaultView(Items);
            ItemsView.Filter = FilterItems;

            AvailableItemNamesView = CollectionViewSource.GetDefaultView(AvailableItemNames);
            AvailableItemNamesView.Filter = FilterAvailableNames;

            AddItemCommand = new RelayCommand(AddItem, CanAddItem);
            RemoveSelectedItemCommand = new NotifyRelayCommand(RemoveSelectedItem, HasSelectedItem);

            Categories.Add("All");
        }
        public void SetShopMenusFile(string? filePath)
        {
            _shopMenusFilePath = (filePath ?? "").Trim();

            _itemsByMenuIndex.Clear();
            EmbeddedMenus.Clear();
            Items.Clear();
            SelectedEmbeddedMenu = null;

            if (!string.IsNullOrWhiteSpace(_currentGroupId))
                LoadGroup(_currentGroupId);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                if (!SetProperty(ref _isEnabled, value))
                    return;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public EmbeddedDealerMenuOptionViewModel? SelectedEmbeddedMenu
        {
            get => _selectedEmbeddedMenu;
            set
            {
                var previous = _selectedEmbeddedMenu;

                if (!SetProperty(ref _selectedEmbeddedMenu, value))
                    return;

                if (_suppressMenuSwitch)
                    return;

                SaveMenuToCache(previous?.MenuIndex);
                LoadSelectedMenuFromCacheOrXml();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (!SetProperty(ref _searchText, value))
                    return;

                ItemsView.Refresh();
                AvailableItemNamesView.Refresh();
            }
        }

        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                var v = (value ?? "All").Trim();
                if (string.IsNullOrWhiteSpace(v))
                    v = "All";

                if (!SetProperty(ref _selectedCategory, v))
                    return;

                ItemsView.Refresh();
                AvailableItemNamesView.Refresh();
            }
        }

        public string SelectedItemName
        {
            get => _selectedItemName;
            set
            {
                if (!SetProperty(ref _selectedItemName, value))
                    return;

                CommandManager.InvalidateRequerySuggested();
            }
        }

        public DenInventoryMenuItemViewModel? SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (!SetProperty(ref _selectedItem, value))
                    return;

                (RemoveSelectedItemCommand as NotifyRelayCommand)?.RaiseCanExecuteChanged();
            }
        }

        public void LoadGroup(string shopMenuGroupId)
        {
            _currentGroupId = (shopMenuGroupId ?? "").Trim();

            _itemsByMenuIndex.Clear();
            Items.Clear();
            EmbeddedMenus.Clear();
            AvailableItemNames.Clear();
            Categories.Clear();
            Categories.Add("All");
            _categoryByName.Clear();

            if (string.IsNullOrWhiteSpace(_currentGroupId))
            {
                ItemsView.Refresh();
                AvailableItemNamesView.Refresh();
                return;
            }

            var categoryCatalog = new ModItemCategoryCatalogService();
            foreach (var x in categoryCatalog.GetAllItemsWithCategories(_rootFolderPath))
            {
                var name = (x.Name ?? "").Trim();
                var cat = (x.Category ?? "").Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (string.IsNullOrWhiteSpace(cat))
                    cat = "Other";

                if (string.Equals(cat, "Vehicles", StringComparison.OrdinalIgnoreCase))
                    continue;

                _categoryByName[name] = cat;
            }

            foreach (var cat in _categoryByName.Values
            .Where(x => !string.Equals((x ?? "").Trim(), "Vehicles", StringComparison.OrdinalIgnoreCase))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                Categories.Add(cat);
            }

            var itemNamesCatalog = new ShopMenuItemNameCatalogService();
            foreach (var n in itemNamesCatalog.GetAllItemNames(_rootFolderPath).OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
            {
                if (_categoryByName.TryGetValue(n, out var mapped) && string.Equals((mapped ?? "").Trim(), "Vehicles", StringComparison.OrdinalIgnoreCase))
                    continue;

                AvailableItemNames.Add(n);
            }

            var reader = new ShopMenuGroupMenuItemsReadService();
            var menus = string.IsNullOrWhiteSpace(_shopMenusFilePath)
                ? reader.GetMenusForGroupId(_rootFolderPath, _currentGroupId)
                : reader.GetMenusForGroupIdFromFile(_shopMenusFilePath, _currentGroupId);

            foreach (var m in menus)
                EmbeddedMenus.Add(new EmbeddedDealerMenuOptionViewModel(m.Index, m.DisplayName));

            _suppressMenuSwitch = true;

            if (EmbeddedMenus.Count > 0)
                SelectedEmbeddedMenu = EmbeddedMenus[0];
            else
                SelectedEmbeddedMenu = null;

            _suppressMenuSwitch = false;

            LoadSelectedMenuFromCacheOrXml();
            ItemsView.Refresh();
            AvailableItemNamesView.Refresh();
        }

        public IReadOnlyList<(int MenuIndex, DenInventoryMenuItem[] Items)> GetMenuEditsForSave()
        {
            SaveCurrentMenuToCache();

            return _itemsByMenuIndex
                .OrderBy(kvp => kvp.Key)
                .Select(kvp => (kvp.Key, kvp.Value))
                .ToArray();
        }

        private void SaveCurrentMenuToCache()
        {
            SaveMenuToCache(_selectedEmbeddedMenu?.MenuIndex);
        }

        private void SaveMenuToCache(int? menuIndex)
        {
            if (menuIndex is null)
                return;

            if (Items.Count == 0 && !_itemsByMenuIndex.ContainsKey(menuIndex.Value))
                return;

            var models = Items
                .Select(x => x.ToModel())
                .Where(x => !string.IsNullOrWhiteSpace(x.ModItemName))
                .ToArray();

            _itemsByMenuIndex[menuIndex.Value] = models;
        }

        private void LoadSelectedMenuFromCacheOrXml()
        {
            Items.Clear();

            if (SelectedEmbeddedMenu is null)
            {
                ItemsView.Refresh();
                return;
            }

            var menuIndex = SelectedEmbeddedMenu.MenuIndex;

            if (_itemsByMenuIndex.TryGetValue(menuIndex, out var cached))
            {
                foreach (var m in cached)
                    Items.Add(ToVm(m));

                ItemsView.Refresh();
                return;
            }

            var reader = new ShopMenuGroupMenuItemsReadService();
            var fromXml = string.IsNullOrWhiteSpace(_shopMenusFilePath)
                ? reader.GetItemsForGroupMenuIndex(_rootFolderPath, _currentGroupId, menuIndex)
                : reader.GetItemsForGroupMenuIndexFromFile(_shopMenusFilePath, _currentGroupId, menuIndex);

            var arr = fromXml.ToArray();
            _itemsByMenuIndex[menuIndex] = arr;

            foreach (var m in arr)
                Items.Add(ToVm(m));

            ItemsView.Refresh();
        }

        private bool FilterItems(object obj)
        {
            if (obj is not DenInventoryMenuItemViewModel vm)
                return false;

            var s = (SearchText ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(s))
            {
                var name = (vm.ModItemName ?? "").Trim();
                if (name.IndexOf(s, StringComparison.OrdinalIgnoreCase) < 0)
                    return false;
            }

            var cat = (SelectedCategory ?? "All").Trim();
            if (!string.Equals(cat, "All", StringComparison.OrdinalIgnoreCase))
            {
                var itemCat = (vm.Category ?? "").Trim();
                if (!string.Equals(itemCat, cat, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private bool FilterAvailableNames(object obj)
        {
            if (obj is not string nameRaw)
                return false;

            var name = (nameRaw ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                return false;

            var s = (SearchText ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(s) && name.IndexOf(s, StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            if (_categoryByName.TryGetValue(name, out var mappedAny) && string.Equals((mappedAny ?? "").Trim(), "Vehicles", StringComparison.OrdinalIgnoreCase))
                return false;

            var cat = (SelectedCategory ?? "All").Trim();
            if (!string.Equals(cat, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (!_categoryByName.TryGetValue(name, out var mapped))
                    mapped = "Other";

                if (string.Equals((mapped ?? "").Trim(), "Vehicles", StringComparison.OrdinalIgnoreCase))
                    return false;

                if (!string.Equals((mapped ?? "Other").Trim(), cat, StringComparison.OrdinalIgnoreCase))
                    return false;
            }
            return true;
        }

        private bool CanAddItem()
        {
            return IsEnabled && !string.IsNullOrWhiteSpace((SelectedItemName ?? "").Trim());
        }

        private void AddItem()
        {
            var itemName = (SelectedItemName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(itemName))
                return;

            var resolvedCategory = (SelectedCategory ?? "All").Trim();
            if (string.Equals(resolvedCategory, "All", StringComparison.OrdinalIgnoreCase))
            {
                if (_categoryByName.TryGetValue(itemName, out var mapped) && !string.IsNullOrWhiteSpace(mapped))
                    resolvedCategory = mapped;
            }

            if (string.IsNullOrWhiteSpace(resolvedCategory))
                resolvedCategory = "Other";

            var isDuplicate = Items.Any(x => string.Equals((x.ModItemName ?? "").Trim(), itemName, StringComparison.OrdinalIgnoreCase));

            var buyPrice = 100;
            var sellPrice = 50;

            var priceLookup = new ShopMenuItemPriceLookupService();
            if (priceLookup.TryGetFirstPrices(_rootFolderPath, itemName, out var foundBuy, out var foundSell))
            {
                buyPrice = foundBuy;
                sellPrice = foundSell;
            }

            var added = new DenInventoryMenuItemViewModel
            {
                ModItemName = itemName,
                PurchasePrice = buyPrice,
                SalesPrice = isDuplicate ? -1 : sellPrice,
                MinimumPurchaseAmount = 1,
                MaximumPurchaseAmount = 10,
                PurchaseIncrement = 1,
                NumberOfItemsToSellToPlayer = -1,
                NumberOfItemsToPurchaseFromPlayer = -1,
                IsIllicilt = false,
                IsFree = false,
                SubPrice = 1,
                SubAmount = 30,
                NumberOfItemsSoldToPlayer = 0,
                NumberOfItemsPurchasedByPlayer = 0,
                Category = resolvedCategory
            };

            Items.Add(added);
            SelectedItem = added;

            ItemsView.Refresh();
        }

        private bool HasSelectedItem()
        {
            return IsEnabled && SelectedItem is not null;
        }

        private void RemoveSelectedItem()
        {
            if (SelectedItem is null)
                return;

            var toRemove = SelectedItem;
            SelectedItem = null;
            Items.Remove(toRemove);

            ItemsView.Refresh();
        }

        private DenInventoryMenuItemViewModel ToVm(DenInventoryMenuItem m)
        {
            var name = (m.ModItemName ?? "").Trim();
            var cat = "Other";
            if (!string.IsNullOrWhiteSpace(name) && _categoryByName.TryGetValue(name, out var mapped) && !string.IsNullOrWhiteSpace(mapped))
                cat = mapped;

            return new DenInventoryMenuItemViewModel
            {
                NumberOfItemsSoldToPlayer = m.NumberOfItemsSoldToPlayer,
                NumberOfItemsPurchasedByPlayer = m.NumberOfItemsPurchasedByPlayer,
                ModItemName = (m.ModItemName ?? "").Trim(),
                PurchasePrice = m.PurchasePrice,
                SalesPrice = m.SalesPrice,
                IsIllicilt = m.IsIllicilt,
                SubPrice = m.SubPrice,
                SubAmount = m.SubAmount,
                MinimumPurchaseAmount = m.MinimumPurchaseAmount,
                MaximumPurchaseAmount = m.MaximumPurchaseAmount,
                PurchaseIncrement = m.PurchaseIncrement,
                NumberOfItemsToSellToPlayer = m.NumberOfItemsToSellToPlayer,
                NumberOfItemsToPurchaseFromPlayer = m.NumberOfItemsToPurchaseFromPlayer,
                IsFree = m.IsFree,
                Category = cat
            };
        }
    }
}
