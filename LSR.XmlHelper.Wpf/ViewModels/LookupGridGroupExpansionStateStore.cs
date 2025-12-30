using System;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class LookupGridGroupExpansionStateStore
    {
        private readonly Dictionary<string, bool> _states = new(StringComparer.OrdinalIgnoreCase);

        public bool TryGet(string key, out bool expanded) => _states.TryGetValue(key, out expanded);

        public void Set(string key, bool expanded) => _states[key] = expanded;

        public void Clear() => _states.Clear();
    }
}
