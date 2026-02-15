using System;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Editing
{
    public sealed class GangDensReplaceService
    {
        public XDocument MergeReplaceForGang(XDocument baseDoc, XDocument addDoc, string gangId)
        {
            if (baseDoc?.Root is null)
                return addDoc?.Root is null ? new XDocument(new XElement("PossibleLocations")) : new XDocument(addDoc);

            if (addDoc?.Root is null)
                return baseDoc;

            var baseRoot = baseDoc.Root;
            var addRoot = addDoc.Root;

            var baseGangDens = baseRoot.Element("GangDens");
            if (baseGangDens is null)
            {
                baseGangDens = new XElement("GangDens");
                baseRoot.Add(baseGangDens);
            }

            var incomingDens = addRoot
                .Descendants("GangDen")
                .ToList();

            if (incomingDens.Count == 0)
                return baseDoc;

            var incomingGangIds = incomingDens
                .Select(d => ((string?)d.Element("AssignedAssociationID") ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (incomingGangIds.Length == 0)
                return baseDoc;

            foreach (var incomingGangId in incomingGangIds)
            {
                var existingDens = baseGangDens
                    .Elements("GangDen")
                    .Where(d => string.Equals(((string?)d.Element("AssignedAssociationID") ?? "").Trim(), incomingGangId, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                foreach (var d in existingDens)
                    d.Remove();

                foreach (var d in incomingDens.Where(x => string.Equals(((string?)x.Element("AssignedAssociationID") ?? "").Trim(), incomingGangId, StringComparison.OrdinalIgnoreCase)))
                    baseGangDens.Add(new XElement(d));
            }

            return baseDoc;
        }
    }
}
