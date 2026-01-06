using System;

namespace LSR.XmlHelper.Core.Models
{
    public sealed class GlobalFriendlySearchHit
    {
        public GlobalFriendlySearchHit(string filePath, string collectionTitle, string entryKey, int entryOccurrence, string fieldKey, string preview)
        {
            FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
            CollectionTitle = collectionTitle ?? throw new ArgumentNullException(nameof(collectionTitle));
            EntryKey = entryKey ?? throw new ArgumentNullException(nameof(entryKey));
            EntryOccurrence = entryOccurrence;
            FieldKey = fieldKey ?? throw new ArgumentNullException(nameof(fieldKey));
            Preview = preview ?? "";
        }

        public string FilePath { get; }
        public string CollectionTitle { get; }
        public string EntryKey { get; }
        public int EntryOccurrence { get; }
        public string FieldKey { get; }
        public string Preview { get; }
        public string FieldName
        {
            get
            {
                var idx = FieldKey.LastIndexOf('/');
                if (idx >= 0 && idx + 1 < FieldKey.Length)
                    return FieldKey.Substring(idx + 1);

                return FieldKey;
            }
        }
    }
}
