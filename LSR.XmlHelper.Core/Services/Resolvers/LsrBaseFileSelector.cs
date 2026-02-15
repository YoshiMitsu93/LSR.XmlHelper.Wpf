using System;
using System.IO;
using System.Linq;

namespace LSR.XmlHelper.Core.Services.Resolvers
{
    public sealed class LsrBaseFileSelector
    {
        public string? ResolveBaseFile(string rootFolderPath, string baseFileName, string wildcardWhenConfigEmpty, Func<string, string> fileNameForConfig, string? configName)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return null;

            var normalizedConfig = LsrConfigNameNormalizer.Normalize(configName);

            var fileName = string.IsNullOrEmpty(configName) ? wildcardWhenConfigEmpty : fileNameForConfig(configName);

            var lsrCandidate = Directory
                .EnumerateFiles(rootFolderPath, fileName, SearchOption.TopDirectoryOnly)
                .Select(p => new FileInfo(p))
                .Where(f => f.Exists && !f.Name.Contains("+", StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.Name, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();

            if (lsrCandidate != null && !string.Equals(normalizedConfig, "Default", StringComparison.OrdinalIgnoreCase))
                return lsrCandidate.FullName;

            var basePath = Path.Combine(rootFolderPath, baseFileName);
            if (File.Exists(basePath))
                return basePath;

            return null;
        }
    }
}
