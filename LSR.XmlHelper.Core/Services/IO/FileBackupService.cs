using System;
using System.IO;

namespace LSR.XmlHelper.Core.Services.IO
{
    public sealed class FileBackupService
    {
        public (bool Ok, string BackupPath, string Message) TryBackup(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return (false, "", "File path was empty.");

            if (!File.Exists(filePath))
                return (false, "", "File does not exist.");

            var dir = Path.GetDirectoryName(filePath) ?? "";
            var name = Path.GetFileName(filePath);
            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupPath = Path.Combine(dir, $"{name}.bak_{stamp}");

            try
            {
                File.Copy(filePath, backupPath, false);
                return (true, backupPath, "OK");
            }
            catch (Exception ex)
            {
                return (false, "", ex.Message);
            }
        }
    }
}
