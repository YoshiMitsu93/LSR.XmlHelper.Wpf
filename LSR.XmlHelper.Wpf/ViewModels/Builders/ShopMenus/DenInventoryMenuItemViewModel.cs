using LSR.XmlHelper.Core.Models;
using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class DenInventoryMenuItemViewModel : ObservableObject
    {
        private int _numberOfItemsSoldToPlayer;
        private int _numberOfItemsPurchasedByPlayer;
        private string _modItemName = "";
        private int _purchasePrice;
        private int _salesPrice = -1;
        private bool _isIllicilt;
        private int _subPrice = 1;
        private int _subAmount = 30;
        private int _minimumPurchaseAmount = 1;
        private int _maximumPurchaseAmount = 10;
        private int _purchaseIncrement = 1;
        private int _numberOfItemsToSellToPlayer = -1;
        private int _numberOfItemsToPurchaseFromPlayer = -1;
        private bool _isFree;
        private string _category = "All";

        public int NumberOfItemsSoldToPlayer
        {
            get => _numberOfItemsSoldToPlayer;
            set => SetProperty(ref _numberOfItemsSoldToPlayer, value);
        }

        public int NumberOfItemsPurchasedByPlayer
        {
            get => _numberOfItemsPurchasedByPlayer;
            set => SetProperty(ref _numberOfItemsPurchasedByPlayer, value);
        }

        public string ModItemName
        {
            get => _modItemName;
            set => SetProperty(ref _modItemName, value);
        }

        public int PurchasePrice
        {
            get => _purchasePrice;
            set => SetProperty(ref _purchasePrice, value);
        }

        public int SalesPrice
        {
            get => _salesPrice;
            set => SetProperty(ref _salesPrice, value);
        }

        public bool IsIllicilt
        {
            get => _isIllicilt;
            set => SetProperty(ref _isIllicilt, value);
        }

        public int SubPrice
        {
            get => _subPrice;
            set => SetProperty(ref _subPrice, value);
        }

        public int SubAmount
        {
            get => _subAmount;
            set => SetProperty(ref _subAmount, value);
        }

        public int MinimumPurchaseAmount
        {
            get => _minimumPurchaseAmount;
            set => SetProperty(ref _minimumPurchaseAmount, value);
        }

        public int MaximumPurchaseAmount
        {
            get => _maximumPurchaseAmount;
            set => SetProperty(ref _maximumPurchaseAmount, value);
        }

        public int PurchaseIncrement
        {
            get => _purchaseIncrement;
            set => SetProperty(ref _purchaseIncrement, value);
        }

        public int NumberOfItemsToSellToPlayer
        {
            get => _numberOfItemsToSellToPlayer;
            set => SetProperty(ref _numberOfItemsToSellToPlayer, value);
        }

        public int NumberOfItemsToPurchaseFromPlayer
        {
            get => _numberOfItemsToPurchaseFromPlayer;
            set => SetProperty(ref _numberOfItemsToPurchaseFromPlayer, value);
        }

        public bool IsFree
        {
            get => _isFree;
            set => SetProperty(ref _isFree, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public DenInventoryMenuItem ToModel()
        {
            return new DenInventoryMenuItem
            {
                NumberOfItemsSoldToPlayer = NumberOfItemsSoldToPlayer,
                NumberOfItemsPurchasedByPlayer = NumberOfItemsPurchasedByPlayer,
                ModItemName = ModItemName?.Trim() ?? "",
                PurchasePrice = PurchasePrice,
                SalesPrice = SalesPrice,
                IsIllicilt = IsIllicilt,
                SubPrice = SubPrice,
                SubAmount = SubAmount,
                MinimumPurchaseAmount = MinimumPurchaseAmount,
                MaximumPurchaseAmount = MaximumPurchaseAmount,
                PurchaseIncrement = PurchaseIncrement,
                NumberOfItemsToSellToPlayer = NumberOfItemsToSellToPlayer,
                NumberOfItemsToPurchaseFromPlayer = NumberOfItemsToPurchaseFromPlayer,
                IsFree = IsFree
            };
        }
    }
}
