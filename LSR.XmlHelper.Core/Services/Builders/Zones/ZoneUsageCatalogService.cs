using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class ZoneUsageCatalogService
    {
        public IReadOnlyDictionary<string, string> GetZoneUsedByDisplay(string rootFolderPath)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return result;

            var gangNameById = LoadGangNames(rootFolderPath);
            var zoneToGangIds = LoadZoneToGangIds(rootFolderPath);

            foreach (var kvp in zoneToGangIds)
            {
                var gangNames = kvp.Value
                    .Select(id => gangNameById.TryGetValue(id, out var name) ? name : id)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                if (gangNames.Length == 0)
                    continue;

                result[kvp.Key] = string.Join(", ", gangNames);
            }

            return result;
        }

        private static Dictionary<string, string> LoadGangNames(string rootFolderPath)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in EnumerateGangsFiles(rootFolderPath))
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                foreach (var gang in doc.Descendants("Gang"))
                {
                    var id = (string?)gang.Element("ID");
                    if (string.IsNullOrWhiteSpace(id))
                        continue;

                    var fullName = (string?)gang.Element("FullName");
                    if (string.IsNullOrWhiteSpace(fullName))
                        fullName = id;

                    dict[id] = fullName;
                }
            }

            return dict;
        }

        private static Dictionary<string, HashSet<string>> LoadZoneToGangIds(string rootFolderPath)
        {
            var dict = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in EnumerateGangTerritoriesFiles(rootFolderPath))
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                var zonesTouchedInThisFile = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                foreach (var territory in doc.Descendants("GangTerritory"))
                {
                    var zone = (string?)territory.Element("ZoneInternalGameName");
                    var gangId = (string?)territory.Element("GangID");

                    if (string.IsNullOrWhiteSpace(zone) || string.IsNullOrWhiteSpace(gangId))
                        continue;

                    if (!zonesTouchedInThisFile.Contains(zone))
                    {
                        dict[zone] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                        zonesTouchedInThisFile.Add(zone);
                    }

                    dict[zone].Add(gangId);
                }
            }

            return dict;
        }

        private static IReadOnlyList<string> EnumerateGangsFiles(string rootFolderPath)
        {
            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveGangs(rootFolderPath, "Default");

            return resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static IReadOnlyList<string> EnumerateGangTerritoriesFiles(string rootFolderPath)
        {
            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveGangTerritories(rootFolderPath, "Default");

            return resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
