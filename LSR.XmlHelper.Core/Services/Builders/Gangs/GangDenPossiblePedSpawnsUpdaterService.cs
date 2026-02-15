using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;

namespace LSR.XmlHelper.Core.Services.Builders
{
    public sealed class GangDenPossiblePedSpawnsUpdaterService
    {
        public void Apply(XDocument locationsDoc, string gangId, List<PossiblePedSpawnModel> spawns)
        {
            if (locationsDoc?.Root is null)
                return;

            if (string.IsNullOrWhiteSpace(gangId))
                return;

            spawns ??= new List<PossiblePedSpawnModel>();

            var dens = locationsDoc.Descendants("GangDen")
                .Where(d => string.Equals(d.Element("AssignedAssociationID")?.Value ?? "", gangId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var den in dens)
            {
                var denName = den.Element("Name")?.Value ?? "";
                var denFullName = den.Element("FullName")?.Value ?? "";

                var denSpawns = spawns
                    .Where(s =>
                        string.Equals((s.DenName ?? "").Trim(), denName.Trim(), StringComparison.OrdinalIgnoreCase) ||
                        string.Equals((s.DenName ?? "").Trim(), denFullName.Trim(), StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var container = den.Element("PossiblePedSpawns");
                if (container is null)
                {
                    container = new XElement("PossiblePedSpawns");
                    den.Add(container);
                }
                else
                {
                    container.RemoveNodes();
                }

                foreach (var spawn in denSpawns)
                {
                    var element = spawn.SourceElement is null ? CreateNewSpawnElement() : new XElement(spawn.SourceElement);

                    SetOrCreate(element, "Heading", FormatDouble(spawn.Heading));
                    SetOrCreate(element, "Percentage", spawn.Percentage.ToString(CultureInfo.InvariantCulture));
                    SetOrCreate(element, "TaskRequirements", spawn.TaskRequirements ?? "");
                    SetOrCreate(element, "MinHourSpawn", spawn.MinHourSpawn.ToString(CultureInfo.InvariantCulture));
                    SetOrCreate(element, "MaxHourSpawn", spawn.MaxHourSpawn.ToString(CultureInfo.InvariantCulture));
                    SetOrCreate(element, "MinWantedLevelSpawn", spawn.MinWantedLevelSpawn.ToString(CultureInfo.InvariantCulture));
                    SetOrCreate(element, "MaxWantedLevelSpawn", spawn.MaxWantedLevelSpawn.ToString(CultureInfo.InvariantCulture));
                    SetOrCreate(element, "LongGunAlwaysEquipped", spawn.LongGunAlwaysEquipped ? "true" : "false");

                    var location = element.Element("Location");
                    if (location is null)
                    {
                        location = new XElement("Location");
                        element.AddFirst(location);
                    }

                    SetOrCreate(location, "X", FormatDouble(spawn.X));
                    SetOrCreate(location, "Y", FormatDouble(spawn.Y));
                    SetOrCreate(location, "Z", FormatDouble(spawn.Z));

                    container.Add(element);
                }
            }
        }

        private static XElement CreateNewSpawnElement()
        {
            var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

            return new XElement("ConditionalLocation",
                new XAttribute(xsi + "type", "GangConditionalLocation"),
                new XElement("Location",
                    new XElement("X", "0"),
                    new XElement("Y", "0"),
                    new XElement("Z", "0")),
                new XElement("Heading", "0"),
                new XElement("Percentage", "35"),
                new XElement("TaskRequirements", "None"),
                new XElement("MinHourSpawn", "0"),
                new XElement("MaxHourSpawn", "24"),
                new XElement("MinWantedLevelSpawn", "0"),
                new XElement("MaxWantedLevelSpawn", "3"),
                new XElement("LongGunAlwaysEquipped", "false"));
        }

        private static void SetOrCreate(XElement parent, string name, string value)
        {
            var el = parent.Element(name);
            if (el is null)
            {
                parent.Add(new XElement(name, value));
                return;
            }

            el.Value = value;
        }

        private static string FormatDouble(double value)
        {
            return value.ToString("0.#####", CultureInfo.InvariantCulture);
        }
    }
}
