using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class DispatchableVehicleVariantReadService
    {
        public XElement? TryReadVehicleByVariantKey(string rootFolderPath, string modelName, string variantKey)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return null;

            if (string.IsNullOrWhiteSpace(modelName) || string.IsNullOrWhiteSpace(variantKey))
                return null;

            var resolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();
            var resolved = resolver.ResolveDispatchableVehicles(rootFolderPath, "Default");

            var files = resolved.EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

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
                    if (!string.Equals(key, variantKey.Trim(), StringComparison.OrdinalIgnoreCase))
                        continue;

                    return new XElement(dv);
                }
            }

            return null;
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
