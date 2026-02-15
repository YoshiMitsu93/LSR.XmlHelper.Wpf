using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Editing
{
    public sealed class XmlArrayReplaceByKeyService
    {
        public XDocument MergeReplace(XDocument baseDoc, XDocument addDoc, string itemElementName, string keyElementName)
        {
            if (addDoc?.Root is null)
                return baseDoc ?? new XDocument(new XElement("Root"));

            if (baseDoc?.Root is null)
                return new XDocument(addDoc);

            itemElementName = (itemElementName ?? "").Trim();
            keyElementName = (keyElementName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(itemElementName) || string.IsNullOrWhiteSpace(keyElementName))
                return baseDoc;

            var baseRoot = baseDoc.Root;
            var addRoot = addDoc.Root;

            var incomingItems = addRoot
                .Elements(itemElementName)
                .ToList();

            if (incomingItems.Count == 0)
                return baseDoc;

            var incomingKeys = incomingItems
                .Select(x => ((string?)x.Element(keyElementName) ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            if (incomingKeys.Count == 0)
                return baseDoc;

            var existingMatches = baseRoot
                .Elements(itemElementName)
                .Where(x => incomingKeys.Contains(((string?)x.Element(keyElementName) ?? "").Trim()))
                .ToList();

            foreach (var m in existingMatches)
                m.Remove();

            foreach (var incoming in incomingItems)
                baseRoot.Add(new XElement(incoming));

            return baseDoc;
        }
    }
}
