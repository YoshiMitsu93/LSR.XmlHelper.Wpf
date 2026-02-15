using System;
using System.Collections.Generic;
using System.Linq;
using LSR.XmlHelper.Core.Services.Builders.Intoxicants;
using LSR.XmlHelper.Core.Services.Builders.ShopMenus;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class TerritoryMenuZoneLabelService
    {
        public Dictionary<string, (string Dealers, string Customers)> GetZoneDrugSummary(
            string rootFolderPath,
            IReadOnlyCollection<string> zoneInternalNames)
        {
            var result = new Dictionary<string, (string Dealers, string Customers)>(StringComparer.OrdinalIgnoreCase);

            if (zoneInternalNames is null || zoneInternalNames.Count == 0)
                return result;

            var intoxicants = new IntoxicantCatalogService()
                .GetIntoxicantNames(rootFolderPath);

            var zoneLookup = new ZoneMenuContainersLookupService();
            var zones = zoneLookup.GetZoneMenuContainers(rootFolderPath, zoneInternalNames);

            var resolver = new ShopMenuGroupIntoxicantResolver();

            foreach (var z in zones)
            {
                var dealerDrugs = resolver.Resolve(rootFolderPath, z.DealerMenuContainerId ?? "", intoxicants);
                var customerDrugs = resolver.Resolve(rootFolderPath, z.CustomerMenuContainerId ?? "", intoxicants);

                var dealerText = dealerDrugs.Count == 0 ? "" : string.Join(", ", dealerDrugs.OrderBy(x => x));
                var customerText = customerDrugs.Count == 0 ? "" : string.Join(", ", customerDrugs.OrderBy(x => x));

                result[z.ZoneInternalName] = (dealerText, customerText);
            }

            return result;
        }
    }
}
