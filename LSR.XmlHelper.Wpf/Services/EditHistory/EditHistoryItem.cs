using System;

namespace LSR.XmlHelper.Wpf.Services.EditHistory
{
    public sealed class EditHistoryItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;

        public string? FilePath { get; set; }
        public string? CollectionTitle { get; set; }

        public string EntryKey { get; set; } = "";
        public string FieldPath { get; set; } = "";

        public string? OldValue { get; set; }
        public string NewValue { get; set; } = "";
    }
}
