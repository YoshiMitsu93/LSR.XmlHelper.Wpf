using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Models
{
    public sealed class XmlFriendlyChildCollection
    {
        public XmlFriendlyChildCollection(string name, IEnumerable<XElement> itemElements)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Items = itemElements.Select(e => new XmlFriendlyChildItem(e)).ToList();
        }

        public string Name { get; }

        public List<XmlFriendlyChildItem> Items { get; }
    }
}
