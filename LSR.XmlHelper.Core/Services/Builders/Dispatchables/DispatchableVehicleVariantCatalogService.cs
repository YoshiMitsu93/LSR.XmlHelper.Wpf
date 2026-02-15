using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class DispatchableVehicleVariantCatalogService
    {
        public IReadOnlyList<(string VariantKey, string DisplayText)> GetVariantsForModel(string rootFolderPath, string modelName)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return Array.Empty<(string VariantKey, string DisplayText)>();

            if (string.IsNullOrWhiteSpace(modelName))
                return Array.Empty<(string VariantKey, string DisplayText)>();

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveDispatchableVehicles(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var variants = new List<(string VariantKey, string DisplayText)>();
            var seenKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
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

                foreach (var dv in doc.Descendants("DispatchableVehicle"))
                {
                    var mn = (((string?)dv.Element("ModelName") ?? "").Trim());
                    if (!string.Equals(mn, modelName.Trim(), StringComparison.OrdinalIgnoreCase))
                        continue;

                    var key = ComputeKey(dv);

                    if (!seenKeys.Add(key))
                        continue;

                    var display = BuildDisplayText(dv, file);
                    variants.Add((key, display));
                }
            }

            return variants
                .OrderBy(x => x.DisplayText, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static string BuildDisplayText(XElement dv, string sourceFile)
        {
            var minOcc = ((string?)dv.Element("MinOccupants") ?? "").Trim();
            var maxOcc = ((string?)dv.Element("MaxOccupants") ?? "").Trim();
            var pri = ((string?)dv.Element("RequiredPrimaryColorID") ?? "").Trim();
            var sec = ((string?)dv.Element("RequiredSecondaryColorID") ?? "").Trim();

            var liveryCount = 0;
            var reqLiv = dv.Element("RequiredLiveries");
            if (reqLiv is not null)
                liveryCount = reqLiv.Elements().Count();

            var extrasCount = 0;
            var extras = dv.Element("VehicleExtras");
            if (extras is not null)
                extrasCount = extras.Elements().Count();

            var modsCount = 0;
            var mods = dv.Element("VehicleMods");
            if (mods is not null)
                modsCount = mods.Elements().Count();

            var fileName = Path.GetFileName(sourceFile);

            return $"Occ {minOcc}-{maxOcc} | Colors {pri}/{sec} | Liveries {liveryCount} | Extras {extrasCount} | Mods {modsCount} | {fileName}";
        }

        private static string ComputeKey(XElement dv)
        {
            var raw = dv.ToString(SaveOptions.DisableFormatting);
            var bytes = Encoding.UTF8.GetBytes(raw);

            using var sha1 = SHA1.Create();
            var hash = sha1.ComputeHash(bytes);

            var sb = new StringBuilder(hash.Length * 2);
            for (var i = 0; i < hash.Length; i++)
                sb.Append(hash[i].ToString("x2"));

            return sb.ToString();
        }
    }
}
