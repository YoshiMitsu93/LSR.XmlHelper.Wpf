using LSR.XmlHelper.Wpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LSR.XmlHelper.Wpf.Services.Guides
{
    public sealed class XmlGuidePackService
    {
        public void ExportPack(string packPath, XmlGuide guide, XmlGuideStoreService store)
        {
            if (guide is null)
                throw new ArgumentNullException(nameof(guide));

            if (store is null)
                throw new ArgumentNullException(nameof(store));

            if (string.IsNullOrWhiteSpace(packPath))
                throw new ArgumentException("Pack path is required.", nameof(packPath));

            if (File.Exists(packPath))
                File.Delete(packPath);

            using var zip = ZipFile.Open(packPath, ZipArchiveMode.Create);

            var payload = new XmlGuide
            {
                Id = guide.Id,
                Title = guide.Title ?? "",
                Category = guide.Category ?? "Uncategorized",
                Summary = guide.Summary ?? "",
                Body = guide.Body ?? "",
                CreatedUtc = guide.CreatedUtc,
                UpdatedUtc = guide.UpdatedUtc
            };

            var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
            var guideEntry = zip.CreateEntry("guide.json");

            using (var s = guideEntry.Open())
            using (var w = new StreamWriter(s))
                w.Write(json);

            var files = XmlGuideImageTokenParser.ExtractImageFiles(payload.Body);

            foreach (var file in files)
            {
                var path = store.TryGetImagePath(payload.Id, file);
                if (path is null)
                    continue;

                zip.CreateEntryFromFile(path, "images/" + Path.GetFileName(file));
            }
        }

        public ImportedGuidePack ImportPack(string packPath)
        {
            if (string.IsNullOrWhiteSpace(packPath))
                throw new ArgumentException("Pack path is required.", nameof(packPath));

            using var zip = ZipFile.OpenRead(packPath);

            var guideEntry = zip.GetEntry("guide.json");
            if (guideEntry is null)
                throw new InvalidOperationException("guide.json not found in pack.");

            XmlGuide guide;
            using (var s = guideEntry.Open())
            using (var r = new StreamReader(s))
                guide = JsonSerializer.Deserialize<XmlGuide>(r.ReadToEnd()) ?? new XmlGuide();

            var images = new Dictionary<string, byte[]>(StringComparer.OrdinalIgnoreCase);

            foreach (var entry in zip.Entries.Where(e => e.FullName.StartsWith("images/", StringComparison.OrdinalIgnoreCase)))
            {
                var name = Path.GetFileName(entry.FullName);
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                using var s = entry.Open();
                using var ms = new MemoryStream();
                s.CopyTo(ms);
                images[name] = ms.ToArray();
            }

            return new ImportedGuidePack(guide, images);
        }
    }

    public sealed record ImportedGuidePack(XmlGuide Guide, IReadOnlyDictionary<string, byte[]> Images);

    internal static class XmlGuideImageTokenParser
    {
        private static readonly Regex Token = new Regex(@"\[\[img:(?<f>[^\]]+)\]\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        public static string[] ExtractImageFiles(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return Array.Empty<string>();

            var matches = Token.Matches(body);

            return matches
                .Select(m => (m.Groups["f"].Value ?? "").Trim())
                .Select(x => Path.GetFileName(x) ?? "")
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
