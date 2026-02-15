namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class ZoneOptionViewModel
    {
        public ZoneOptionViewModel(
            string internalGameName,
            string displayName,
            string usedBy,
            string dealerDrugs,
            string customerDrugs)
        {
            InternalGameName = internalGameName;
            DisplayName = displayName;
            UsedBy = usedBy;
            DealerDrugs = dealerDrugs;
            CustomerDrugs = customerDrugs;
        }

        public string InternalGameName { get; }
        public string DisplayName { get; }
        public string UsedBy { get; }
        public string DealerDrugs { get; }
        public string CustomerDrugs { get; }

        public string DisplayText
        {
            get
            {
                var text = $"{DisplayName} ({InternalGameName})";

                if (!string.IsNullOrWhiteSpace(UsedBy))
                    text += " | Used by: " + UsedBy;

                if (!string.IsNullOrWhiteSpace(DealerDrugs))
                    text += " | Civilian Drug Dealers: " + DealerDrugs;

                if (!string.IsNullOrWhiteSpace(CustomerDrugs))
                    text += " | Civilian Drug Customers: " + CustomerDrugs;

                return text;
            }
        }
    }
}
