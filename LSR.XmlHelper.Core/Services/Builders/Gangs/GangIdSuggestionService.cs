using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangIdSuggestionService
    {
        public string Suggest(string packName, string fullName, IReadOnlyCollection<string> existingIds)
        {
            var baseText = !string.IsNullOrWhiteSpace(packName) ? packName : fullName;
            if (string.IsNullOrWhiteSpace(baseText))
                baseText = "NewGang";

            var cleaned = Regex.Replace(baseText, @"[^A-Za-z0-9]+", "");
            if (string.IsNullOrWhiteSpace(cleaned))
                cleaned = "NewGang";

            var candidate = cleaned;

            if (existingIds is null || existingIds.Count == 0)
                return candidate;

            var set = new HashSet<string>(existingIds, StringComparer.OrdinalIgnoreCase);

            if (!set.Contains(candidate))
                return candidate;

            var i = 2;
            while (true)
            {
                var next = $"{candidate}{i}";
                if (!set.Contains(next))
                    return next;

                i++;
            }
        }
    }
}
