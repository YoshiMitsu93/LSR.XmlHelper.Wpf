using System.Collections.Generic;

namespace LSR.XmlHelper.Core.Services.Builders.Zones
{
    public sealed class ZoneDefinition
    {
        public ZoneDefinition(
         string internalGameName,
         string displayName,
         string countyId,
         string stateId,
         bool isRestrictedDuringWanted,
         bool isSpecificLocation,
         string economy,
         string type,
         IReadOnlyList<ZoneBoundaryPoint> boundaries)
        {
            InternalGameName = internalGameName;
            DisplayName = displayName;
            CountyID = countyId;
            StateID = stateId;
            IsRestrictedDuringWanted = isRestrictedDuringWanted;
            IsSpecificLocation = isSpecificLocation;
            Economy = economy;
            Type = type;
            Boundaries = boundaries;
        }

        public string InternalGameName { get; }
        public string DisplayName { get; }
        public string CountyID { get; }
        public string StateID { get; }
        public bool IsRestrictedDuringWanted { get; }
        public bool IsSpecificLocation { get; }
        public string Economy { get; }
        public string Type { get; }
        public IReadOnlyList<ZoneBoundaryPoint> Boundaries { get; }
    }
}
