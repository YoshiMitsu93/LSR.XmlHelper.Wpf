using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Models
{
    public sealed class XmlFriendlyChildItem
    {
        public XmlFriendlyChildItem(XElement element)
        {
            Element = element ?? throw new ArgumentNullException(nameof(element));

            var leafs = new Dictionary<string, XElement>(StringComparer.OrdinalIgnoreCase);

            foreach (var child in Element.Elements())
            {
                if (!child.HasElements)
                {
                    leafs[child.Name.LocalName] = child;
                }
            }

            LeafFields = leafs;
        }

        public XElement Element { get; }

        public IReadOnlyDictionary<string, XElement> LeafFields { get; }

        public string GetValueOrEmpty(string fieldName)
        {
            if (LeafFields.TryGetValue(fieldName, out var el))
                return el.Value ?? string.Empty;

            return string.Empty;
        }

        public bool TrySetValue(string fieldName, string newValue, out string? error)
        {
            error = null;

            if (!LeafFields.TryGetValue(fieldName, out var el))
            {
                error = $"Field '{fieldName}' was not found.";
                return false;
            }

            try
            {
                el.Value = newValue ?? string.Empty;
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
