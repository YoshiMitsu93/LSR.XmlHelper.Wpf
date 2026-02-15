using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Editing
{
    public sealed class GangTerritoriesWinnerFileByZoneResolverService
    {
        public GangTerritoriesWinnerFileByZoneResolution Resolve(string rootFolderPath, string? configName)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return new GangTerritoriesWinnerFileByZoneResolution(null, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase));

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveGangTerritories(rootFolderPath, configName);

            var winnerByZone = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in resolved.EnumerateReadOrder())
            {
                if (string.IsNullOrWhiteSpace(file) || !File.Exists(file))
                    continue;

                XDocument doc;

                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                var zonesInThisFile = doc
                    .Descendants("GangTerritory")
                    .Select(t => ((string?)t.Element("ZoneInternalGameName") ?? "").Trim())
                    .Where(z => !string.IsNullOrWhiteSpace(z))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                foreach (var z in zonesInThisFile)
                    winnerByZone[z] = file;
            }

            var fallback = resolved.BasePath;

            if (string.IsNullOrWhiteSpace(fallback))
                fallback = resolved.AdditivePaths != null && resolved.AdditivePaths.Count > 0 ? resolved.AdditivePaths.LastOrDefault() : null;

            if (string.IsNullOrWhiteSpace(fallback))
                fallback = Path.Combine(rootFolderPath, "GangTerritories.xml");

            return new GangTerritoriesWinnerFileByZoneResolution(fallback, winnerByZone);
        }
    }

    public sealed class GangTerritoriesWinnerFileByZoneResolution
    {
        public GangTerritoriesWinnerFileByZoneResolution(string? fallbackPath, IReadOnlyDictionary<string, string> winnerFileByZone)
        {
            FallbackPath = string.IsNullOrWhiteSpace(fallbackPath) ? null : fallbackPath;
            WinnerFileByZone = winnerFileByZone ?? new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        public string? FallbackPath { get; }
        public IReadOnlyDictionary<string, string> WinnerFileByZone { get; }
    }
}
