using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Models
{
    public sealed class XmlFriendlyChildItem
    {
        public XmlFriendlyChildItem(XElement element)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));

            var dict = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var child in Element.Elements())
            {
                if (!child.HasElements)
                {
                    dict[child.Name.LocalName] = child;
                }
            }

            Fields = dict;
        }

        public XElement Element { get; }

        public IReadOnlyDictionary<string, XElement> Fields { get; }

        public string? GetValue(string fieldName)
        {
            if (Fields.TryGetValue(fieldName, out var el))
                return el.Value;

            return null;
        }

        public bool TrySetValue(string fieldName, string? value, out string? error)
        {
            error = null;

            if (!Fields.TryGetValue(fieldName, out var el))
            {
                error = $"Field '{fieldName}' not found on child item '{Element.Name.LocalName}'.";
                return false;
            }

            try
            {
                el.Value = value ?? string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }
    }
}
