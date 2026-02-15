using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangRelationshipsCatalogService
    {
        public IReadOnlyList<string> GetEnemyGangIds(string rootFolderPath, string gangId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(gangId))
                return Array.Empty<string>();

            string[]? winner = null;

            foreach (var file in Enumerate(rootFolderPath, "GangRelationships*.xml"))
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

                var gang = doc.Descendants("Gang")
                    .FirstOrDefault(x => string.Equals((string?)x.Element("ID"), gangId, StringComparison.OrdinalIgnoreCase));

                if (gang is null)
                    continue;

                var enemyContainer = gang.Element("EnemyGangs");
                if (enemyContainer is null)
                {
                    winner = Array.Empty<string>();
                    continue;
                }

                var ids = enemyContainer.Elements()
                    .Select(e => (e.Value ?? "").Trim())
                    .Where(v => !string.IsNullOrWhiteSpace(v))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                    .ToArray();

                winner = ids;
            }

            return winner ?? Array.Empty<string>();     
        }

        private static IEnumerable<string> Enumerate(string rootFolderPath, string searchPattern)
        {
            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveGangRelationships(rootFolderPath, "Default");

            return resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase);
        }
    }
}

