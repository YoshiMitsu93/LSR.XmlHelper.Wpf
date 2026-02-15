using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class TerritoryMenuSetupSummaryService
    {
        public string Build(string rootFolderPath, IReadOnlyCollection<string> zoneInternalNames)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return "";

            if (zoneInternalNames is null || zoneInternalNames.Count == 0)
                return "";

            var normalizedZones = zoneInternalNames
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (normalizedZones.Length == 0)
                return "";

            var zoneCatalog = new ZoneCatalogService();
            var zoneDisplay = zoneCatalog
                .GetZones(rootFolderPath)
                .ToDictionary(x => x.InternalGameName, x => x.DisplayName ?? x.InternalGameName, StringComparer.OrdinalIgnoreCase);

            var zoneLookup = new ZoneMenuContainersLookupService();
            var zoneResults = zoneLookup.GetZoneMenuContainers(rootFolderPath, normalizedZones);

            if (zoneResults.Count == 0)
                return "";

            var groupCatalog = new ShopMenuGroupCatalogService();
            var groups = groupCatalog.GetShopMenuGroups(rootFolderPath)
                .ToDictionary(x => x.Id, x => x.Name ?? "", StringComparer.OrdinalIgnoreCase);

            var menuCatalog = new ShopMenuCatalogService();

            var dealerIds = zoneResults
                .Select(x => (x.DealerMenuContainerId ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var customerIds = zoneResults
                .Select(x => (x.CustomerMenuContainerId ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var sb = new StringBuilder();

            if (dealerIds.Length > 1 || customerIds.Length > 1)
            {
                sb.AppendLine("WARNING: Selected Zones have multiple existing territory setups.");
                sb.AppendLine("If you set new values above, you will overwrite the existing values for all selected Zones.");
                sb.AppendLine();
            }

            sb.AppendLine("Dealer setup found in selected Zones:");
            if (dealerIds.Length == 0)
            {
                sb.AppendLine("- (no DealerMenuContainerID set)");
            }
            else
            {
                foreach (var id in dealerIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                {
                    var groupName = groups.TryGetValue(id, out var name) ? name : "";
                    var menus = menuCatalog.GetShopMenusForGroup(rootFolderPath, id);

                    var zonesUsing = zoneResults
                        .Where(x => string.Equals((x.DealerMenuContainerId ?? "").Trim(), id, StringComparison.OrdinalIgnoreCase))
                        .Select(x =>
                        {
                            var display = zoneDisplay.TryGetValue(x.ZoneInternalName, out var d) ? d : x.ZoneInternalName;
                            return display + " (" + x.ZoneInternalName + ")";
                        })
                        .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    var title = string.IsNullOrWhiteSpace(groupName) ? id : id + " (" + groupName + ")";
                    sb.AppendLine("- " + title);
                    sb.AppendLine("  Zones: " + string.Join(", ", zonesUsing));

                    if (menus.Count == 0)
                        sb.AppendLine("  Menus: (none found)");
                    else
                        sb.AppendLine("  Menus: " + string.Join(", ", menus.Select(m => (m.Id + " " + (m.Name ?? "")).Trim())));
                }
            }

            sb.AppendLine();
            sb.AppendLine("Customer setup found in selected Zones:");
            if (customerIds.Length == 0)
            {
                sb.AppendLine("- (no CustomerMenuContainerID set)");
            }
            else
            {
                foreach (var id in customerIds.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                {
                    var groupName = groups.TryGetValue(id, out var name) ? name : "";
                    var menus = menuCatalog.GetShopMenusForGroup(rootFolderPath, id);

                    var zonesUsing = zoneResults
                        .Where(x => string.Equals((x.CustomerMenuContainerId ?? "").Trim(), id, StringComparison.OrdinalIgnoreCase))
                        .Select(x =>
                        {
                            var display = zoneDisplay.TryGetValue(x.ZoneInternalName, out var d) ? d : x.ZoneInternalName;
                            return display + " (" + x.ZoneInternalName + ")";
                        })
                        .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    var title = string.IsNullOrWhiteSpace(groupName) ? id : id + " (" + groupName + ")";
                    sb.AppendLine("- " + title);
                    sb.AppendLine("  Zones: " + string.Join(", ", zonesUsing));

                    if (menus.Count == 0)
                        sb.AppendLine("  Menus: (none found)");
                    else
                        sb.AppendLine("  Menus: " + string.Join(", ", menus.Select(m => (m.Id + " " + (m.Name ?? "")).Trim())));
                }
            }

            return sb.ToString().TrimEnd();
        }
    }
}
