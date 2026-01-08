using LSR.XmlHelper.Core.Models;
using LSR.XmlHelper.Core.Services;
using System;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Services.Friendly
{
    public sealed class FriendlyBulkFieldEditService
    {
        public BulkFieldEditResult Apply(
            XmlFriendlyViewService friendly,
            XmlFriendlyDocument doc,
            XmlFriendlyCollection? collection,
            IReadOnlyList<XmlFriendlyEntry> entries,
            string fieldName,
            string newValue)
        {
            var edits = new List<EntryFieldEdit>();
            var applied = 0;
            var failed = 0;
            string? firstError = null;

            foreach (var entry in entries)
            {
                entry.Fields.TryGetValue(fieldName, out var existingField);
                var previousValue = existingField?.Value;

                var occurrence = 0;
                if (collection is not null)
                {
                    var matchesBefore = 0;

                    foreach (var e2 in collection.Entries)
                    {
                        if (!string.Equals(e2.Key, entry.Key, StringComparison.OrdinalIgnoreCase))
                            continue;

                        if (ReferenceEquals(e2, entry))
                        {
                            occurrence = matchesBefore;
                            break;
                        }

                        matchesBefore++;
                    }
                }

                if (!entry.TrySetField(fieldName, newValue, out var err))
                {
                    failed++;
                    firstError ??= err;
                    edits.Add(new EntryFieldEdit(entry.Key, occurrence, previousValue, false));
                    continue;
                }

                entry.InvalidateFields();
                applied++;
                edits.Add(new EntryFieldEdit(entry.Key, occurrence, previousValue, true));
            }

            var updatedXml = friendly.ToXml(doc);

            return new BulkFieldEditResult(updatedXml, applied, failed, firstError, edits);
        }
    }

    public sealed class BulkFieldEditResult
    {
        public BulkFieldEditResult(string updatedXml, int appliedCount, int failedCount, string? error, IReadOnlyList<EntryFieldEdit> edits)
        {
            UpdatedXml = updatedXml;
            AppliedCount = appliedCount;
            FailedCount = failedCount;
            Error = error;
            Edits = edits;
        }

        public string UpdatedXml { get; }

        public int AppliedCount { get; }

        public int FailedCount { get; }

        public string? Error { get; }

        public IReadOnlyList<EntryFieldEdit> Edits { get; }
    }

    public sealed class EntryFieldEdit
    {
        public EntryFieldEdit(string entryKey, int occurrence, string? previousValue, bool applied)
        {
            EntryKey = entryKey;
            Occurrence = occurrence;
            PreviousValue = previousValue;
            Applied = applied;
        }

        public string EntryKey { get; }

        public int Occurrence { get; }

        public string? PreviousValue { get; }

        public bool Applied { get; }
    }
}
