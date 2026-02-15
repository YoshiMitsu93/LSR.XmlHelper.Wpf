using LSR.XmlHelper.Wpf.Infrastructure;
using LSR.XmlHelper.Wpf.Services.EditHistory;
using System;
using System.Linq;
using System.Xml.Linq;

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

                if (Item.Operation == EditHistoryOperation.AddEntry)
                {
                    var name = "";
                    var id = "";
                    var groupName = "";
                    var modelName = "";
                    var category = "";
                    var itemsCount = 0;

                    var raw = Item.NewValue ?? "";
                    if (!string.IsNullOrWhiteSpace(raw))
                    {
                        try
                        {
                            var el = XElement.Parse(raw);

                            string FindFirstElementValueIgnoreCase(string wanted)
                            {
                                foreach (var node in el.Descendants())
                                {
                                    if (string.Equals(node.Name.LocalName, wanted, StringComparison.OrdinalIgnoreCase))
                                        return node.Value ?? "";
                                }

                                foreach (var node in el.Elements())
                                {
                                    if (string.Equals(node.Name.LocalName, wanted, StringComparison.OrdinalIgnoreCase))
                                        return node.Value ?? "";
                                }

                                return "";
                            }

                            string FindFirstAttributeValueIgnoreCase(string wanted)
                            {
                                foreach (var node in el.DescendantsAndSelf())
                                {
                                    foreach (var a in node.Attributes())
                                    {
                                        if (string.Equals(a.Name.LocalName, wanted, StringComparison.OrdinalIgnoreCase))
                                            return a.Value ?? "";
                                    }
                                }

                                return "";
                            }

                            XElement? FindFirstElementIgnoreCase(string wanted)
                            {
                                foreach (var node in el.Descendants())
                                {
                                    if (string.Equals(node.Name.LocalName, wanted, StringComparison.OrdinalIgnoreCase))
                                        return node;
                                }

                                foreach (var node in el.Elements())
                                {
                                    if (string.Equals(node.Name.LocalName, wanted, StringComparison.OrdinalIgnoreCase))
                                        return node;
                                }

                                return null;
                            }

                            name = FindFirstElementValueIgnoreCase("Name");
                            if (string.IsNullOrWhiteSpace(name))
                                name = FindFirstElementValueIgnoreCase("DisplayName");
                            if (string.IsNullOrWhiteSpace(name))
                                name = FindFirstElementValueIgnoreCase("Title");
                            if (string.IsNullOrWhiteSpace(name))
                                name = FindFirstAttributeValueIgnoreCase("Name");

                            id = FindFirstElementValueIgnoreCase("ID");
                            if (string.IsNullOrWhiteSpace(id))
                                id = FindFirstAttributeValueIgnoreCase("ID");

                            groupName = FindFirstElementValueIgnoreCase("GroupName");
                            if (string.IsNullOrWhiteSpace(groupName))
                                groupName = FindFirstAttributeValueIgnoreCase("GroupName");

                            modelName = FindFirstElementValueIgnoreCase("ModelName");
                            category = FindFirstElementValueIgnoreCase("Category");

                            var itemsEl = FindFirstElementIgnoreCase("Items");
                            if (itemsEl != null)
                                itemsCount = itemsEl.Elements().Count();
                        }
                        catch
                        {
                        }
                    }
                    
                    var details = "";

                    if (!string.IsNullOrWhiteSpace(name))
                        details += $" | Name: {name}";
                    else if (!string.IsNullOrWhiteSpace(id))
                        details += $" | ID: {id}";
                    else if (!string.IsNullOrWhiteSpace(groupName))
                        details += $" | Group: {groupName}";

                    if (itemsCount > 0)
                        details += $" | Items: {itemsCount}";

                    if (!string.IsNullOrWhiteSpace(modelName))
                        details += $" | Model: {modelName}";
                    if (!string.IsNullOrWhiteSpace(category))
                        details += $" | Category: {category}";

                    if (string.IsNullOrWhiteSpace(details))
                    {
                        var friendly = Item.OldValue ?? "";
                        if (!string.IsNullOrWhiteSpace(friendly))
                            details = $" | {friendly}";
                    }

                    return $"{Item.TimestampUtc:u} | {col} | ADD | {Item.EntryKey}#{Item.EntryOccurrence}{details} | {file}";
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
