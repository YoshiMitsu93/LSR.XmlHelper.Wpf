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

        public EditHistoryService(AppSettings settings, AppSettingsService settingsService, XmlFriendlyViewService friendly, XmlDocumentService xml)
        {
            _settings = settings;
            _settingsService = settingsService;
            _friendly = friendly;
            _xml = xml;
        }

        public IReadOnlyList<EditHistoryItem> Pending => _settings.EditHistory.Pending;
        public IReadOnlyList<EditHistoryItem> Committed => _settings.EditHistory.Committed;

        public void AddPending(string filePath, string? collectionTitle, string entryKey, string fieldPath, string? oldValue, string newValue)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return;

            if (string.IsNullOrWhiteSpace(entryKey) || string.IsNullOrWhiteSpace(fieldPath))
                return;

            if (string.Equals(oldValue ?? "", newValue ?? "", StringComparison.Ordinal))
                return;

            var item = new EditHistoryItem
            {
                FilePath = filePath,
                CollectionTitle = collectionTitle,
                EntryKey = entryKey,
                FieldPath = fieldPath,
                OldValue = oldValue,
                NewValue = newValue ?? ""
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

            foreach (var e in edits)
            {
                if (!TryGetEntry(doc, e.EntryKey, out var entry))
                {
                    error = $"Entry not found: {e.EntryKey}";
                    return false;
                }

                if (!entry.TrySetField(e.FieldPath, e.NewValue, out var fieldErr))
                {
                    error = fieldErr ?? $"Failed to set field: {e.FieldPath}";
                    return false;
                }
            }

            updatedXml = _friendly.ToXml(doc);
            return true;
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

        public bool TryGetCurrentFieldValue(string xmlText, string entryKey, string fieldPath, out string? value, out string? error)
        {
            value = null;
            error = null;

            var doc = _friendly.TryBuild(xmlText);
            if (doc is null)
            {
                error = "XML could not be parsed into Friendly View.";
                return false;
            }

            if (!TryGetEntry(doc, entryKey, out var entry))
            {
                error = $"Entry not found: {entryKey}";
                return false;
            }

            if (!entry.Fields.TryGetValue(fieldPath, out var field))
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

        private static bool TryGetEntry(XmlFriendlyDocument doc, string entryKey, out XmlFriendlyEntry entry)
        {
            foreach (var c in doc.Collections)
            {
                var match = c.Entries.FirstOrDefault(e => string.Equals(e.Key, entryKey, StringComparison.OrdinalIgnoreCase));
                if (match is not null)
                {
                    entry = match;
                    return true;
                }
            }

            entry = null!;
            return false;
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
        }
    }
}
