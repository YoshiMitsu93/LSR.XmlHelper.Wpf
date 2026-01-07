using LSR.XmlHelper.Wpf.Services.EditHistory;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSR.XmlHelper.Wpf.Services.Compare
{
    public sealed class CompareEditsImportService
    {
        private readonly AppSettingsService _settingsService;
        private readonly AppSettings _settings;

        public CompareEditsImportService(AppSettingsService settingsService, AppSettings settings)
        {
            _settingsService = settingsService;
            _settings = settings;
        }

        public void ImportAsPending(IReadOnlyList<EditHistoryItem> items)
        {
            ImportInto(_settings.EditHistory.Pending, items);
        }

        public void ImportAsCommitted(IReadOnlyList<EditHistoryItem> items)
        {
            ImportInto(_settings.EditHistory.Committed, items);
        }

        private void ImportInto(List<EditHistoryItem> target, IReadOnlyList<EditHistoryItem> items)
        {
            if (items is null || items.Count == 0)
                return;

            var existing = new HashSet<string>(target.Select(BuildSignature), StringComparer.Ordinal);
            foreach (var item in items)
            {
                var sig = BuildSignature(item);
                if (existing.Add(sig))
                    target.Add(item);
            }

            _settingsService.Save(_settings);
        }

        private static string BuildSignature(EditHistoryItem item)
        {
            var fp = item.FilePath ?? "";
            var col = item.CollectionTitle ?? "";
            var op = (int)item.Operation;
            var sk = item.SourceEntryKey ?? "";
            var so = item.SourceEntryOccurrence?.ToString() ?? "";
            var k = item.EntryKey ?? "";
            var o = item.EntryOccurrence.ToString();
            var path = item.FieldPath ?? "";
            var nv = item.NewValue ?? "";
            return string.Join("|", fp, col, op.ToString(), sk, so, k, o, path, nv);
        }
    }
}
