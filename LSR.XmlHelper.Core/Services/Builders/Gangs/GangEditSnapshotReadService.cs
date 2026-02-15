using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangEditSnapshotReadService
    {
        public GangEditSnapshot? TryGet(string rootFolderPath, string gangId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath))
                return null;

            if (!Directory.Exists(rootFolderPath))
                return null;

            if (string.IsNullOrWhiteSpace(gangId))
                return null;

            var resolver = new LSR.XmlHelper.Core.Services.LsrConfigFileResolverService();
            var resolvedPath = resolver.ResolveGangFile(rootFolderPath, gangId);

            if (string.IsNullOrWhiteSpace(resolvedPath) || !File.Exists(resolvedPath))
                return null;

            XDocument doc;

            try
            {
                doc = XDocument.Load(resolvedPath, LoadOptions.None);
            }
            catch
            {
                return null;
            }

            var gang = doc
                .Descendants("Gang")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), gangId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (gang is null)
                return null;

            var fullName = ((string?)gang.Element("FullName") ?? "").Trim();

            var peopleGroupId = (((string?)gang.Element("PeopleGroupID") ?? "").Trim());
            if (string.IsNullOrWhiteSpace(peopleGroupId))
                peopleGroupId = (((string?)gang.Element("PersonnelID") ?? "").Trim());

            var vehicleGroupId = (((string?)gang.Element("VehicleGroupID") ?? "").Trim());
            if (string.IsNullOrWhiteSpace(vehicleGroupId))
                vehicleGroupId = (((string?)gang.Element("VehiclesID") ?? "").Trim());

            var dealerMenuGroupId = (((string?)gang.Element("DealerMenuGroupID") ?? "").Trim());
            if (string.IsNullOrWhiteSpace(dealerMenuGroupId))
                dealerMenuGroupId = (((string?)gang.Element("DealerMenuGroup") ?? "").Trim());

            var meleeWeaponsId = ((string?)gang.Element("MeleeWeaponsID") ?? "").Trim();
            var sideArmsId = ((string?)gang.Element("SideArmsID") ?? "").Trim();
            var longGunsId = ((string?)gang.Element("LongGunsID") ?? "").Trim();

            return new GangEditSnapshot(
                gangId.Trim(),
                fullName,
                peopleGroupId,
                vehicleGroupId,
                dealerMenuGroupId,
                meleeWeaponsId,
                sideArmsId,
                longGunsId);
        }
    }
}
