using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class DispatchableVehicleGroupAppendToMainService
    {
        public (bool Ok, string Message, string WrittenPath) AppendToMain(string rootFolderPath, XDocument singleGroupDoc)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return (false, "Root folder path is invalid.", "");

            if (singleGroupDoc?.Root is null)
                return (false, "Vehicle group document is missing a root element.", "");

            var group = singleGroupDoc.Descendants("DispatchableVehicleGroup").FirstOrDefault();
            if (group is null)
                return (false, "Could not find DispatchableVehicleGroup element to append.", "");

            var groupId = ((string?)group.Element("DispatchableVehicleGroupID") ?? "").Trim();
            if (string.IsNullOrWhiteSpace(groupId))
                return (false, "DispatchableVehicleGroup is missing DispatchableVehicleGroupID.", "");

            var mainPath = Path.Combine(rootFolderPath, "DispatchableVehicles.xml");
            if (!File.Exists(mainPath))
                return (false, "DispatchableVehicles.xml was not found in the root folder.", "");

            XDocument mainDoc;
            try
            {
                mainDoc = XDocument.Load(mainPath, LoadOptions.None);
            }
            catch (Exception ex)
            {
                return (false, $"Failed to load DispatchableVehicles.xml: {ex.Message}", "");
            }

            if (mainDoc.Root is null)
                return (false, "DispatchableVehicles.xml has no root element.", "");

            var exists = mainDoc.Descendants("DispatchableVehicleGroup")
                .Any(x => string.Equals((((string?)x.Element("DispatchableVehicleGroupID") ?? "").Trim()), groupId, StringComparison.OrdinalIgnoreCase));

            if (exists)
                return (false, $"DispatchableVehicles.xml already contains a DispatchableVehicleGroup with ID '{groupId}'.", "");

            mainDoc.Root.Add(new XElement(group));
            mainDoc.Save(mainPath);

            return (true, "OK", mainPath);
        }
    }
}
