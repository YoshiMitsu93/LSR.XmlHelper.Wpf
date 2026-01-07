using LSR.XmlHelper.Core.Services;
using LSR.XmlHelper.Wpf.Services.EditHistory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LSR.XmlHelper.Wpf.Services.Compare
{
    public sealed class XmlCompareService
    {
        private readonly XmlFriendlyViewService _friendly;

        public XmlCompareService(XmlFriendlyViewService friendly)
        {
            _friendly = friendly;
        }

        public List<EditHistoryItem> BuildEdits(
            string currentXmlText,
            string currentFilePath,
            string externalFilePath,
            out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(currentXmlText))
                return new List<EditHistoryItem>();

            if (string.IsNullOrWhiteSpace(currentFilePath) || !File.Exists(currentFilePath))
                return new List<EditHistoryItem>();

            if (string.IsNullOrWhiteSpace(externalFilePath) || !File.Exists(externalFilePath))
                return new List<EditHistoryItem>();

            var externalText = File.ReadAllText(externalFilePath);

            var currentDoc = _friendly.TryBuild(currentXmlText);
            if (currentDoc is null)
            {
                error = "Current XML could not be parsed into Friendly View.";
                return new List<EditHistoryItem>();
            }

            var externalDoc = _friendly.TryBuild(externalText);
            if (externalDoc is null)
            {
                error = "External XML could not be parsed into Friendly View.";
                return new List<EditHistoryItem>();
            }

            var edits = new List<EditHistoryItem>();
            var warnings = new List<string>();

            var currentCollections = currentDoc.Collections
                .GroupBy(c => c.Title, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            var externalCollections = externalDoc.Collections
                .GroupBy(c => c.Title, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var pair in externalCollections)
            {
                if (!currentCollections.TryGetValue(pair.Key, out var curCol))
                    continue;

                var extCol = pair.Value;

                var curByKey = curCol.Entries
                    .GroupBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Occurrence).ToList(), StringComparer.OrdinalIgnoreCase);

                var extByKey = extCol.Entries
                    .GroupBy(e => e.Key, StringComparer.OrdinalIgnoreCase)
                    .ToDictionary(g => g.Key, g => g.OrderBy(x => x.Occurrence).ToList(), StringComparer.OrdinalIgnoreCase);

                foreach (var extEntryGroup in extByKey)
                {
                    var entryKey = extEntryGroup.Key;
                    var extList = extEntryGroup.Value;
                    curByKey.TryGetValue(entryKey, out var curList);
                    curList ??= new List<XmlFriendlyEntry>();

                    var commonCount = Math.Min(curList.Count, extList.Count);

                    if (extList.Count > curList.Count)
                    {
                        if (curList.Count == 0)
                        {
                            warnings.Add($"Cannot import added entry: {pair.Key} | {entryKey} (no base entry to duplicate)");
                        }
                        else
                        {
                            var sourceOcc = curList.Max(x => x.Occurrence);
                            var sourceDisplay = curList.Last().Display;

                            for (var i = 0; i < extList.Count - curList.Count; i++)
                            {
                                edits.Add(new EditHistoryItem
                                {
                                    Operation = EditHistoryOperation.DuplicateEntry,
                                    FilePath = currentFilePath,
                                    CollectionTitle = pair.Key,
                                    SourceEntryKey = entryKey,
                                    SourceEntryOccurrence = sourceOcc + i,
                                    EntryKey = entryKey,
                                    EntryOccurrence = sourceOcc + i + 1,
                                    FieldPath = "DUPLICATE",
                                    OldValue = $"{sourceDisplay} ({entryKey}#{sourceOcc + i})",
                                    NewValue = $"{sourceDisplay} ({entryKey}#{sourceOcc + i + 1})"
                                });
                            }
                        }
                    }

                    if (curList.Count > extList.Count)
                    {
                        var toDelete = curList
                            .OrderByDescending(x => x.Occurrence)
                            .Take(curList.Count - extList.Count)
                            .ToList();

                        foreach (var d in toDelete)
                        {
                            edits.Add(new EditHistoryItem
                            {
                                Operation = EditHistoryOperation.DeleteEntry,
                                FilePath = currentFilePath,
                                CollectionTitle = pair.Key,
                                EntryKey = d.Key,
                                EntryOccurrence = d.Occurrence,
                                FieldPath = "DELETE",
                                OldValue = $"{d.Display} ({d.Key}#{d.Occurrence})",
                                NewValue = "(deleted)"
                            });
                        }
                    }

                    for (var i = 0; i < commonCount; i++)
                    {
                        var curEntry = curList[i];
                        var extEntry = extList[i];

                        foreach (var field in extEntry.Fields)
                        {
                            var path = field.Key;
                            var extValue = field.Value.Value ?? "";

                            if (!curEntry.Fields.TryGetValue(path, out var curField))
                                continue;

                            var curValue = curField.Value ?? "";
                            if (string.Equals(curValue, extValue, StringComparison.Ordinal))
                                continue;

                            edits.Add(new EditHistoryItem
                            {
                                Operation = EditHistoryOperation.FieldChange,
                                FilePath = currentFilePath,
                                CollectionTitle = pair.Key,
                                EntryKey = entryKey,
                                EntryOccurrence = curEntry.Occurrence,
                                FieldPath = path,
                                OldValue = curValue,
                                NewValue = extValue
                            });
                        }
                    }
                }
            }

            if (warnings.Count > 0)
                error = string.Join(" | ", warnings);

            return edits
                .OrderBy(x => x.CollectionTitle ?? "", StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.EntryKey ?? "", StringComparer.OrdinalIgnoreCase)
                .ThenBy(x => x.EntryOccurrence)
                .ThenBy(x => x.FieldPath ?? "", StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
