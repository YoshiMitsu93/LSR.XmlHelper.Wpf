using LSR.XmlHelper.Wpf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Windows.Media.Imaging;

namespace LSR.XmlHelper.Wpf.Services.Guides
{
    public sealed class XmlGuideStoreService
    {
        private readonly string _guidesFolder;
        private readonly string _communityFolder;
        private readonly string _userGuidesFolder;
        private readonly string _appGuidesFolder;
        private readonly Dictionary<string, string> _guideFolderById = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public XmlGuideStoreService()
            : this(Path.Combine(Directory.GetCurrentDirectory(), "LSR-XML-Helper"), Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Guides"))
        {
        }

        public XmlGuideStoreService(string helperRootFolder)
            : this(helperRootFolder, Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Guides"))
        {
        }

        public XmlGuideStoreService(string helperRootFolder, string appGuidesFolder)
        {
            if (string.IsNullOrWhiteSpace(helperRootFolder))
                helperRootFolder = Path.Combine(Directory.GetCurrentDirectory(), "LSR-XML-Helper");

            _guidesFolder = Path.Combine(helperRootFolder, "Guides");
            _communityFolder = Path.Combine(_guidesFolder, "CommunityGuides");
            _userGuidesFolder = Path.Combine(_guidesFolder, "UserGuides");
            _appGuidesFolder = appGuidesFolder ?? "";

            Directory.CreateDirectory(_guidesFolder);
            Directory.CreateDirectory(_communityFolder);
            Directory.CreateDirectory(_userGuidesFolder);
        }

        public string GuidesFolder => _guidesFolder;
        private void InstallBuiltInGuidesFromAppFolder()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_appGuidesFolder))
                    return;

                var packsFolder = Path.Combine(_appGuidesFolder, "Packs");
                if (Directory.Exists(packsFolder))
                {
                    var packService = new XmlGuidePackService();

                    foreach (var packPath in Directory.GetFiles(packsFolder, "*.lsrguidepack", SearchOption.TopDirectoryOnly))
                    {
                        var imported = packService.ImportPack(packPath);
                        var guide = NormalizeImportedGuide(imported.Guide);
                        guide.IsBuiltIn = true;

                        UpsertGuideFolder(_communityFolder, guide, true);

                        foreach (var kv in imported.Images)
                            SaveImportedImageToGuide(guide.Id, kv.Key, kv.Value);
                    }
                }

                var legacyBuiltIn = Path.Combine(_appGuidesFolder, "BuiltIn");
                if (Directory.Exists(legacyBuiltIn))
                {
                    foreach (var file in Directory.GetFiles(legacyBuiltIn, "*.json", SearchOption.TopDirectoryOnly))
                    {
                        var json = File.ReadAllText(file);
                        var asList = TryDeserialize<List<XmlGuide>>(json);
                        if (asList is not null)
                        {
                            foreach (var g in asList)
                            {
                                var guide = NormalizeImportedGuide(g);
                                guide.IsBuiltIn = true;
                                UpsertGuideFolder(_communityFolder, guide, true);
                            }
                            continue;
                        }

                        var asOne = TryDeserialize<XmlGuide>(json);
                        if (asOne is null)
                            continue;

                        var one = NormalizeImportedGuide(asOne);
                        one.IsBuiltIn = true;
                        UpsertGuideFolder(_communityFolder, one, true);
                    }
                }
            }
            catch
            {
            }
        }

        private void MigrateLegacyUserGuidesFile()
        {
            try
            {
                var legacyBuiltInFolder = Path.Combine(_guidesFolder, "BuiltIn");
                var legacyUserFolder = Path.Combine(_guidesFolder, "User");
                var legacyImagesFolder = Path.Combine(_guidesFolder, "Images");
                var legacyListPath = Path.Combine(_guidesFolder, "UserGuides.json");

                if (Directory.Exists(legacyBuiltInFolder))
                {
                    foreach (var g in LoadLegacyJsonGuidesFromFolder(legacyBuiltInFolder))
                    {
                        var guide = NormalizeImportedGuide(g);
                        guide.IsBuiltIn = true;
                        UpsertGuideFolder(_communityFolder, guide, true);
                        CopyLegacyImagesIfAny(legacyImagesFolder, guide.Id);
                    }
                }

                if (Directory.Exists(legacyUserFolder))
                {
                    foreach (var g in LoadLegacyJsonGuidesFromFolder(legacyUserFolder))
                    {
                        var guide = NormalizeImportedGuide(g);
                        guide.IsBuiltIn = false;
                        UpsertGuideFolder(_userGuidesFolder, guide, false);
                        CopyLegacyImagesIfAny(legacyImagesFolder, guide.Id);
                    }
                }

                if (File.Exists(legacyListPath))
                {
                    var json = File.ReadAllText(legacyListPath);
                    var legacy = TryDeserialize<List<XmlGuide>>(json);
                    if (legacy is not null)
                    {
                        foreach (var g in legacy)
                        {
                            var guide = NormalizeImportedGuide(g);
                            guide.IsBuiltIn = false;
                            UpsertGuideFolder(_userGuidesFolder, guide, false);
                        }
                    }
                }

                try
                {
                    if (File.Exists(legacyListPath))
                        File.Delete(legacyListPath);

                    if (Directory.Exists(legacyBuiltInFolder))
                        Directory.Delete(legacyBuiltInFolder, true);

                    if (Directory.Exists(legacyUserFolder))
                        Directory.Delete(legacyUserFolder, true);

                    if (Directory.Exists(legacyImagesFolder))
                        Directory.Delete(legacyImagesFolder, true);
                }
                catch
                {
                }
            }
            catch
            {
            }
        }

        private IEnumerable<XmlGuide> LoadLegacyJsonGuidesFromFolder(string folder)
        {
            var results = new List<XmlGuide>();

            try
            {
                foreach (var file in Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly))
                {
                    var json = File.ReadAllText(file);

                    var asList = TryDeserialize<List<XmlGuide>>(json);
                    if (asList is not null)
                    {
                        results.AddRange(asList);
                        continue;
                    }

                    var asOne = TryDeserialize<XmlGuide>(json);
                    if (asOne is not null)
                        results.Add(asOne);
                }
            }
            catch
            {
            }

            return results;
        }

        private void CopyLegacyImagesIfAny(string legacyImagesFolder, string guideId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(guideId))
                    return;

                var source = Path.Combine(legacyImagesFolder, guideId);
                if (!Directory.Exists(source))
                    return;

                var dest = GetImagesFolder(guideId);
                Directory.CreateDirectory(dest);

                foreach (var img in Directory.GetFiles(source, "*.*", SearchOption.TopDirectoryOnly))
                {
                    var destPath = Path.Combine(dest, Path.GetFileName(img));
                    File.Copy(img, destPath, true);
                }
            }
            catch
            {
            }
        }

        public string GetImagesFolder(string guideId)
        {
            var folder = EnsureGuideFolderKnown(guideId);
            return Path.Combine(folder, "Images");
        }

        private string EnsureGuideFolderKnown(string guideId)
        {
            if (string.IsNullOrWhiteSpace(guideId))
                return _userGuidesFolder;

            if (_guideFolderById.TryGetValue(guideId, out var existing) && Directory.Exists(existing))
                return existing;

            var found = FindGuideFolderById(_userGuidesFolder, guideId);
            if (string.IsNullOrWhiteSpace(found))
                found = FindGuideFolderById(_communityFolder, guideId);

            if (!string.IsNullOrWhiteSpace(found))
            {
                _guideFolderById[guideId] = found;
                return found;
            }

            var fallback = Path.Combine(_userGuidesFolder, "Guide__" + guideId.Substring(0, Math.Min(8, guideId.Length)));
            _guideFolderById[guideId] = fallback;
            return fallback;
        }

        private static string? FindGuideFolderById(string root, string guideId)
        {
            try
            {
                if (!Directory.Exists(root))
                    return null;

                foreach (var dir in Directory.GetDirectories(root, "*", SearchOption.TopDirectoryOnly))
                {
                    var guidePath = Path.Combine(dir, "guide.json");
                    if (!File.Exists(guidePath))
                        continue;

                    var json = File.ReadAllText(guidePath);
                    var g = TryDeserialize<XmlGuide>(json);
                    if (g is null)
                        continue;

                    if (string.Equals(g.Id, guideId, StringComparison.OrdinalIgnoreCase))
                        return dir;
                }
            }
            catch
            {
            }

            return null;
        }

        private string UpsertGuideFolder(string root, XmlGuide guide, bool isBuiltIn)
        {
            Directory.CreateDirectory(root);

            var desiredFolderName = MakeGuideFolderName(guide.Title, guide.Id);
            var desiredFolder = Path.Combine(root, desiredFolderName);

            var currentFolder = EnsureGuideFolderKnown(guide.Id);
            if (!string.IsNullOrWhiteSpace(currentFolder) && currentFolder.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                if (!string.Equals(currentFolder, desiredFolder, StringComparison.OrdinalIgnoreCase) && Directory.Exists(currentFolder) && !Directory.Exists(desiredFolder))
                {
                    Directory.Move(currentFolder, desiredFolder);
                    currentFolder = desiredFolder;
                }
            }
            else
            {
                currentFolder = desiredFolder;
            }

            Directory.CreateDirectory(currentFolder);

            var toWrite = new XmlGuide
            {
                Id = guide.Id,
                Title = guide.Title ?? "",
                Category = guide.Category ?? "Uncategorized",
                Summary = guide.Summary ?? "",
                Body = guide.Body ?? "",
                CreatedUtc = guide.CreatedUtc,
                UpdatedUtc = guide.UpdatedUtc,
                IsBuiltIn = isBuiltIn
            };

            var json = JsonSerializer.Serialize(toWrite, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(Path.Combine(currentFolder, "guide.json"), json);

            _guideFolderById[guide.Id] = currentFolder;
            return currentFolder;
        }

        private static string MakeGuideFolderName(string? title, string id)
        {
            var safeTitle = MakeSafeFileName(string.IsNullOrWhiteSpace(title) ? "Guide" : title.Trim());
            var shortId = string.IsNullOrWhiteSpace(id) ? Guid.NewGuid().ToString("N").Substring(0, 8) : id.Trim().Substring(0, Math.Min(8, id.Trim().Length));
            return safeTitle + "__" + shortId;
        }

        public string AddImageToGuide(string guideId, string sourceFilePath)
        {
            var imagesFolder = GetImagesFolder(guideId);
            Directory.CreateDirectory(imagesFolder);

            var ext = Path.GetExtension(sourceFilePath);
            if (string.IsNullOrWhiteSpace(ext))
                ext = ".png";

            var name = Guid.NewGuid().ToString("N") + ext.ToLowerInvariant();
            var destPath = Path.Combine(imagesFolder, name);

            File.Copy(sourceFilePath, destPath, true);

            return name;
        }

        public void SaveImportedImageToGuide(string guideId, string fileName, byte[] bytes)
        {
            var imagesFolder = GetImagesFolder(guideId);
            Directory.CreateDirectory(imagesFolder);

            var safe = Path.GetFileName(fileName);
            var destPath = Path.Combine(imagesFolder, safe);

            File.WriteAllBytes(destPath, bytes);
        }

        public string AddPastedImageToGuide(string guideId, BitmapSource image)
        {
            var imagesFolder = GetImagesFolder(guideId);
            Directory.CreateDirectory(imagesFolder);

            var name = Guid.NewGuid().ToString("N") + ".png";
            var destPath = Path.Combine(imagesFolder, name);

            using (var stream = File.Create(destPath))
            {
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(stream);
            }

            return name;
        }
        public void CopyImageBetweenGuides(string fromGuideId, string toGuideId, string fileName)
        {
            var fromPath = Path.Combine(GetImagesFolder(fromGuideId), fileName);
            if (!File.Exists(fromPath))
                return;

            var toFolder = GetImagesFolder(toGuideId);
            Directory.CreateDirectory(toFolder);

            var toPath = Path.Combine(toFolder, fileName);
            File.Copy(fromPath, toPath, true);
        }

        public string? TryGetImagePath(string guideId, string fileName)
        {
            var imagesFolder = GetImagesFolder(guideId);
            var path = Path.Combine(imagesFolder, fileName);

            return File.Exists(path) ? path : null;
        }

        public IReadOnlyList<XmlGuide> LoadAll()
        {
            Directory.CreateDirectory(_guidesFolder);
            Directory.CreateDirectory(_communityFolder);
            Directory.CreateDirectory(_userGuidesFolder);

            _guideFolderById.Clear();

            InstallBuiltInGuidesFromAppFolder();
            MigrateLegacyUserGuidesFile();

            var builtIn = LoadBuiltInGuides();
            var user = LoadUserGuides();

            var merged = new Dictionary<string, XmlGuide>(StringComparer.OrdinalIgnoreCase);

            foreach (var g in builtIn)
                merged[g.Id] = g;

            foreach (var g in user)
                merged[g.Id] = g;

            return merged.Values
                .OrderBy(g => g.Category, StringComparer.OrdinalIgnoreCase)
                .ThenBy(g => g.Title, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        public void SaveUserGuides(IEnumerable<XmlGuide> guides)
        {
            Directory.CreateDirectory(_userGuidesFolder);

            var toSave = guides
                .Where(g => !g.IsBuiltIn)
                .Select(g => new XmlGuide
                {
                    Id = string.IsNullOrWhiteSpace(g.Id) ? Guid.NewGuid().ToString("N") : g.Id.Trim(),
                    Title = g.Title ?? "",
                    Category = g.Category ?? "Uncategorized",
                    Summary = g.Summary ?? "",
                    Body = g.Body ?? "",
                    CreatedUtc = g.CreatedUtc,
                    UpdatedUtc = g.UpdatedUtc,
                    IsBuiltIn = false
                })
                .ToList();

            var keepIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var guide in toSave)
            {
                UpsertGuideFolder(_userGuidesFolder, guide, false);
                keepIds.Add(guide.Id);
            }

            foreach (var dir in Directory.GetDirectories(_userGuidesFolder, "*", SearchOption.TopDirectoryOnly))
            {
                var guidePath = Path.Combine(dir, "guide.json");
                if (!File.Exists(guidePath))
                    continue;

                var json = File.ReadAllText(guidePath);
                var g = TryDeserialize<XmlGuide>(json);
                if (g is null)
                    continue;

                if (!keepIds.Contains(g.Id))
                {
                    try
                    {
                        Directory.Delete(dir, true);
                    }
                    catch
                    {
                    }
                }
            }
        }

        public XmlGuide NormalizeImportedGuide(XmlGuide guide)
        {
            guide.Id = string.IsNullOrWhiteSpace(guide.Id) ? Guid.NewGuid().ToString("N") : guide.Id.Trim();
            guide.Title = (guide.Title ?? "").Trim();
            guide.Category = string.IsNullOrWhiteSpace(guide.Category) ? "Uncategorized" : guide.Category.Trim();
            guide.Summary = guide.Summary ?? "";
            guide.Body = guide.Body ?? "";
            guide.IsBuiltIn = false;

            if (guide.CreatedUtc == default)
                guide.CreatedUtc = DateTimeOffset.UtcNow;

            guide.UpdatedUtc = DateTimeOffset.UtcNow;

            return guide;
        }

        private List<XmlGuide> LoadUserGuides()
        {
            return LoadGuidesFromFolder(_userGuidesFolder, false);
        }

        private List<XmlGuide> LoadGuidesFromFolder(string folder, bool isBuiltIn)
        {
            var results = new List<XmlGuide>();

            try
            {
                if (!Directory.Exists(folder))
                    return results;

                foreach (var dir in Directory.GetDirectories(folder, "*", SearchOption.TopDirectoryOnly))
                {
                    var guidePath = Path.Combine(dir, "guide.json");
                    if (!File.Exists(guidePath))
                        continue;

                    var json = File.ReadAllText(guidePath);
                    var g = TryDeserialize<XmlGuide>(json);
                    if (g is null)
                        continue;

                    g.IsBuiltIn = isBuiltIn;
                    results.Add(g);

                    if (!string.IsNullOrWhiteSpace(g.Id))
                        _guideFolderById[g.Id.Trim()] = dir;
                }

                foreach (var file in Directory.GetFiles(folder, "*.json", SearchOption.TopDirectoryOnly))
                {
                    if (string.Equals(Path.GetFileName(file), "guide.json", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var json = File.ReadAllText(file);

                    var asList = TryDeserialize<List<XmlGuide>>(json);
                    if (asList is not null)
                    {
                        foreach (var g in asList)
                        {
                            g.IsBuiltIn = isBuiltIn;
                            results.Add(g);
                            UpsertGuideFolder(folder, NormalizeImportedGuide(g), isBuiltIn);
                        }

                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                        }
                        continue;
                    }

                    var asOne = TryDeserialize<XmlGuide>(json);
                    if (asOne is not null)
                    {
                        asOne.IsBuiltIn = isBuiltIn;
                        results.Add(asOne);
                        UpsertGuideFolder(folder, NormalizeImportedGuide(asOne), isBuiltIn);

                        try
                        {
                            File.Delete(file);
                        }
                        catch
                        {
                        }
                    }
                }
            }
            catch
            {
            }

            return results;
        }

        private static string MakeGuideFileName(XmlGuide guide)
        {
            var title = MakeSafeFileName(guide.Title);
            var id = (guide.Id ?? "").Trim();
            if (string.IsNullOrWhiteSpace(id))
                id = Guid.NewGuid().ToString("N");

            if (string.IsNullOrWhiteSpace(title))
                title = "Guide";

            return $"{title}__{id}.json";
        }

        private static string MakeSafeFileName(string? value)
        {
            var name = (value ?? "").Trim();
            if (string.IsNullOrWhiteSpace(name))
                return "";

            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c.ToString(), "");

            name = name.Replace(" ", "_");
            return name.Length > 60 ? name.Substring(0, 60) : name;
        }

        private List<XmlGuide> LoadBuiltInGuides()
        {
            return LoadGuidesFromFolder(_communityFolder, true);
        }

        private static T? TryDeserialize<T>(string json)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        private static string ResolveWritableGuidesFolder()
        {
            var root = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Guides");
            if (CanWriteToFolder(root))
                return root;

            var appData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "LSR.XmlHelper.Wpf",
                "Guides");

            Directory.CreateDirectory(appData);
            return appData;
        }

        private static bool CanWriteToFolder(string folder)
        {
            try
            {
                Directory.CreateDirectory(folder);

                var testPath = Path.Combine(folder, Guid.NewGuid().ToString("N") + ".tmp");
                File.WriteAllText(testPath, "x");
                File.Delete(testPath);

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
