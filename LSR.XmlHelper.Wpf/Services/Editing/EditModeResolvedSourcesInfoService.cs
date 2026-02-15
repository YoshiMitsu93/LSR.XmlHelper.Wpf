using System;
using System.IO;
using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Core.Services.Builders;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class EditModeResolvedSourcesInfoService
    {
        public string Build(string rootFolderPath, string gangId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return "";

            if (string.IsNullOrWhiteSpace(gangId))
                return "";

            var resolver = new LsrConfigFileResolverService();

            var gangsPath = resolver.ResolveGangFile(rootFolderPath, gangId) ?? Path.Combine(rootFolderPath, "Gangs.xml");

            var snapshotReader = new GangEditSnapshotReadService();
            var snapshot = snapshotReader.TryGet(rootFolderPath, gangId);

            var peoplePath = "";
            var vehiclesPath = "";
            var territoriesPath = resolver.ResolveGangTerritoriesFile(rootFolderPath, gangId) ?? Path.Combine(rootFolderPath, "GangTerritories.xml");

            var locationsPath = "";
            var shopMenusPath = "";

            var zonesPath = Path.Combine(rootFolderPath, "Zones.xml");

            if (snapshot is not null)
            {
                if (!string.IsNullOrWhiteSpace(snapshot.PeopleGroupId))
                    peoplePath = resolver.ResolveDispatchablePeopleFile(rootFolderPath, snapshot.PeopleGroupId) ?? Path.Combine(rootFolderPath, "DispatchablePeople.xml");

                if (!string.IsNullOrWhiteSpace(snapshot.VehicleGroupId))
                    vehiclesPath = resolver.ResolveDispatchableVehiclesFile(rootFolderPath, snapshot.VehicleGroupId) ?? Path.Combine(rootFolderPath, "DispatchableVehicles.xml");

                locationsPath = resolver.ResolveLocationsFileForGangDens(rootFolderPath, gangId) ?? Path.Combine(rootFolderPath, "Locations.xml");

                if (!string.IsNullOrWhiteSpace(snapshot.DealerMenuGroupId))
                    shopMenusPath = resolver.ResolveShopMenusFile(rootFolderPath, snapshot.DealerMenuGroupId) ?? Path.Combine(rootFolderPath, "ShopMenus.xml");
                else
                    shopMenusPath = Path.Combine(rootFolderPath, "ShopMenus.xml");
            }

            var message =
                "Reading the same way Los Santos RED reads configs: base XML first, then additive +_ XML overrides if the same IDs exist.\r\n" +
                "If an additive file contains the same GangID / GroupID / MenuID, it takes priority and is what the game uses.\r\n" +
                "Resolved sources for this gang:\r\n" +
                "- Gangs: " + Path.GetFileName(gangsPath) + "\r\n" +
                "- DispatchablePeople: " + (string.IsNullOrWhiteSpace(peoplePath) ? "(not resolved)" : Path.GetFileName(peoplePath)) + "\r\n" +
                "- DispatchableVehicles: " + (string.IsNullOrWhiteSpace(vehiclesPath) ? "(not resolved)" : Path.GetFileName(vehiclesPath)) + "\r\n" +
                "- GangTerritories: " + Path.GetFileName(territoriesPath) + "\r\n" +
                "- Locations: " + (string.IsNullOrWhiteSpace(locationsPath) ? "(not resolved)" : Path.GetFileName(locationsPath)) + "\r\n" +
                "- ShopMenus: " + (string.IsNullOrWhiteSpace(shopMenusPath) ? "(not resolved)" : Path.GetFileName(shopMenusPath)) + "\r\n" +
                "- Zones: " + (File.Exists(zonesPath) ? Path.GetFileName(zonesPath) : "(not found)");

            return message;
        }
    }
}
