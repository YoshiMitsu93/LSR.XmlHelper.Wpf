using LSR.XmlHelper.Core.Models;
using LSR.XmlHelper.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LSR.XmlHelper.Wpf.Services.EditHistory
{
    public sealed class EditHistoryService
    {
        private readonly AppSettings _settings;
        private readonly AppSettingsService _settingsService;
        private readonly XmlFriendlyViewService _friendly;
        private readonly XmlDocumentService _xml;
        public event EventHandler? HistoryChanged;

        public EditHistoryService(AppSettings settings, AppSettingsService settingsService, XmlFriendlyViewService friendly, XmlDocumentService xml)
        {
            _settings = settings;
            _settingsService = settingsService;
            _friendly = friendly;
            _xml = xml;
        }

        public IReadOnlyList<EditHistoryItem> Pending => _settings.EditHistory.Pending;
        public IReadOnlyList<EditHistoryItem> Committed => _settings.EditHistory.Committed;

        public void AddPending(string filePath, string? collectionTitle, string entryKey, int entryOccurrence, string fieldPath, string? oldValue, string newValue)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            if (string.IsNullOrWhiteSpace(entryKey) || string.IsNullOrWhiteSpace(fieldPath))
                return;

            if (string.Equals(oldValue ?? "", newValue ?? "", StringComparison.Ordinal))
                return;

            var item = new EditHistoryItem
            {
                Operation = EditHistoryOperation.FieldChange,
                FilePath = filePath,
                CollectionTitle = collectionTitle,
                EntryKey = entryKey,
                EntryOccurrence = entryOccurrence,
                FieldPath = fieldPath,
                OldValue = oldValue,
                NewValue = newValue ?? ""
            };

            _settings.EditHistory.Pending.Add(item);
            Persist();
        }
        public void AddPendingDuplicateEntry(string filePath, string? collectionTitle, string sourceEntryKey, int sourceEntryOccurrence, string? sourceEntryDisplay)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            if (string.IsNullOrWhiteSpace(sourceEntryKey))
                return;

            if (sourceEntryOccurrence < 0)
                return;

            var sourceLabel = string.IsNullOrWhiteSpace(sourceEntryDisplay) ? sourceEntryKey : sourceEntryDisplay;

            var item = new EditHistoryItem
            {
                Operation = EditHistoryOperation.DuplicateEntry,
                FilePath = filePath,
                CollectionTitle = collectionTitle,
                SourceEntryKey = sourceEntryKey,
                SourceEntryOccurrence = sourceEntryOccurrence,
                EntryKey = sourceEntryKey,
                EntryOccurrence = sourceEntryOccurrence + 1,
                FieldPath = "DUPLICATE",
                OldValue = $"{sourceLabel} ({sourceEntryKey}#{sourceEntryOccurrence})",
                NewValue = $"{sourceLabel} ({sourceEntryKey}#{sourceEntryOccurrence + 1})"
            };

            _settings.EditHistory.Pending.Add(item);
            Persist();
        }

        public void AddPendingDuplicateChildBlock(string filePath, string? collectionTitle, string entryKey, int entryOccurrence, string fieldPath, string? display)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            if (string.IsNullOrWhiteSpace(entryKey))
                return;

            if (entryOccurrence < 0)
                return;

            if (string.IsNullOrWhiteSpace(fieldPath))
                return;

            var label = string.IsNullOrWhiteSpace(display) ? entryKey : display;

            var item = new EditHistoryItem
            {
                Operation = EditHistoryOperation.DuplicateChildBlock,
                FilePath = filePath,
                CollectionTitle = collectionTitle,
                EntryKey = entryKey,
                EntryOccurrence = entryOccurrence,
                FieldPath = fieldPath,
                OldValue = $"{label} ({entryKey}#{entryOccurrence})",
                NewValue = $"{label} ({entryKey}#{entryOccurrence})"
            };

            _settings.EditHistory.Pending.Add(item);
            Persist();
        }

        public void AddPendingDeleteEntry(string filePath, string? collectionTitle, string entryKey, int entryOccurrence, string? entryDisplay)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            if (string.IsNullOrWhiteSpace(entryKey))
                return;

            if (entryOccurrence < 0)
                return;

            var entryLabel = string.IsNullOrWhiteSpace(entryDisplay) ? entryKey : entryDisplay;

            var item = new EditHistoryItem
            {
                Operation = EditHistoryOperation.DeleteEntry,
                FilePath = filePath,
                CollectionTitle = collectionTitle,
                EntryKey = entryKey,
                EntryOccurrence = entryOccurrence,
                FieldPath = "DELETE",
                OldValue = $"{entryLabel} ({entryKey}#{entryOccurrence})",
                NewValue = "(deleted)"
            };

            _settings.EditHistory.Pending.Add(item);
            Persist();
        }

        public void AddPendingDeleteChildBlock(string filePath, string? collectionTitle, string entryKey, int entryOccurrence, string fieldPath, string? display)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            if (string.IsNullOrWhiteSpace(entryKey))
                return;

            if (entryOccurrence < 0)
                return;

            if (string.IsNullOrWhiteSpace(fieldPath))
                return;

            var label = string.IsNullOrWhiteSpace(display) ? entryKey : display;

            var item = new EditHistoryItem
            {
                Operation = EditHistoryOperation.DeleteChildBlock,
                FilePath = filePath,
                CollectionTitle = collectionTitle,
                EntryKey = entryKey,
                EntryOccurrence = entryOccurrence,
                FieldPath = fieldPath,
                OldValue = $"{label} ({entryKey}#{entryOccurrence})",
                NewValue = "(deleted)"
            };

            _settings.EditHistory.Pending.Add(item);
            Persist();
        }

        public void CommitForFile(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            var pending = _settings.EditHistory.Pending;
            if (pending.Count == 0)
                return;

            var moved = pending
                .Where(p => string.Equals(p.FilePath, filePath, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (moved.Count == 0)
                return;

            foreach (var m in moved)
                pending.Remove(m);

            _settings.EditHistory.Committed.AddRange(moved);
            Persist();
        }

        public bool TryApplyToXmlText(string xmlText, IEnumerable<EditHistoryItem> edits, out string updatedXml, out string? error)
        {
            updatedXml = xmlText;
            error = null;

            var doc = _friendly.TryBuild(xmlText);
            if (doc is null)
            {
                error = "XML could not be parsed into Friendly View.";
                return false;
            }

            foreach (var e in edits.OrderBy(x => x.TimestampUtc).ThenBy(x => x.Id))
            {
                if (e.Operation == EditHistoryOperation.DuplicateEntry)
                {
                    var srcKey = e.SourceEntryKey ?? e.EntryKey;
                    var srcOcc = e.SourceEntryOccurrence ?? Math.Max(0, e.EntryOccurrence - 1);

                    if (!TryGetEntry(doc, e.CollectionTitle, srcKey, srcOcc, out var sourceEntry))
                    {
                        error = $"Entry not found: {srcKey}";
                        return false;
                    }

                    if (!_friendly.TryDuplicateEntry(doc, sourceEntry, insertAfter: true, out _, out var dupErr))
                    {
                        error = dupErr ?? "Duplicate failed.";
                        return false;
                    }

                    var rebuiltAfterDuplicate = _friendly.TryBuild(_friendly.ToXml(doc));
                    if (rebuiltAfterDuplicate is null)
                    {
                        error = "XML could not be rebuilt after duplication.";
                        return false;
                    }

                    doc = rebuiltAfterDuplicate;
                    continue;
                }

                if (e.Operation == EditHistoryOperation.DeleteEntry)
                {
                    if (!TryGetEntry(doc, e.CollectionTitle, e.EntryKey, e.EntryOccurrence, out var deleteEntry))
                        continue;

                    if (!_friendly.TryDeleteEntry(doc, deleteEntry, out var delErr))
                    {
                        error = delErr ?? "Delete failed.";
                        return false;
                    }

                    var rebuiltAfterDelete = _friendly.TryBuild(_friendly.ToXml(doc));
                    if (rebuiltAfterDelete is null)
                    {
                        error = "XML could not be rebuilt after deletion.";
                        return false;
                    }

                    doc = rebuiltAfterDelete;
                    continue;
                }

                if (e.Operation == EditHistoryOperation.DuplicateChildBlock)
                {
                    if (!TryGetEntry(doc, e.CollectionTitle, e.EntryKey, e.EntryOccurrence, out var entry))
                    {
                        error = $"Entry not found: {e.EntryKey}";
                        return false;
                    }

                    if (!LSR.XmlHelper.Wpf.Services.LookupFieldPathParser.TryParseLookupField(e.FieldPath, out var groupTitle, out var itemName, out _))
                    {
                        error = "Child block path is not in the expected format.";
                        return false;
                    }

                    if (!_friendly.TryDuplicateChildBlock(doc, entry, groupTitle, itemName, insertAfter: true, out var dupChildErr))
                    {
                        error = dupChildErr ?? "Duplicate item failed.";
                        return false;
                    }

                    var rebuiltAfterChildDup = _friendly.TryBuild(_friendly.ToXml(doc));
                    if (rebuiltAfterChildDup is null)
                    {
                        error = "XML could not be rebuilt after duplicating item.";
                        return false;
                    }

                    doc = rebuiltAfterChildDup;
                    continue;
                }

                if (e.Operation == EditHistoryOperation.DeleteChildBlock)
                {
                    if (!TryGetEntry(doc, e.CollectionTitle, e.EntryKey, e.EntryOccurrence, out var entry))
                    {
                        error = $"Entry not found: {e.EntryKey}";
                        return false;
                    }

                    if (!LSR.XmlHelper.Wpf.Services.LookupFieldPathParser.TryParseLookupField(e.FieldPath, out var groupTitle, out var itemName, out _))
                    {
                        error = "Child block path is not in the expected format.";
                        return false;
                    }

                    if (!_friendly.TryDeleteChildBlock(doc, entry, groupTitle, itemName, out var delChildErr))
                    {
                        error = delChildErr ?? "Delete item failed.";
                        return false;
                    }

                    var rebuiltAfterChildDel = _friendly.TryBuild(_friendly.ToXml(doc));
                    if (rebuiltAfterChildDel is null)
                    {
                        error = "XML could not be rebuilt after deleting item.";
                        return false;
                    }

                    doc = rebuiltAfterChildDel;
                    continue;
                }

                if (!TryGetEntry(doc, e.CollectionTitle, e.EntryKey, e.EntryOccurrence, out var fieldEntry))
                {
                    if (!TryResolveEntryByUniqueField(doc, e.CollectionTitle, e.FieldPath, out fieldEntry))
                    {
                        error = $"Entry not found: {e.EntryKey}";
                        return false;
                    }
                }

                if (!fieldEntry.TrySetField(e.FieldPath, e.NewValue, out var fieldErr))
                {
                    error = fieldErr ?? $"Failed to set field: {e.FieldPath}";
                    return false;
                }
            }

            updatedXml = _friendly.ToXml(doc);
            return true;
        }

        public bool TryDeleteChildBlock(XmlFriendlyDocument document, XmlFriendlyEntry sourceEntry, string groupTitle, string itemName, out string? error)
        {
            error = null;

            if (string.IsNullOrWhiteSpace(groupTitle) || string.IsNullOrWhiteSpace(itemName))
            {
                error = "Group title or item name is missing.";
                return false;
            }

            var lb = itemName.IndexOf('[', StringComparison.Ordinal);
            var rb = itemName.EndsWith("]", StringComparison.Ordinal);

            if (lb <= 0 || !rb)
            {
                error = "Item name is not in the expected format (Name[index]).";
                return false;
            }

            var elementName = itemName.Substring(0, lb);
            var indexText = itemName.Substring(lb + 1, itemName.Length - lb - 2);

            if (!int.TryParse(indexText, out var index) || index <= 0)
            {
                error = "Item index is invalid.";
                return false;
            }

            try
            {
                var entryElement = sourceEntry.Element;

                var candidateContainer = entryElement.Element(groupTitle);
                var container = candidateContainer is not null && candidateContainer.Elements(elementName).Any()
                    ? candidateContainer
                    : entryElement;

                var items = container.Elements(elementName).ToList();
                if (items.Count == 0)
                {
                    error = "No matching items were found for this group.";
                    return false;
                }

                if (index > items.Count)
                {
                    error = "Item index is out of range.";
                    return false;
                }

                var toRemove = items[index - 1];
                RemovePreservingWhitespace(toRemove);

                sourceEntry.InvalidateFields();
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool TryLoadFileAndApply(string filePath, IEnumerable<EditHistoryItem> edits, out string updatedXml, out string? error)
        {
            updatedXml = "";
            error = null;

            try
            {
                var xml = _xml.LoadFromFile(filePath);
                return TryApplyToXmlText(xml, edits, out updatedXml, out error);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool TryLoadXml(string filePath, out string xmlText, out string? error)
        {
            xmlText = "";
            error = null;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                error = "No file path provided.";
                return false;
            }

            try
            {
                xmlText = _xml.LoadFromFile(filePath);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        public bool TryEntryExists(string xmlText, string? collectionTitle, string entryKey, int entryOccurrence)
        {
            var doc = _friendly.TryBuild(xmlText);
            if (doc is null)
                return false;

            return TryGetEntry(doc, collectionTitle, entryKey, entryOccurrence, out _);
        }

        public bool TryGetCurrentFieldValue(string xmlText, string? collectionTitle, string entryKey, int entryOccurrence, string fieldPath, out string? value, out string? error)
        {
            value = null;
            error = null;

            var doc = _friendly.TryBuild(xmlText);
            if (doc is null)
            {
                error = "XML could not be parsed into Friendly View.";
                return false;
            }

            if (!TryGetEntry(doc, collectionTitle, entryKey, entryOccurrence, out var friendlyEntry))
            {
                if (!TryResolveEntryByUniqueField(doc, collectionTitle, fieldPath, out friendlyEntry))
                {
                    error = $"Entry not found: {entryKey}";
                    return false;
                }
            }

            if (!friendlyEntry.Fields.TryGetValue(fieldPath, out var field))
            {
                error = $"Field not found: {fieldPath}";
                return false;
            }

            value = field.Value;
            return true;
        }

        public int DeletePending(IEnumerable<Guid> ids)
        {
            var set = new HashSet<Guid>(ids);
            if (set.Count == 0)
                return 0;

            var removed = _settings.EditHistory.Pending.RemoveAll(e => set.Contains(e.Id));
            if (removed > 0)
                Persist();

            return removed;
        }

        public int DeleteCommitted(IEnumerable<Guid> ids)
        {
            var set = new HashSet<Guid>(ids);
            if (set.Count == 0)
                return 0;

            var removed = _settings.EditHistory.Committed.RemoveAll(e => set.Contains(e.Id));
            if (removed > 0)
                Persist();

            return removed;
        }

        private static bool TryGetEntry(XmlFriendlyDocument doc, string? collectionTitle, string entryKey, int occurrence, out XmlFriendlyEntry entry)
        {
            if (occurrence < 0)
                occurrence = 0;

            IEnumerable<XmlFriendlyCollection> collections = doc.Collections;

            if (!string.IsNullOrWhiteSpace(collectionTitle))
            {
                var matchCollection = doc.Collections.FirstOrDefault(c => string.Equals(c.Title, collectionTitle, StringComparison.OrdinalIgnoreCase));
                if (matchCollection is not null)
                    collections = new[] { matchCollection };
            }

            foreach (var c in collections)
            {
                var matches = c.Entries.Where(e => string.Equals(e.Key, entryKey, StringComparison.OrdinalIgnoreCase)).ToList();
                if (matches.Count == 0)
                    continue;

                if (occurrence >= matches.Count)
                    occurrence = matches.Count - 1;

                entry = matches[occurrence];
                return true;
            }

            entry = null!;
            return false;
        }

        private static bool TryResolveEntryByUniqueField(XmlFriendlyDocument doc, string? collectionTitle, string fieldPath, out XmlFriendlyEntry entry)
        {
            IEnumerable<XmlFriendlyCollection> collections = doc.Collections;

            if (!string.IsNullOrWhiteSpace(collectionTitle))
            {
                var matchCollection = doc.Collections.FirstOrDefault(c => string.Equals(c.Title, collectionTitle, StringComparison.OrdinalIgnoreCase));
                if (matchCollection is not null)
                    collections = new[] { matchCollection };
            }

            var matches = new List<XmlFriendlyEntry>();

            foreach (var c in collections)
            {
                foreach (var e in c.Entries)
                {
                    if (e.Fields.ContainsKey(fieldPath))
                        matches.Add(e);
                }
            }

            if (matches.Count == 1)
            {
                entry = matches[0];
                return true;
            }

            entry = null!;
            return false;
        }

        private static void RemovePreservingWhitespace(System.Xml.Linq.XElement element)
        {
            var next = element.NextNode as System.Xml.Linq.XText;
            if (next is not null && string.IsNullOrWhiteSpace(next.Value))
                next.Remove();

            var prev = element.PreviousNode as System.Xml.Linq.XText;
            if (prev is not null && string.IsNullOrWhiteSpace(prev.Value))
                prev.Remove();
        }

        private void Persist()
        {
            try
            {
                _settingsService.Save(_settings);
            }
            catch
            {
            }

            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

