using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Models
{
    public sealed class XmlFriendlyChildCollection
    {
        public XmlFriendlyChildCollection(string name, IEnumerable<XElement> items)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Items = items.Select(x => new XmlFriendlyChildItem(x)).ToList();
        }

        public string Name { get; }

        public List<XmlFriendlyChildItem> Items { get; }
    }
}
    