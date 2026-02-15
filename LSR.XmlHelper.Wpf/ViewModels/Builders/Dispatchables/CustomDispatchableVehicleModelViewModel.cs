using System;
using System.Collections.Generic;
using System.Linq;
using LSR.XmlHelper.Wpf.Infrastructure;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders
{
    public sealed class CustomDispatchableVehicleModelViewModel : ObservableObject
    {
        private string _modelName = "";
        private string _variantKey = "";
        private string _variantDisplayText = "";
        private string _overridePrimaryColorId = "";
        private string _overrideSecondaryColorId = "";
        private string _overrideLiveriesCsv = "";

        public CustomDispatchableVehicleModelViewModel(string modelName, string variantKey, string variantDisplayText)
        {
            _modelName = modelName;
            _variantKey = variantKey;
            _variantDisplayText = variantDisplayText;
        }

        public string ModelName
        {
            get => _modelName;
            set
            {
                if (SetProperty(ref _modelName, value))
                    OnPropertyChanged(nameof(DisplayText));
            }
        }

        public string VariantKey
        {
            get => _variantKey;
            set => SetProperty(ref _variantKey, value);
        }

        public string VariantDisplayText
        {
            get => _variantDisplayText;
            set
            {
                if (SetProperty(ref _variantDisplayText, value))
                    OnPropertyChanged(nameof(DisplayText));
            }
        }

        public string OverridePrimaryColorId
        {
            get => _overridePrimaryColorId;
            set => SetProperty(ref _overridePrimaryColorId, value);
        }

        public string OverrideSecondaryColorId
        {
            get => _overrideSecondaryColorId;
            set => SetProperty(ref _overrideSecondaryColorId, value);
        }

        public string OverrideLiveriesCsv
        {
            get => _overrideLiveriesCsv;
            set => SetProperty(ref _overrideLiveriesCsv, value);
        }

        public string DisplayText
        {
            get
            {
                var suffix = string.IsNullOrWhiteSpace(VariantDisplayText) ? "" : $" | {VariantDisplayText}";
                return $"{ModelName}{suffix}";
            }
        }

        public IReadOnlyList<int> GetOverrideLiveryIds()
        {
            if (string.IsNullOrWhiteSpace(OverrideLiveriesCsv))
                return Array.Empty<int>();

            var parts = OverrideLiveriesCsv.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var ids = new List<int>();

            foreach (var p in parts)
            {
                if (int.TryParse(p.Trim(), out var id))
                    ids.Add(id);
            }

            return ids.Distinct().ToList();
        }

        public bool TryGetOverridePrimaryColorId(out int value)
        {
            return int.TryParse((OverridePrimaryColorId ?? "").Trim(), out value);
        }

        public bool TryGetOverrideSecondaryColorId(out int value)
        {
            return int.TryParse((OverrideSecondaryColorId ?? "").Trim(), out value);
        }

        public override string ToString()
        {
            return DisplayText;
        }
    }
}
