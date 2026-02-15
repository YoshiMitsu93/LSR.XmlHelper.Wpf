using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangPackExportService
    {
        public (bool ok, string message, string exportFolderPath, IReadOnlyList<string> exportedFiles) Export(
            string rootFolderPath,
            string packName)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (false, "Root folder is missing or invalid.", "", Array.Empty<string>());

            if (string.IsNullOrWhiteSpace(packName))
                return (false, "Pack name is required.", "", Array.Empty<string>());

            var candidates = new[]
            {
                $"Gangs+_{packName}.xml",
                $"DispatchablePeople+_{packName}.xml",
                $"DispatchableVehicles+_{packName}.xml",
                $"GangTerritories+_{packName}.xml",
                $"Locations+_{packName}.xml",
                $"ShopMenus+_{packName}.xml",
                $"IssuableWeapons+_{packName}.xml"
            };

            var existing = candidates
                .Select(f => Path.Combine(rootFolderPath, f))
                .Where(File.Exists)
                .ToArray();

            if (existing.Length == 0)
                return (false, "No generated +_PackName.xml files were found to export. Build Pack first.", "", Array.Empty<string>());

            var packsRoot = Path.Combine(rootFolderPath, "LSR-XML-Helper", "GangPacks");
            Directory.CreateDirectory(packsRoot);

            var exportFolderPath = Path.Combine(packsRoot, packName);
            if (Directory.Exists(exportFolderPath))
            {
                var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                exportFolderPath = Path.Combine(packsRoot, $"{packName}_{stamp}");
            }

            Directory.CreateDirectory(exportFolderPath);

            var exported = new List<string>();

            foreach (var srcPath in existing)
            {
                var fileName = Path.GetFileName(srcPath);
                var destPath = Path.Combine(exportFolderPath, fileName);
                File.Copy(srcPath, destPath, true);
                exported.Add(fileName);
                foreach (var copiedImage in CopyReferencedGangBannerImages(rootFolderPath, exportFolderPath, packName))
                    exported.Add(copiedImage);
            }

            var readmePath = Path.Combine(exportFolderPath, "README.txt");
            File.WriteAllText(readmePath, BuildReadme(rootFolderPath, packName, exported), System.Text.Encoding.UTF8);
            exported.Add("README.txt");

            return (true, "OK", exportFolderPath, exported);
        }

        private static string BuildReadme(string rootFolderPath, string packName, IReadOnlyList<string> exportedFiles)
        {
            var lines = new List<string>
            {
                $"Los Santos RED - Gang Pack Export",
                $"PackName: {packName}",
                $"ExportedAt: {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                "",
                "Files:",
            };

            foreach (var f in exportedFiles.OrderBy(x => x, StringComparer.OrdinalIgnoreCase))
                lines.Add($"- {f}");

            lines.Add("");
            lines.Add("Summary (from generated files if available):");

            var gangsPath = Path.Combine(rootFolderPath, $"Gangs+_{packName}.xml");
            if (File.Exists(gangsPath))
            {
                try
                {
                    var doc = XDocument.Load(gangsPath, LoadOptions.None);
                    var gang = doc.Descendants("Gang").FirstOrDefault();

                    if (gang is not null)
                    {
                        lines.Add($"GangID: {GetValue(gang, "ID")}");
                        lines.Add($"FullName: {GetValue(gang, "FullName")}");
                        lines.Add($"PersonnelID: {GetValue(gang, "PersonnelID")}");
                        lines.Add($"VehiclesID: {GetValue(gang, "VehiclesID")}");
                        lines.Add($"DealerMenuGroup: {GetValue(gang, "DealerMenuGroup")}");
                        lines.Add($"MeleeWeaponsID: {GetValue(gang, "MeleeWeaponsID")}");
                        lines.Add($"SideArmsID: {GetValue(gang, "SideArmsID")}");
                        lines.Add($"LongGunsID: {GetValue(gang, "LongGunsID")}");

                        var enemies = gang.Element("EnemyGangs")?.Elements()
                            .Select(e => (e.Value ?? "").Trim())
                            .Where(v => !string.IsNullOrWhiteSpace(v))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                            .ToArray() ?? Array.Empty<string>();

                        lines.Add($"EnemyGangs: {(enemies.Length == 0 ? "None" : string.Join(", ", enemies))}");
                    }
                }
                catch
                {
                    lines.Add("Gangs summary: Failed to parse Gangs+ file.");
                }
            }
            else
            {
                lines.Add("Gangs summary: Gangs+ file not found.");
            }

            var territoriesPath = Path.Combine(rootFolderPath, $"GangTerritories+_{packName}.xml");
            if (File.Exists(territoriesPath))
            {
                try
                {
                    var doc = XDocument.Load(territoriesPath, LoadOptions.None);
                    var zones = doc.Descendants("GangTerritory")
                        .Select(x => (string?)x.Element("ZoneInternalGameName"))
                        .Where(v => !string.IsNullOrWhiteSpace(v))
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .OrderBy(v => v, StringComparer.OrdinalIgnoreCase)
                        .ToArray();

                    lines.Add($"TerritoryZones: {(zones.Length == 0 ? "None" : string.Join(", ", zones))}");
                }
                catch
                {
                    lines.Add("TerritoryZones: Failed to parse GangTerritories+ file.");
                }
            }

            lines.Add("");
            lines.Add("Install:");
            lines.Add("Copy the exported XML files into your LSR config folder (or use a mod manager).");
            lines.Add("Uninstall:");
            lines.Add("Remove the exported XML files.");

            return string.Join(Environment.NewLine, lines);
        }
        private static IReadOnlyList<string> CopyReferencedGangBannerImages(string rootFolderPath, string exportFolderPath, string packName)
        {
            var copied = new List<string>();

            var locationsPath = Path.Combine(rootFolderPath, $"Locations+_{packName}.xml");
            if (!File.Exists(locationsPath))
                return copied;

            XDocument doc;
            try
            {
                doc = XDocument.Load(locationsPath, LoadOptions.None);
            }
            catch
            {
                return copied;
            }

            var bannerPaths = doc.Descendants("BannerImagePath")
                .Select(x => ((string?)x ?? "").Trim())
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            foreach (var bannerRelRaw in bannerPaths)
            {
                var bannerRel = bannerRelRaw.Replace("/", "\\").TrimStart('\\');
                if (Path.IsPathRooted(bannerRel))
                    continue;

                if (bannerRel.StartsWith("images\\", StringComparison.OrdinalIgnoreCase))
                    bannerRel = bannerRel.Substring("images\\".Length);

                var src = Path.Combine(rootFolderPath, "images", bannerRel);
                if (!File.Exists(src))
                    continue;

                var dest = Path.Combine(exportFolderPath, "images", bannerRel);
                var destDir = Path.GetDirectoryName(dest);
                if (!string.IsNullOrWhiteSpace(destDir))
                    Directory.CreateDirectory(destDir);

                File.Copy(src, dest, true);
                copied.Add(Path.Combine("images", bannerRel));
            }

            return copied;
        }

        private static string GetValue(XElement parent, string elementName)
        {
            return (string?)parent.Element(elementName) ?? "";
        }
    }
}
