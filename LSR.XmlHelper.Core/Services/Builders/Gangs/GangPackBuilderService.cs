using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangPackBuilderService
    {
        public (bool ok, string message, GangPackBuildResult? result) BuildCloneFirst(
            string rootFolderPath,
            string packName,
            string newGangId,
            string newGangFullName,
            string cloneFromGangId)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (false, "Root folder path is missing or invalid.", null);

            if (string.IsNullOrWhiteSpace(packName))
                return (false, "Pack name is required.", null);

            if (string.IsNullOrWhiteSpace(newGangId))
                return (false, "New gang ID is required.", null);

            if (string.IsNullOrWhiteSpace(newGangFullName))
                return (false, "Gang full name is required.", null);

            if (string.IsNullOrWhiteSpace(cloneFromGangId))
                return (false, "Clone-from gang ID is required for clone-first build.", null);

            var fileSetResolver = new LSR.XmlHelper.Core.Services.LsrFileSetResolverService();

            var gangsFiles = fileSetResolver.ResolveGangs(rootFolderPath, "Default")
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var peopleFiles = fileSetResolver.ResolveDispatchablePeople(rootFolderPath, "Default")
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var vehicleFiles = fileSetResolver.ResolveDispatchableVehicles(rootFolderPath, "Default")
                .EnumerateReadOrder()
                .Where(p => !string.IsNullOrWhiteSpace(p) && File.Exists(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var resolver = new LSR.XmlHelper.Core.Services.LsrConfigFileResolverService();
            var resolvedGangPath = resolver.ResolveGangFile(rootFolderPath, cloneFromGangId, "Default");

            if (string.IsNullOrWhiteSpace(resolvedGangPath) || !File.Exists(resolvedGangPath))
                return (false, $"Could not find Gang with ID '{cloneFromGangId}' in any active Gangs file.", null);

            XDocument gangDoc;

            try
            {
                gangDoc = XDocument.Load(resolvedGangPath, LoadOptions.None);
            }
            catch
            {
                return (false, $"Could not read Gang with ID '{cloneFromGangId}' from file: {resolvedGangPath}", null);
            }

            var gangElement = gangDoc
                .Descendants("Gang")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), cloneFromGangId.Trim(), StringComparison.OrdinalIgnoreCase));

            if (gangElement is null)
                return (false, $"Could not find Gang with ID '{cloneFromGangId}' in file: {resolvedGangPath}", null);


            var sourcePeopleGroupId = (string?)gangElement.Element("PersonnelID") ?? "";
            var sourceVehicleGroupId = (string?)gangElement.Element("VehiclesID") ?? "";
            var sourceDealerMenuGroupId = (string?)gangElement.Element("DealerMenuGroup") ?? "";
            var sourceMeleeWeaponsId = (string?)gangElement.Element("MeleeWeaponsID") ?? "";
            var sourceSideArmsId = (string?)gangElement.Element("SideArmsID") ?? "";
            var sourceLongGunsId = (string?)gangElement.Element("LongGunsID") ?? "";

            if (string.IsNullOrWhiteSpace(sourcePeopleGroupId))
                return (false, $"Clone source gang '{cloneFromGangId}' is missing PersonnelID.", null);

            if (string.IsNullOrWhiteSpace(sourceVehicleGroupId))
                return (false, $"Clone source gang '{cloneFromGangId}' is missing VehiclesID.", null);

            var (peopleDoc, peopleGroupElement) = FindGroupElement(peopleFiles, "DispatchablePersonGroup", "DispatchablePersonGroupID", sourcePeopleGroupId);
            if (peopleDoc is null || peopleGroupElement is null)
                return (false, $"Could not find DispatchablePersonGroup with ID '{sourcePeopleGroupId}' in any DispatchablePeople*.xml file.", null);

            var (vehicleDoc, vehicleGroupElement) = FindGroupElement(vehicleFiles, "DispatchableVehicleGroup", "DispatchableVehicleGroupID", sourceVehicleGroupId);
            if (vehicleDoc is null || vehicleGroupElement is null)
                return (false, $"Could not find DispatchableVehicleGroup with ID '{sourceVehicleGroupId}' in any DispatchableVehicles*.xml file.", null);

            var existingGangIds = GetAllIds(gangsFiles, "Gang", "ID");
            var existingPeopleGroupIds = GetAllIds(peopleFiles, "DispatchablePersonGroup", "DispatchablePersonGroupID");
            var existingVehicleGroupIds = GetAllIds(vehicleFiles, "DispatchableVehicleGroup", "DispatchableVehicleGroupID");

            var finalGangId = MakeUniqueId(newGangId, existingGangIds);

            var finalPeopleGroupId = MakeUniqueId($"{finalGangId}_PEOPLE", existingPeopleGroupIds);
            var finalVehicleGroupId = MakeUniqueId($"{finalGangId}_VEHICLES", existingVehicleGroupIds);

            var newGangElement = new XElement(gangElement);
            SetOrCreate(newGangElement, "ID", finalGangId);
            SetOrCreate(newGangElement, "FullName", newGangFullName);

            if (newGangElement.Element("ShortName") is not null)
                SetOrCreate(newGangElement, "ShortName", newGangFullName);
            SetOrCreate(newGangElement, "ContactName", newGangFullName);
            SetOrCreate(newGangElement, "MemberName", $"{newGangFullName} Member");

            SetOrCreate(newGangElement, "PersonnelID", finalPeopleGroupId);
            SetOrCreate(newGangElement, "VehiclesID", finalVehicleGroupId);

            var newPeopleGroupElement = new XElement(peopleGroupElement);
            SetOrCreate(newPeopleGroupElement, "DispatchablePersonGroupID", finalPeopleGroupId);

            var newVehicleGroupElement = new XElement(vehicleGroupElement);
            SetOrCreate(newVehicleGroupElement, "DispatchableVehicleGroupID", finalVehicleGroupId);

            var gangsOut = CreateSingleElementDocument(gangDoc, gangElement.Name, newGangElement);
            var peopleOut = CreateSingleElementDocument(peopleDoc, peopleGroupElement.Name, newPeopleGroupElement);
            var vehiclesOut = CreateSingleElementDocument(vehicleDoc, vehicleGroupElement.Name, newVehicleGroupElement);

            var result = new GangPackBuildResult(
                gangsOut,
                peopleOut,
                vehiclesOut,
                finalGangId,
                finalPeopleGroupId,
                finalVehicleGroupId,
                sourceDealerMenuGroupId,
                sourceMeleeWeaponsId,
                sourceSideArmsId,
                sourceLongGunsId);


            return (true, "OK", result);
        }

        private static (XDocument? doc, XElement? gangElement) FindGangElement(string[] files, string gangId)
        {
            for (int i = files.Length - 1; i >= 0; i--)
            {
                var file = files[i];
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                var match = doc.Descendants("Gang")
                    .FirstOrDefault(x => string.Equals((string?)x.Element("ID"), gangId, StringComparison.OrdinalIgnoreCase));

                if (match is not null)
                    return (doc, match);
            }

            return (null, null);
        }

        private static (XDocument? doc, XElement? groupElement) FindGroupElement(string[] files, string elementName, string idElementName, string idValue)
        {
            for (int i = files.Length - 1; i >= 0; i--)
            {
                var file = files[i];
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                var match = doc.Descendants(elementName)
                    .FirstOrDefault(x => string.Equals((string?)x.Element(idElementName), idValue, StringComparison.OrdinalIgnoreCase));

                if (match is not null)
                    return (doc, match);
            }

            return (null, null);
        }

        private static HashSet<string> GetAllIds(string[] files, string elementName, string idElementName)
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var file in files)
            {
                XDocument doc;
                try
                {
                    doc = XDocument.Load(file, LoadOptions.None);
                }
                catch
                {
                    continue;
                }

                foreach (var e in doc.Descendants(elementName))
                {
                    var id = (string?)e.Element(idElementName);
                    if (!string.IsNullOrWhiteSpace(id))
                        set.Add(id);
                }
            }

            return set;
        }

        private static string MakeUniqueId(string desired, HashSet<string> existing)
        {
            if (!existing.Contains(desired))
                return desired;

            var i = 2;
            while (true)
            {
                var next = $"{desired}{i}";
                if (!existing.Contains(next))
                    return next;

                i++;
            }
        }

        private static void SetOrCreate(XElement parent, string childName, string value)
        {
            var child = parent.Element(childName);
            if (child is null)
                parent.Add(new XElement(childName, value));
            else
                child.Value = value;
        }

        private static XDocument CreateSingleElementDocument(XDocument sourceDoc, XName recordElementName, XElement recordElement)
        {
            var root = sourceDoc.Root;
            if (root is null)
                return new XDocument(recordElement);

            var rootName = root.Name;

            var doc = new XDocument();
            if (sourceDoc.Declaration is not null)
                doc.Declaration = new XDeclaration(sourceDoc.Declaration);

            doc.Add(new XElement(rootName, recordElement));

            return doc;
        }
    }

    public sealed class GangPackBuildResult
    {
        public GangPackBuildResult(
             XDocument gangsDoc,
             XDocument peopleDoc,
             XDocument vehiclesDoc,
             string gangId,
             string peopleGroupId,
             string vehicleGroupId,
             string sourceDealerMenuGroupId,
             string sourceMeleeWeaponsId,
             string sourceSideArmsId,
             string sourceLongGunsId)
        {
            GangsDoc = gangsDoc;
            PeopleDoc = peopleDoc;
            VehiclesDoc = vehiclesDoc;
            GangId = gangId;
            PeopleGroupId = peopleGroupId;
            VehicleGroupId = vehicleGroupId;
            SourceDealerMenuGroupId = sourceDealerMenuGroupId;
            SourceMeleeWeaponsId = sourceMeleeWeaponsId;
            SourceSideArmsId = sourceSideArmsId;
            SourceLongGunsId = sourceLongGunsId;
        }

        public XDocument GangsDoc { get; }
        public XDocument PeopleDoc { get; }
        public XDocument VehiclesDoc { get; }
        public string GangId { get; }
        public string PeopleGroupId { get; }
        public string VehicleGroupId { get; }
        public string SourceDealerMenuGroupId { get; }
        public string SourceMeleeWeaponsId { get; }
        public string SourceSideArmsId { get; }
        public string SourceLongGunsId { get; }

    }
}
