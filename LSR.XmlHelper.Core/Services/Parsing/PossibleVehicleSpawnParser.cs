using System.Collections.Generic;
using System.Globalization;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;

namespace LSR.XmlHelper.Core.Services.Parsing
{
    public static class PossibleVehicleSpawnParser
    {
        public static List<PossibleVehicleSpawnModel> ParseGangDen(XElement gangDenElement)
        {
            var result = new List<PossibleVehicleSpawnModel>();

            var denName = gangDenElement.Element("Name")?.Value ?? "";

            var container = gangDenElement.Element("PossibleVehicleSpawns");
            if (container is null)
                return result;

            foreach (var spawn in container.Elements("ConditionalLocation"))
            {
                var location = spawn.Element("Location");

                var model = new PossibleVehicleSpawnModel
                {
                    DenName = denName,
                    X = ParseDouble(location?.Element("X")?.Value),
                    Y = ParseDouble(location?.Element("Y")?.Value),
                    Z = ParseDouble(location?.Element("Z")?.Value),
                    Heading = ParseDouble(spawn.Element("Heading")?.Value),
                    Percentage = ParseInt(spawn.Element("Percentage")?.Value),
                    TaskRequirements = spawn.Element("TaskRequirements")?.Value ?? "",
                    MinHourSpawn = ParseInt(spawn.Element("MinHourSpawn")?.Value),
                    MaxHourSpawn = ParseInt(spawn.Element("MaxHourSpawn")?.Value),
                    MinWantedLevelSpawn = ParseInt(spawn.Element("MinWantedLevelSpawn")?.Value),
                    MaxWantedLevelSpawn = ParseInt(spawn.Element("MaxWantedLevelSpawn")?.Value),
                    RequiredVehicleGroup = spawn.Element("RequiredVehicleGroup")?.Value ?? "",
                    ForceVehicleGroup = ParseBool(spawn.Element("ForceVehicleGroup")?.Value),
                    AllowAirVehicle = ParseBool(spawn.Element("AllowAirVehicle")?.Value),
                    AllowBoat = ParseBool(spawn.Element("AllowBoat")?.Value),
                    SourceElement = new XElement(spawn)
                };

                result.Add(model);
            }

            return result;
        }

        private static int ParseInt(string? value)
        {
            return int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var i) ? i : 0;
        }

        private static double ParseDouble(string? value)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) ? d : 0;
        }

        private static bool ParseBool(string? value)
        {
            return bool.TryParse(value, out var b) && b;
        }
    }
}
