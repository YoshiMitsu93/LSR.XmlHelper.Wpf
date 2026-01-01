using LSR.XmlHelper.Wpf.Services.EditHistory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LSR.XmlHelper.Wpf.Services.EditSummary
{
    public sealed class EditSummaryExportService
    {
        public string BuildSummary(
            string loadedFolderPath,
            string? exportName,
            string? notes,
            IEnumerable<EditHistoryItem> pending,
            IEnumerable<EditHistoryItem> committed)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Edit Summary");
            if (!string.IsNullOrWhiteSpace(exportName))
                sb.AppendLine($"Name: {exportName}");
            sb.AppendLine($"Date: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"Loaded Folder: {loadedFolderPath}");

            if (!string.IsNullOrWhiteSpace(notes))
            {
                sb.AppendLine();
                sb.AppendLine("Notes:");
                foreach (var line in notes.Replace("\r\n", "\n").Split('\n'))
                    sb.AppendLine($"  {line}");
            }

            sb.AppendLine();

            var pendingList = (pending ?? Enumerable.Empty<EditHistoryItem>()).ToList();
            var committedList = (committed ?? Enumerable.Empty<EditHistoryItem>()).ToList();

            var fileKeys = pendingList
                .Select(x => x.FilePath)
                .Concat(committedList.Select(x => x.FilePath))
                .Where(p => !string.IsNullOrWhiteSpace(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (var filePath in fileKeys)
            {
                var displayPath = GetDisplayPath(loadedFolderPath, filePath!);
                sb.AppendLine($"XML: {displayPath}");

                var p = pendingList
                    .Where(x => string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => x.TimestampUtc)
                    .ThenBy(x => x.Id)
                    .ToList();

                var c = committedList
                    .Where(x => string.Equals(x.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(x => x.TimestampUtc)
                    .ThenBy(x => x.Id)
                    .ToList();

                WriteGroup(sb, "Pending", p);
                WriteGroup(sb, "Committed", c);

                sb.AppendLine();
            }

            return sb.ToString();
        }

        private void WriteGroup(StringBuilder sb, string title, List<EditHistoryItem> items)
        {
            sb.AppendLine($"  {title} ({items.Count}):");

            if (items.Count == 0)
            {
                sb.AppendLine("    (none)");
                return;
            }

            foreach (var item in items)
            {
                var key = BuildKey(item.EntryKey, item.EntryOccurrence);
                var action = ToFriendlyAction(item.Operation);

                if (item.Operation == EditHistoryOperation.FieldChange)
                {
                    var oldV = item.OldValue ?? "";
                    var newV = item.NewValue ?? "";
                    sb.AppendLine($"    - {key}: {item.FieldPath} | {oldV} -> {newV}");
                }
                else
                {
                    sb.AppendLine($"    - {key}: {action}");
                }
            }
        }

        private string BuildKey(string entryKey, int entryOccurrence)
        {
            if (entryOccurrence <= 0)
                return entryKey;
            return $"{entryKey}#{entryOccurrence}";
        }

        private string ToFriendlyAction(EditHistoryOperation op)
        {
            var s = op.ToString();
            return SplitPascalCase(s);
        }

        private string SplitPascalCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            var sb = new StringBuilder();
            sb.Append(value[0]);

            for (var i = 1; i < value.Length; i++)
            {
                var c = value[i];
                if (char.IsUpper(c) && value[i - 1] != ' ')
                    sb.Append(' ');
                sb.Append(c);
            }

            return sb.ToString();
        }

        private string GetDisplayPath(string loadedFolderPath, string fullPath)
        {
            if (string.IsNullOrWhiteSpace(loadedFolderPath))
                return Path.GetFileName(fullPath);

            try
            {
                var rel = Path.GetRelativePath(loadedFolderPath, fullPath);
                if (!string.IsNullOrWhiteSpace(rel) && !rel.StartsWith(".."))
                    return rel;
            }
            catch
            {
            }

            return Path.GetFileName(fullPath);
        }
    }
}
