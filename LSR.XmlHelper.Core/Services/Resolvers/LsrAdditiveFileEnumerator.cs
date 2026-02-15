using System;
using System.IO;
using System.Linq;

namespace LSR.XmlHelper.Core.Services.Resolvers
{
    public sealed class LsrAdditiveFileEnumerator
    {
        public string[] GetAdditivePaths(string rootFolderPath, string additivePattern)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return Array.Empty<string>();

            if (string.IsNullOrWhiteSpace(additivePattern))
                return Array.Empty<string>();

            return Directory
                .EnumerateFiles(rootFolderPath, additivePattern, SearchOption.TopDirectoryOnly)
                .Select(p => new FileInfo(p))
                .Where(f => f.Exists)
                .OrderByDescending(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .Select(f => f.FullName)
                .ToArray();
        }
    }
}
