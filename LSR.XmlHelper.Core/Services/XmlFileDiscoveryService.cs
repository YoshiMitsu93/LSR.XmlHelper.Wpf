using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class XmlFileDiscoveryService
    {
        public IReadOnlyList<string> GetXmlFiles(string rootFolder, bool includeSubfolders)
        {
            if (string.IsNullOrWhiteSpace(rootFolder))
                throw new ArgumentException("Root folder is required.", nameof(rootFolder));

            if (!Directory.Exists(rootFolder))
                throw new DirectoryNotFoundException($"Root folder does not exist: {rootFolder}");

            var option = includeSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

            return Directory.EnumerateFiles(rootFolder, "*.xml", option)
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }
    }
}
