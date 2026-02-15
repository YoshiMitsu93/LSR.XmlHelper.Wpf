using LSR.XmlHelper.Wpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace LSR.XmlHelper.Wpf.Services.Blips
{
    public static class BlipSpriteReferenceProvider
    {
        private const string BlipsUrl = "https://docs.fivem.net/docs/game-references/blips/";

        public static List<BlipSpriteOption> GetFallbackBlips()
        {
            return new List<BlipSpriteOption>
            {
                new BlipSpriteOption("378", "Skull (378)"),
                new BlipSpriteOption("84", "Rampage (84)"),
                new BlipSpriteOption("110", "Ammu-Nation (110)"),
                new BlipSpriteOption("52", "Store (52)"),
                new BlipSpriteOption("162", "Point of interest (162)"),
                new BlipSpriteOption("431", "Dollar sign circled (431)")
            };
        }

        public static List<BlipSpriteOption> LoadAllBlips()
        {
            var cachePath = GetCachePath();
            var cached = TryReadCache(cachePath);
            if (cached.Count > 0)
                return cached;

            var downloaded = TryDownloadAndParse();
            if (downloaded.Count > 0)
            {
                TryWriteCache(cachePath, downloaded);
                return downloaded;
            }

            return GetFallbackBlips();
        }

        private static List<BlipSpriteOption> TryDownloadAndParse()
        {
            try
            {
                using var http = new HttpClient();
                http.Timeout = TimeSpan.FromSeconds(15);

                var html = http.GetStringAsync(BlipsUrl).GetAwaiter().GetResult();
                if (string.IsNullOrWhiteSpace(html))
                    return new List<BlipSpriteOption>();

                var noTags = Regex.Replace(html, "<.*?>", "\n");
                noTags = Regex.Replace(noTags, @"\r", "");
                noTags = Regex.Replace(noTags, @"\n{2,}", "\n");

                var results = new Dictionary<int, string>();

                var pattern = new Regex(@"(?m)^\s*(\d{1,4})\s*$\n^\s*([a-z0-9_]+)\s*$", RegexOptions.IgnoreCase);
                foreach (Match m in pattern.Matches(noTags))
                {
                    if (!int.TryParse(m.Groups[1].Value, out var id))
                        continue;

                    var name = m.Groups[2].Value?.Trim() ?? "";
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    results[id] = name;
                }

                return results
                    .OrderBy(k => k.Key)
                    .Select(k => new BlipSpriteOption(k.Key.ToString(), $"{k.Value} ({k.Key})"))
                    .ToList();

            }
            catch
            {
                return new List<BlipSpriteOption>();
            }
        }

        private static List<BlipSpriteOption> TryReadCache(string cachePath)
        {
            try
            {
                if (!File.Exists(cachePath))
                    return new List<BlipSpriteOption>();

                var json = File.ReadAllText(cachePath);
                if (string.IsNullOrWhiteSpace(json))
                    return new List<BlipSpriteOption>();

                var entries = JsonSerializer.Deserialize<List<CacheEntry>>(json);
                if (entries is null)
                    return new List<BlipSpriteOption>();

                return entries
                    .Where(e => !string.IsNullOrWhiteSpace(e.Id) && !string.IsNullOrWhiteSpace(e.Name))
                    .Select(e => new BlipSpriteOption(e.Id, $"{e.Name} ({e.Id})"))
                    .ToList();

            }
            catch
            {
                return new List<BlipSpriteOption>();
            }
        }

        private static void TryWriteCache(string cachePath, List<BlipSpriteOption> blips)
        {
            try
            {
                var dir = Path.GetDirectoryName(cachePath);
                if (!string.IsNullOrWhiteSpace(dir))
                    Directory.CreateDirectory(dir);

                var entries = blips
                    .Select(b => new CacheEntry { Id = b.Value, Name = ExtractName(b.DisplayText, b.Value) })
                    .ToList();

                var json = JsonSerializer.Serialize(entries);
                File.WriteAllText(cachePath, json);
            }
            catch
            {
            }
        }

        private static string ExtractName(string displayText, string id)
        {
            var suffix = $"({id})";
            var index = displayText.LastIndexOf(suffix, StringComparison.Ordinal);
            if (index <= 0)
                return displayText.Trim();

            return displayText.Substring(0, index).Trim();
        }

        private static string GetCachePath()
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "LSR.XmlHelper.Wpf", "Cache", "blips_cache.json");
        }

        private sealed class CacheEntry
        {
            public string Id { get; set; } = "";
            public string Name { get; set; } = "";
        }
    }
}
