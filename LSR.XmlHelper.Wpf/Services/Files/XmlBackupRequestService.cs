using LSR.XmlHelper.Core.Services;
using System;

namespace LSR.XmlHelper.Wpf.Services
{
    public sealed class XmlBackupRequestService
    {
        public bool TryBackup(string xmlPath, out string? error)
        {
            error = null;

            try
            {
                var root = new XmlHelperRootService();
                var backup = new XmlBackupService(root);
                backup.Backup(xmlPath);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
