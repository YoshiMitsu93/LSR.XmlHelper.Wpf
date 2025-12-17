using System;
using System.Globalization;
using System.IO;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class XmlBackupService
    {
        private readonly XmlHelperRootService _root;

        public XmlBackupService(XmlHelperRootService root)
        {
            _root = root;
        }

        public void Backup(string xmlPath)
        {
            if (!File.Exists(xmlPath))
                return;

            var backupDir = _root.GetOrCreateSubfolder(xmlPath, "BackupXMLs");

            var fileName = Path.GetFileName(xmlPath);
            var baseName = Path.GetFileNameWithoutExtension(fileName);

            var stampRaw = DateTime.Now.ToString("g", CultureInfo.CurrentCulture);
            var stampSafe = MakeSafeStamp(stampRaw);

            var backupName = $"{baseName}_{stampSafe}.xml";
            var backupPath = Path.Combine(backupDir, backupName);

            var idx = 1;
            while (File.Exists(backupPath))
            {
                backupName = $"{baseName}_{stampSafe}_{idx}.xml";
                backupPath = Path.Combine(backupDir, backupName);
                idx++;
            }

            File.Copy(xmlPath, backupPath);
        }

        private static string MakeSafeStamp(string value)
        {
            Span<char> buffer = stackalloc char[value.Length];
            var count = 0;

            foreach (var ch in value)
            {
                if (char.IsLetterOrDigit(ch))
                {
                    buffer[count++] = ch;
                    continue;
                }

                buffer[count++] = '-';
            }

            var s = new string(buffer[..count]).Trim('-');

            while (s.Contains("--", StringComparison.Ordinal))
                s = s.Replace("--", "-", StringComparison.Ordinal);

            return s.Length == 0 ? "time" : s;
        }
    }
}
