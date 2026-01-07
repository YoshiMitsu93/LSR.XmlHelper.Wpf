using LSR.XmlHelper.Wpf.Services.EditHistory;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.Services.Compare
{
    public sealed class CompareEditsApplyService
    {
        private readonly EditHistoryService _history;
        private readonly XmlFileSaveService _saver;
        private readonly XmlBackupRequestService _backup;

        public CompareEditsApplyService(EditHistoryService history, XmlFileSaveService saver, XmlBackupRequestService backup)
        {
            _history = history;
            _saver = saver;
            _backup = backup;
        }

        public bool TryApplyAndSave(string targetPath, string targetXmlText, IReadOnlyList<EditHistoryItem> edits, out string? error)
        {
            error = null;

            if (!_history.TryApplyToXmlText(targetXmlText, edits, out var updated, out var applyError))
            {
                error = applyError ?? "Compare edits could not be applied.";
                return false;
            }

            if (!_backup.TryBackup(targetPath, out var backupErr))
            {
                error = backupErr ?? "Backup failed.";
                return false;
            }

            var (ok, saveErr) = _saver.Save(targetPath, updated);
            if (!ok)
            {
                error = saveErr ?? "Save failed.";
                return false;
            }

            return true;
        }
    }
}
