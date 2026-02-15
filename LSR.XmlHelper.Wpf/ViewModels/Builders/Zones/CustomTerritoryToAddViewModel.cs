using LSR.XmlHelper.Core.Services.Builders.Zones;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.ViewModels.Builders.Zones
{
    public sealed class CustomTerritoryToAddViewModel
    {
        public CustomTerritoryToAddViewModel(ZoneDefinition definition)
        {
            Definition = definition;
        }

        public ZoneDefinition Definition { get; }

        public string DisplayText
        {
            get
            {
                var name = Definition.DisplayName ?? "";
                var internalName = Definition.InternalGameName ?? "";
                return $"{name} ({internalName})";
            }
        }

        public IReadOnlyList<ZoneBoundaryPoint> Boundaries => Definition.Boundaries;
    }
}
