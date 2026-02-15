using System;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangPackAppendToMainXmlsService
    {
        public (bool Ok, string Message) AppendGangs(XDocument mainDoc, XDocument additiveDoc)
        {
            return AppendSingleRecordById(mainDoc, additiveDoc, "Gang", "ID");
        }

        public (bool Ok, string Message) AppendDispatchablePeople(XDocument mainDoc, XDocument additiveDoc)
        {
            return AppendSingleRecordById(mainDoc, additiveDoc, "DispatchablePersonGroup", "DispatchablePersonGroupID");
        }

        public (bool Ok, string Message) AppendDispatchableVehicles(XDocument mainDoc, XDocument additiveDoc)
        {
            return AppendSingleRecordById(mainDoc, additiveDoc, "DispatchableVehicleGroup", "DispatchableVehicleGroupID");
        }

        public (bool Ok, string Message) AppendIssuableWeapons(XDocument mainDoc, XDocument additiveDoc)
        {
            return AppendSingleRecordById(mainDoc, additiveDoc, "IssuableWeaponsGroup", "IssuableWeaponsID");
        }

        public (bool Ok, string Message) AppendGangTerritories(XDocument mainDoc, XDocument additiveDoc)
        {
            var mainRoot = mainDoc.Root;
            var addRoot = additiveDoc.Root;

            if (mainRoot is null || addRoot is null)
                return (false, "Missing root element.");

            var territories = addRoot.Elements("GangTerritory").ToList();
            if (territories.Count == 0)
                return (true, "Nothing to append.");

            foreach (var t in territories)
            {
                var gangId = ((string?)t.Element("GangID") ?? "").Trim();
                var zone = ((string?)t.Element("ZoneName") ?? "").Trim();

                if (string.IsNullOrWhiteSpace(gangId) || string.IsNullOrWhiteSpace(zone))
                    continue;

                var exists = mainRoot.Elements("GangTerritory").Any(x =>
                    string.Equals((((string?)x.Element("GangID") ?? "").Trim()), gangId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals((((string?)x.Element("ZoneName") ?? "").Trim()), zone, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                    mainRoot.Add(new XElement(t));
            }

            return (true, "OK");
        }

        public (bool Ok, string Message) MergeLocationsGangDens(XDocument mainDoc, XDocument additiveDoc)
        {
            var mainRoot = mainDoc.Root;
            var addRoot = additiveDoc.Root;

            if (mainRoot is null || addRoot is null)
                return (false, "Missing root element.");

            var mainGangDens = mainDoc.Descendants("GangDens").FirstOrDefault();
            if (mainGangDens is null)
            {
                mainGangDens = new XElement("GangDens");
                mainRoot.Add(mainGangDens);
            }

            var densToAdd = additiveDoc.Descendants("GangDen").ToList();
            if (densToAdd.Count == 0)
                return (true, "Nothing to merge.");

            foreach (var den in densToAdd)
            {
                var gangId = ((string?)den.Element("GangID") ?? "").Trim();
                var name = ((string?)den.Element("Name") ?? "").Trim();

                if (string.IsNullOrWhiteSpace(gangId) || string.IsNullOrWhiteSpace(name))
                    continue;

                var exists = mainGangDens.Elements("GangDen").Any(x =>
                    string.Equals((((string?)x.Element("GangID") ?? "").Trim()), gangId, StringComparison.OrdinalIgnoreCase)
                    && string.Equals((((string?)x.Element("Name") ?? "").Trim()), name, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                    mainGangDens.Add(new XElement(den));
            }

            return (true, "OK");
        }

        public (bool Ok, string Message) MergeShopMenus(XDocument mainDoc, XDocument additiveDoc)
        {
            var mainRoot = mainDoc.Root;
            var addRoot = additiveDoc.Root;

            if (mainRoot is null || addRoot is null)
                return (false, "Missing root element.");

            var addGroups = additiveDoc.Descendants("ShopMenuGroup").ToList();
            var addMenus = additiveDoc.Descendants("ShopMenu").ToList();

            if (addGroups.Count == 0 && addMenus.Count == 0)
                return (true, "Nothing to merge.");

            var mainGroupList = mainDoc.Descendants("ShopMenuGroupList").FirstOrDefault();
            if (mainGroupList is null)
            {
                mainGroupList = new XElement("ShopMenuGroupList");
                mainRoot.Add(mainGroupList);
            }

            foreach (var g in addGroups)
            {
                var id = ((string?)g.Element("ID") ?? "").Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var exists = mainGroupList.Elements("ShopMenuGroup")
                    .Any(x => string.Equals((((string?)x.Element("ID") ?? "").Trim()), id, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                    mainGroupList.Add(new XElement(g));
            }

            var mainMenuList = mainDoc.Descendants("ShopMenuList").FirstOrDefault();
            if (mainMenuList is null)
            {
                mainMenuList = new XElement("ShopMenuList");
                mainRoot.Add(mainMenuList);
            }

            foreach (var m in addMenus)
            {
                var id = ((string?)m.Element("ShopMenuID") ?? (string?)m.Element("ShopMenuId") ?? "").Trim();
                if (string.IsNullOrWhiteSpace(id))
                    continue;

                var exists = mainMenuList.Elements("ShopMenu")
                    .Any(x =>
                        string.Equals((((string?)x.Element("ShopMenuID") ?? (string?)x.Element("ShopMenuId") ?? "").Trim()),
                            id, StringComparison.OrdinalIgnoreCase));

                if (!exists)
                    mainMenuList.Add(new XElement(m));
            }

            return (true, "OK");
        }

        private static (bool Ok, string Message) AppendSingleRecordById(XDocument mainDoc, XDocument additiveDoc, string recordElementName, string idElementName)
        {
            var mainRoot = mainDoc.Root;
            var addRoot = additiveDoc.Root;

            if (mainRoot is null || addRoot is null)
                return (false, "Missing root element.");

            var record = addRoot.Elements(recordElementName).FirstOrDefault();
            if (record is null)
                return (false, $"Additive doc does not contain '{recordElementName}' element.");

            var id = ((string?)record.Element(idElementName) ?? "").Trim();
            if (string.IsNullOrWhiteSpace(id))
                return (false, $"'{recordElementName}' is missing '{idElementName}'.");

            var exists = mainRoot.Elements(recordElementName)
                .Any(x => string.Equals((((string?)x.Element(idElementName) ?? "").Trim()), id, StringComparison.OrdinalIgnoreCase));

            if (exists)
                return (false, $"Main XML already contains '{recordElementName}' with {idElementName} '{id}'.");

            mainRoot.Add(new XElement(record));
            return (true, "OK");
        }
    }
}
