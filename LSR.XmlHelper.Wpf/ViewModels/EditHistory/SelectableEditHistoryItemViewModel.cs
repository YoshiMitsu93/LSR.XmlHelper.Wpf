using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services.EditHistory;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class SelectableEditHistoryItemViewModel : ObservableObject
    {
        private bool _isSelected;

        public SelectableEditHistoryItemViewModel(EditHistoryItem item, bool isSelected)
        {
            Item = item;
            _isSelected = isSelected;
        }

        public EditHistoryItem Item { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string Display
        {
            get
            {
                var file = Item.FilePath ?? "";
                var col = Item.CollectionTitle ?? "";

                if (Item.Operation == EditHistoryOperation.DuplicateEntry)
                {
                    var srcKey = Item.SourceEntryKey ?? "";
                    var srcOcc = Item.SourceEntryOccurrence ?? 0;
                    return $"{Item.TimestampUtc:u} | {col} | DUPLICATE | {srcKey}#{srcOcc} -> {Item.EntryKey}#{Item.EntryOccurrence} | {file}";
                }

                var oldV = Item.OldValue ?? "";
                if (Item.Operation == EditHistoryOperation.DeleteEntry)
                {
                    return $"{Item.TimestampUtc:u} | {col} | DELETE | {Item.EntryKey}#{Item.EntryOccurrence} | {file}";
                }

                return $"{Item.TimestampUtc:u} | {col} | {Item.EntryKey}#{Item.EntryOccurrence} | {Item.FieldPath} | {oldV} -> {Item.NewValue} | {file}";
            }
        }
    }
}
