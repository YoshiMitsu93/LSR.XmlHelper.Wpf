using LSR.XmlHelper.Wpf.Services.EditHistory;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LSR.XmlHelper.Wpf.Services.SharedConfigs
{
    public sealed class SharedConfigPackService
    {
        private readonly AppSettingsService _settingsService;
        private readonly SettingsCopyService _copier;

        public SharedConfigPackService(AppSettingsService settingsService, SettingsCopyService copier)
        {
            _settingsService = settingsService;
            _copier = copier;
        }

        public string GetSharedConfigsFolder(string rootFolder)
        {
            var dir = Path.Combine(rootFolder, "LSR-XML-Helper", "Shared-Configs");
            Directory.CreateDirectory(dir);
            return dir;
        }

        public void OpenSharedConfigsFolder(string rootFolder)
        {
            var dir = GetSharedConfigsFolder(rootFolder);
            var psi = new ProcessStartInfo
            {
                FileName = dir,
                UseShellExecute = true
            };
            Process.Start(psi);
        }

        public IReadOnlyList<string> GetConfigPackFiles(string rootFolder)
        {
            var dir = GetSharedConfigsFolder(rootFolder);
            return Directory.GetFiles(dir, "*.json")
                .OrderByDescending(File.GetLastWriteTimeUtc)
                .ToList();
        }

        public string Export(
            string rootFolder,
            AppSettings settings,
            string name,
            string description,
            bool includeAppearance,
            IReadOnlyList<EditHistoryItem> selectedPending,
            IReadOnlyList<EditHistoryItem> selectedCommitted)
        {
            var dir = GetSharedConfigsFolder(rootFolder);
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var safeName = MakeSafeFileName(string.IsNullOrWhiteSpace(name) ? "ConfigPack" : name);
            var fileName = $"{safeName}_{stamp}.json";
            var outPath = Path.Combine(dir, fileName);

            var pack = new SharedConfigPack
            {
                Version = 1,
                CreatedUtc = DateTimeOffset.UtcNow,
                Name = name ?? "",
                Description = description ?? ""
            };

            if (includeAppearance)
                pack.Appearance = settings.Appearance;

            pack.EditHistory = new EditHistorySettings
            {
                Pending = selectedPending?.ToList() ?? new List<EditHistoryItem>(),
                Committed = selectedCommitted?.ToList() ?? new List<EditHistoryItem>()
            };

            var json = JsonSerializer.Serialize(pack, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(outPath, json);
            return outPath;
        }

        public SharedConfigPack Load(string packPath)
        {
            var json = File.ReadAllText(packPath);
            return JsonSerializer.Deserialize<SharedConfigPack>(json) ?? new SharedConfigPack();
        }

        public IReadOnlyList<string> PreviewAppearanceChanges(AppearanceSettings current, AppearanceSettings incoming)
        {
            var changes = new List<string>();
            BuildAppearanceDiff(changes, "Appearance", current, incoming);
            return changes;
        }

        public void ImportInto(
            AppSettings settings,
            SharedConfigPack pack,
            bool importAppearance,
            bool importEdits,
            IReadOnlyList<EditHistoryItem> pendingToImport,
            IReadOnlyList<EditHistoryItem> committedToImport,
            bool importEditsAsPending)
        {
            if (importAppearance && pack.Appearance is not null)
            {
                _copier.CopyPublicSettableProperties(pack.Appearance, settings.Appearance);
            }

            if (importEdits && pack.EditHistory is not null)
            {
                var incoming = importEditsAsPending
                    ? pendingToImport.Concat(committedToImport).ToList()
                    : committedToImport.Concat(pendingToImport).ToList();

                if (importEditsAsPending)
                    MergeList(settings.EditHistory.Pending, incoming);
                else
                    MergeList(settings.EditHistory.Committed, incoming);
            }

            _settingsService.Save(settings);
        }

        private static void MergeList(List<EditHistoryItem> target, List<EditHistoryItem> incoming)
        {
            var existing = new HashSet<Guid>(target.Select(x => x.Id));
            foreach (var item in incoming)
            {
                if (existing.Add(item.Id))
                    target.Add(item);
            }
        }

        private static string MakeSafeFileName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var cleaned = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
            return string.IsNullOrWhiteSpace(cleaned) ? "ConfigPack" : cleaned.Trim();
        }

        private static void BuildAppearanceDiff(List<string> changes, string path, object current, object incoming)
        {
            var t = current.GetType();
            foreach (var p in t.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance))
            {
                if (!p.CanRead)
                    continue;

                var curVal = p.GetValue(current);
                var newVal = p.GetValue(incoming);

                if (curVal is null && newVal is null)
                    continue;

                var propPath = $"{path}.{p.Name}";

                if (p.PropertyType.IsValueType || p.PropertyType == typeof(string))
                {
                    var curS = curVal?.ToString() ?? "";
                    var newS = newVal?.ToString() ?? "";
                    if (!string.Equals(curS, newS, StringComparison.Ordinal))
                        changes.Add($"{propPath}: {curS} -> {newS}");

                    continue;
                }

                if (curVal is IEnumerable curEnum && newVal is IEnumerable newEnum)
                {
                    var curSummary = SummarizeEnumerable(curEnum);
                    var newSummary = SummarizeEnumerable(newEnum);

                    if (!string.Equals(curSummary, newSummary, StringComparison.Ordinal))
                        changes.Add($"{propPath}: {curSummary} -> {newSummary}");

                    continue;
                }

                if (curVal is null || newVal is null)
                    continue;

                BuildAppearanceDiff(changes, propPath, curVal, newVal);
            }
        }

        private static string SummarizeEnumerable(IEnumerable items)
        {
            if (items is IList list)
                return $"{list.Count} items";

            var count = 0;
            foreach (var _ in items)
            {
                count++;
                if (count > 1000)
                    break;
            }

            return $"{count} items";
        }

    }
}

