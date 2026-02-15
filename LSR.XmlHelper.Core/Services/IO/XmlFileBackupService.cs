using System;
using System.Globalization;
using System.IO;

namespace LSR.XmlHelper.Core.Services.IO
{
    public sealed class XmlFileBackupService
    {
        public string? CreateBackup(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return null;

            if (!File.Exists(filePath))
                return null;

            var folder = Path.GetDirectoryName(filePath);
            if (string.IsNullOrWhiteSpace(folder))
                return null;

            var backupFolder = Path.Combine(folder, "Backups");
            Directory.CreateDirectory(backupFolder);

            var name = Path.GetFileNameWithoutExtension(filePath);
            var ext = Path.GetExtension(filePath);

            var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss", CultureInfo.InvariantCulture);
            var backupName = name + "_" + stamp + ext;

            var backupPath = Path.Combine(backupFolder, backupName);

            File.Copy(filePath, backupPath, true);

            return backupPath;
        }
    }
}
