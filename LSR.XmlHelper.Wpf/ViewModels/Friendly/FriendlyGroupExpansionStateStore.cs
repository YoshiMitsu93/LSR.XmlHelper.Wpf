using System;
using System.Collections.Generic;

namespace LSR.XmlHelper.Wpf.ViewModels
{
    public sealed class FriendlyGroupExpansionStateStore
    {
        private readonly Dictionary<string, bool> _states = new(StringComparer.OrdinalIgnoreCase);

        public void Capture(IEnumerable<object> groups)
        {
            foreach (var group in groups)
            {
                if (group is XmlFriendlyFieldGroupViewModel fieldGroup)
                {
                    _states[$"F:{fieldGroup.Title}"] = fieldGroup.IsExpanded;
                    continue;
                }

                if (group is XmlFriendlyLookupGroupViewModel lookupGroup)
                    _states[$"L:{lookupGroup.Title}"] = lookupGroup.IsExpanded;
            }
        }

        public void Apply(IEnumerable<object> groups)
        {
            foreach (var group in groups)
            {
                if (group is XmlFriendlyFieldGroupViewModel fieldGroup)
                {
                    var key = $"F:{fieldGroup.Title}";
                    if (_states.TryGetValue(key, out var expanded))
                        fieldGroup.IsExpanded = expanded;
                    else
                        _states[key] = fieldGroup.IsExpanded;

                    continue;
                }

                if (group is XmlFriendlyLookupGroupViewModel lookupGroup)
                {
                    var key = $"L:{lookupGroup.Title}";
                    if (_states.TryGetValue(key, out var expanded))
                        lookupGroup.IsExpanded = expanded;
                    else
                        _states[key] = lookupGroup.IsExpanded;
                }
            }
        }

        public void Clear()
        {
            _states.Clear();
        }
    }
}
