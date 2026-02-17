using System;
using System.Collections.Generic;
using System.Linq;
using LSR.XmlHelper.Wpf.ViewModels.Builders;

namespace LSR.XmlHelper.Wpf.Services.Builders.Dispatchables
{
    public sealed class DispatchablePersonEntryCloneService
    {
        public DispatchablePersonEntryViewModel Clone(DispatchablePersonEntryViewModel template, string newDebugName)
        {
            if (template is null)
                throw new ArgumentNullException(nameof(template));

            var fields = new List<DispatchablePersonFieldViewModel>();

            foreach (var f in template.Fields)
            {
                if (f is null)
                    continue;

                fields.Add(new DispatchablePersonFieldViewModel(f.Name, f.Value, f.IsXml));
            }

            var sourceDebugName = (template.DebugName ?? "").Trim();
            var created = new DispatchablePersonEntryViewModel(sourceDebugName, -1, fields);
            created.DebugName = newDebugName ?? "";
            return created;
        }

        public string SuggestNextDebugName(IReadOnlyCollection<DispatchablePersonEntryViewModel> existingEntries, string baseName)
        {
            baseName = (baseName ?? "").Trim();
            if (string.IsNullOrWhiteSpace(baseName))
                baseName = "NewPed";

            existingEntries ??= Array.Empty<DispatchablePersonEntryViewModel>();

            var existing = existingEntries
                .Select(x => (x?.DebugName ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (!existing.Contains(baseName))
                return baseName;

            var prefix = baseName;
            var digitStartIndex = baseName.Length;

            for (var i = baseName.Length - 1; i >= 0; i--)
            {
                if (!char.IsDigit(baseName[i]))
                    break;

                digitStartIndex = i;
            }

            if (digitStartIndex < baseName.Length)
            {
                var p = baseName.Substring(0, digitStartIndex).Trim();
                if (!string.IsNullOrWhiteSpace(p))
                    prefix = p;
            }

            for (var n = 1; n < 5000; n++)
            {
                var candidate = prefix + n;
                if (!existing.Contains(candidate))
                    return candidate;
            }

            return prefix + Guid.NewGuid().ToString("N").Substring(0, 6);
        }
    }
}
