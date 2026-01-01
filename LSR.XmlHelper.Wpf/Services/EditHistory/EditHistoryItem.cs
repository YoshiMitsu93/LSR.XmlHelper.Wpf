using System;

namespace LSR.XmlHelper.Wpf.Services.EditHistory
{
    public enum EditHistoryOperation
    {
        FieldChange = 0,
        DuplicateEntry = 1,
        DeleteEntry = 2,
        DuplicateChildBlock = 3,
        DeleteChildBlock = 4
    }

    public sealed class EditHistoryItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;

        public string? FilePath { get; set; }
        public string? CollectionTitle { get; set; }

        public EditHistoryOperation Operation { get; set; } = EditHistoryOperation.FieldChange;

        public string? SourceEntryKey { get; set; }
        public int? SourceEntryOccurrence { get; set; }

        public string EntryKey { get; set; } = "";
        public int EntryOccurrence { get; set; }
        public string FieldPath { get; set; } = "";

        public string? OldValue { get; set; }
        public string NewValue { get; set; } = "";
    }
}
