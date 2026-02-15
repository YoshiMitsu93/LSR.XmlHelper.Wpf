using System;

namespace LSR.XmlHelper.Core.Services.Resolvers
{
    public static class LsrConfigNameNormalizer
    {
        public static string Normalize(string? configName)
        {
            var normalized = (configName ?? "").Trim();
            return string.IsNullOrWhiteSpace(normalized) ? "Default" : normalized;
        }
    }
}
