using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class LoanParameterEntryViewModel : ObservableObject
    {
        private string _resepectLevel = "";
        private string _rate = "";
        private string _maxPeriods = "";
        private string _minAmount = "";
        private string _maxAmount = "";

        public LoanParameterEntryViewModel()
        {
        }

        public LoanParameterEntryViewModel(string resepectLevel, string rate, string maxPeriods, string minAmount, string maxAmount)
        {
            _resepectLevel = resepectLevel;
            _rate = rate;
            _maxPeriods = maxPeriods;
            _minAmount = minAmount;
            _maxAmount = maxAmount;
        }

        public string ResepectLevel
        {
            get => _resepectLevel;
            set => SetProperty(ref _resepectLevel, value);
        }

        public string Rate
        {
            get => _rate;
            set => SetProperty(ref _rate, value);
        }

        public string MaxPeriods
        {
            get => _maxPeriods;
            set => SetProperty(ref _maxPeriods, value);
        }

        public string MinAmount
        {
            get => _minAmount;
            set => SetProperty(ref _minAmount, value);
        }

        public string MaxAmount
        {
            get => _maxAmount;
            set => SetProperty(ref _maxAmount, value);
        }

        public LoanParameterEntryViewModel Clone()
        {
            return new LoanParameterEntryViewModel(
                (ResepectLevel ?? "").Trim(),
                (Rate ?? "").Trim(),
                (MaxPeriods ?? "").Trim(),
                (MinAmount ?? "").Trim(),
                (MaxAmount ?? "").Trim());
        }
    }
}
