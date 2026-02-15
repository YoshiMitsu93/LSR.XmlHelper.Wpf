namespace LSR.XmlHelper.Core.Models
{
    public sealed class DenInventoryMenuItem
    {
        public int NumberOfItemsSoldToPlayer { get; set; } = 0;
        public int NumberOfItemsPurchasedByPlayer { get; set; } = 0;

        public string ModItemName { get; set; } = "";

        public int PurchasePrice { get; set; } = 0;
        public int SalesPrice { get; set; } = -1;

        public bool IsIllicilt { get; set; } = false;

        public int SubPrice { get; set; } = 1;
        public int SubAmount { get; set; } = 30;

        public int MinimumPurchaseAmount { get; set; } = 1;
        public int MaximumPurchaseAmount { get; set; } = 10;

        public int PurchaseIncrement { get; set; } = 1;

        public int NumberOfItemsToSellToPlayer { get; set; } = -1;
        public int NumberOfItemsToPurchaseFromPlayer { get; set; } = -1;

        public bool IsFree { get; set; } = false;
    }
}
