using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;
using LSR.XmlHelper.Wpf.ViewModels.Builders;

namespace LSR.XmlHelper.Wpf.Services.Editing
{
    public sealed class EditModePostSaveValidationService
    {
        public IReadOnlyList<string> Validate(
            string rootFolderPath,
            string gangId,
            IReadOnlyCollection<string> editedFilePaths,
            bool validatePeopleGroupId,
            string expectedPeopleGroupId,
            IReadOnlyCollection<DispatchablePersonEntryViewModel> expectedPeopleEntries,
            bool validateVehicleGroupId,
            string expectedVehicleGroupId,
            bool validateTerritories,
            IReadOnlyCollection<string> expectedZoneInternalNames,
            bool validateDen,
            string expectedDenName,
            IReadOnlyCollection<PossiblePedSpawnViewModel> expectedPedSpawns,
            IReadOnlyCollection<PossibleVehicleSpawnViewModel> expectedVehicleSpawns,
            bool validateDenInventory,
            string expectedDenMenuId,
            IReadOnlyCollection<DenInventoryMenuItemViewModel> expectedDenInventoryItems,
            bool validateDealerMenus,
            string expectedDealerGroupId,
            IReadOnlyList<(int MenuIndex, LSR.XmlHelper.Core.Models.DenInventoryMenuItem[] Items)> expectedDealerMenuEdits,
            bool validateDispatchableVehicles,
            string expectedDispatchableVehiclesGroupId,
            IReadOnlyCollection<CustomDispatchableVehicleModelViewModel> expectedCustomVehicleModels)
        {
            var issues = new List<string>();

            rootFolderPath = (rootFolderPath ?? "").Trim();
            gangId = (gangId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(rootFolderPath) || string.IsNullOrWhiteSpace(gangId))
                return new[] { "Validation skipped: missing root folder path or gang id." };

            editedFilePaths ??= Array.Empty<string>();

            var edited = new HashSet<string>(
                editedFilePaths.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()),
                StringComparer.OrdinalIgnoreCase);

            var gangsPath = Path.Combine(rootFolderPath, "Gangs.xml");
            var peoplePath = Path.Combine(rootFolderPath, "DispatchablePeople.xml");
            var territoriesPath = Path.Combine(rootFolderPath, "GangTerritories.xml");
            var locationsPath = Path.Combine(rootFolderPath, "Locations.xml");
            var shopMenusPath = Path.Combine(rootFolderPath, "ShopMenus.xml");
            var vehiclesPath = Path.Combine(rootFolderPath, "DispatchableVehicles.xml");

            if (edited.Contains(gangsPath))
                ValidateGangs(gangsPath, gangId, validatePeopleGroupId, expectedPeopleGroupId, validateVehicleGroupId, expectedVehicleGroupId, issues);

            if (edited.Contains(peoplePath))
                ValidatePeople(peoplePath, expectedPeopleGroupId, expectedPeopleEntries, issues);

            if (edited.Contains(territoriesPath) && validateTerritories)
                ValidateTerritories(territoriesPath, gangId, expectedZoneInternalNames, issues);

            if (edited.Contains(locationsPath) && validateDen)
                if (edited.Contains(shopMenusPath) && (validateDenInventory || validateDealerMenus))
                {
                    ValidateShopMenus(
                        shopMenusPath,
                        validateDenInventory,
                        expectedDenMenuId,
                        expectedDenInventoryItems,
                        validateDealerMenus,
                        expectedDealerGroupId,
                        expectedDealerMenuEdits,
                        issues);
                }

            if (edited.Contains(vehiclesPath) && validateDispatchableVehicles)
            {
                ValidateDispatchableVehicles(
                    vehiclesPath,
                    expectedDispatchableVehiclesGroupId,
                    expectedCustomVehicleModels,
                    issues);
            }

            ValidateDen(locationsPath, gangId, expectedDenName, expectedPedSpawns, expectedVehicleSpawns, issues);

            return issues;
        }

        private static void ValidateGangs(
            string gangsPath,
            string gangId,
            bool validatePeopleGroupId,
            string expectedPeopleGroupId,
            bool validateVehicleGroupId,
            string expectedVehicleGroupId,
            List<string> issues)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load(gangsPath, LoadOptions.None);
            }
            catch (Exception ex)
            {
                issues.Add("Gangs.xml: failed to reload for validation: " + ex.Message);
                return;
            }

            var gang = doc.Descendants("Gang")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), gangId, StringComparison.OrdinalIgnoreCase));

            if (gang is null)
            {
                issues.Add("Gangs.xml: could not find Gang ID '" + gangId + "'.");
                return;
            }

            if (validatePeopleGroupId)
            {
                var actual = ReadGangFieldValue(gang, "PeopleGroupID", "PersonnelID");
                var expected = (expectedPeopleGroupId ?? "").Trim();
                if (!string.Equals(actual, expected, StringComparison.Ordinal))
                    issues.Add("Gangs.xml: PeopleGroupID/PersonnelID mismatch. Expected '" + expected + "' but found '" + actual + "'.");
            }

            if (validateVehicleGroupId)
            {
                var actual = ReadGangFieldValue(gang, "VehicleGroupID", "VehiclesID");
                var expected = (expectedVehicleGroupId ?? "").Trim();
                if (!string.Equals(actual, expected, StringComparison.Ordinal))
                    issues.Add("Gangs.xml: VehicleGroupID/VehiclesID mismatch. Expected '" + expected + "' but found '" + actual + "'.");
            }
        }

        private static void ValidateTerritories(
            string territoriesPath,
            string gangId,
            IReadOnlyCollection<string> expectedZoneInternalNames,
            List<string> issues)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load(territoriesPath, LoadOptions.None);
            }
            catch (Exception ex)
            {
                issues.Add("GangTerritories.xml: failed to reload for validation: " + ex.Message);
                return;
            }

            var actualZones = doc.Descendants("GangTerritory")
                .Where(t => string.Equals(((string?)t.Element("GangID") ?? "").Trim(), gangId, StringComparison.OrdinalIgnoreCase))
                .Select(t => ((string?)t.Element("ZoneInternalGameName") ?? "").Trim())
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(z => z, StringComparer.OrdinalIgnoreCase)
                .ToList();

            expectedZoneInternalNames ??= Array.Empty<string>();

            var expectedZones = expectedZoneInternalNames
                .Select(z => (z ?? "").Trim())
                .Where(z => !string.IsNullOrWhiteSpace(z))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(z => z, StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (!actualZones.SequenceEqual(expectedZones, StringComparer.OrdinalIgnoreCase))
            {
                issues.Add(
                    "GangTerritories.xml: zone list mismatch.\r\n" +
                    "Expected: " + string.Join(", ", expectedZones) + "\r\n" +
                    "Actual: " + string.Join(", ", actualZones));
            }
        }

        private static void ValidateDen(
            string locationsPath,
            string gangId,
            string expectedDenName,
            IReadOnlyCollection<PossiblePedSpawnViewModel> expectedPedSpawns,
            IReadOnlyCollection<PossibleVehicleSpawnViewModel> expectedVehicleSpawns,
            List<string> issues)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load(locationsPath, LoadOptions.None);
            }
            catch (Exception ex)
            {
                issues.Add("Locations.xml: failed to reload for validation: " + ex.Message);
                return;
            }

            var den = doc.Descendants("GangDen")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("AssignedAssociationID") ?? "").Trim(), gangId, StringComparison.OrdinalIgnoreCase));

            if (den is null)
            {
                issues.Add("Locations.xml: could not find GangDen for AssignedAssociationID '" + gangId + "'.");
                return;
            }

            var expectedName = (expectedDenName ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(expectedName))
            {
                var actualName = ((string?)den.Element("Name") ?? "").Trim();
                if (!string.Equals(actualName, expectedName, StringComparison.Ordinal))
                    issues.Add("Locations.xml: den Name mismatch. Expected '" + expectedName + "' but found '" + actualName + "'.");
            }

            ValidateSpawnPercentages(den, "PossiblePedSpawns", expectedPedSpawns.Select(ToKeyedSpawn).ToList(), issues, "ped");
            ValidateSpawnPercentages(den, "PossibleVehicleSpawns", expectedVehicleSpawns.Select(ToKeyedSpawn).ToList(), issues, "vehicle");
        }

        private static void ValidatePeople(
            string peoplePath,
            string groupId,
            IReadOnlyCollection<DispatchablePersonEntryViewModel> expectedEntries,
            List<string> issues)
        {
            groupId = (groupId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(groupId))
                return;

            XDocument doc;
            try
            {
                doc = XDocument.Load(peoplePath, LoadOptions.None);
            }
            catch (Exception ex)
            {
                issues.Add("DispatchablePeople.xml: failed to reload for validation: " + ex.Message);
                return;
            }

            var group = doc
                .Descendants("DispatchablePersonGroup")
                .FirstOrDefault(g =>
                {
                    var id = ((string?)g.Element("ID") ?? (string?)g.Element("DispatchablePersonGroupID") ?? "").Trim();
                    return string.Equals(id, groupId, StringComparison.OrdinalIgnoreCase);
                });

            if (group is null)
            {
                issues.Add("DispatchablePeople.xml: could not find person group '" + groupId + "'.");
                return;
            }

            var people = group.Descendants("DispatchablePerson").ToList();
            if (people.Count == 0)
            {
                issues.Add("DispatchablePeople.xml: person group '" + groupId + "' has no DispatchablePerson entries.");
                return;
            }

            expectedEntries ??= Array.Empty<DispatchablePersonEntryViewModel>();

            foreach (var entry in expectedEntries)
            {
                if (entry is null)
                    continue;

                if (entry.SourceIndex < 0 || entry.SourceIndex >= people.Count)
                    continue;

                var person = people[entry.SourceIndex];

                foreach (var field in entry.Fields)
                {
                    var name = (field?.Name ?? "").Trim();
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (string.Equals(name, "ID", StringComparison.OrdinalIgnoreCase))
                        continue;

                    if (string.Equals(name, "DispatchablePersonGroupID", StringComparison.OrdinalIgnoreCase))
                        continue;

                    var expected = (field?.Value ?? "");
                    var actualElement = person.Element(name);

                    if (actualElement is null)
                    {
                        issues.Add("DispatchablePeople.xml: missing element '" + name + "' at person index " + entry.SourceIndex + ".");
                        continue;
                    }

                    var actual = (field?.IsXml ?? false)
                    ? actualElement.ToString(SaveOptions.DisableFormatting)
                    : (actualElement.Value ?? "");
                    if (!string.Equals(actual, expected, StringComparison.Ordinal))
                        issues.Add("DispatchablePeople.xml: '" + name + "' mismatch at person index " + entry.SourceIndex + ". Expected '" + expected + "' but found '" + actual + "'.");
                }
            }
        }

        private static void ValidateSpawnPercentages(
            XElement den,
            string containerElementName,
            IReadOnlyList<KeyedSpawn> expected,
            List<string> issues,
            string spawnType)
        {
            var container = den.Element(containerElementName);
            if (container is null)
                return;

            var actual = container.Elements("ConditionalLocation")
                .Select(ReadKeyedSpawn)
                .Where(x => x.IsValid)
                .ToList();

            foreach (var exp in expected.Where(x => x.IsValid))
            {
                var match = actual.FirstOrDefault(a => a.Matches(exp));
                if (!match.IsValid)
                {
                    issues.Add("Locations.xml: missing " + spawnType + " spawn for X=" + exp.X + ", Y=" + exp.Y + ", Z=" + exp.Z + ", Heading=" + exp.Heading + ".");
                    continue;
                }

                if (match.Percentage != exp.Percentage)
                    issues.Add("Locations.xml: " + spawnType + " spawn Percentage mismatch at X=" + exp.X + ", Y=" + exp.Y + ", Z=" + exp.Z + ", Heading=" + exp.Heading + ". Expected " + exp.Percentage + " but found " + match.Percentage + ".");
            }
        }

        private static KeyedSpawn ToKeyedSpawn(PossiblePedSpawnViewModel vm)
        {
            if (vm is null)
                return default;

            return new KeyedSpawn(vm.X, vm.Y, vm.Z, vm.Heading, vm.Percentage, true);
        }

        private static KeyedSpawn ToKeyedSpawn(PossibleVehicleSpawnViewModel vm)
        {
            if (vm is null)
                return default;

            return new KeyedSpawn(vm.X, vm.Y, vm.Z, vm.Heading, vm.Percentage, true);
        }

        private static KeyedSpawn ReadKeyedSpawn(XElement conditionalLocation)
        {
            if (conditionalLocation is null)
                return default;

            var loc = conditionalLocation.Element("Location");
            var x = ReadDouble(loc?.Element("X")?.Value);
            var y = ReadDouble(loc?.Element("Y")?.Value);
            var z = ReadDouble(loc?.Element("Z")?.Value);
            var heading = ReadDouble(conditionalLocation.Element("Heading")?.Value);
            var percentage = ReadInt(conditionalLocation.Element("Percentage")?.Value);

            if (!x.HasValue || !y.HasValue || !z.HasValue || !heading.HasValue || !percentage.HasValue)
                return default;

            return new KeyedSpawn(x.Value, y.Value, z.Value, heading.Value, percentage.Value, true);
        }

        private static double? ReadDouble(string? raw)
        {
            raw = (raw ?? "").Trim();
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                return value;

            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.CurrentCulture, out value))
                return value;

            return null;
        }

        private static int? ReadInt(string? raw)
        {
            raw = (raw ?? "").Trim();
            if (string.IsNullOrWhiteSpace(raw))
                return null;

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value))
                return value;

            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.CurrentCulture, out value))
                return value;

            return null;
        }
        private static void ValidateShopMenus(
    string shopMenusPath,
    bool validateDenInventory,
    string expectedDenMenuId,
    IReadOnlyCollection<DenInventoryMenuItemViewModel> expectedDenInventoryItems,
    bool validateDealerMenus,
    string expectedDealerGroupId,
    IReadOnlyList<(int MenuIndex, LSR.XmlHelper.Core.Models.DenInventoryMenuItem[] Items)> expectedDealerMenuEdits,
    List<string> issues)
        {
            XDocument doc;
            try
            {
                doc = XDocument.Load(shopMenusPath, LoadOptions.None);
            }
            catch (Exception ex)
            {
                issues.Add("ShopMenus.xml: failed to reload for validation: " + ex.Message);
                return;
            }

            if (validateDenInventory)
            {
                var menuId = (expectedDenMenuId ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(menuId))
                {
                    var menu = doc.Descendants("ShopMenu")
                        .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), menuId, StringComparison.OrdinalIgnoreCase));

                    if (menu is null)
                    {
                        issues.Add("ShopMenus.xml: missing den inventory ShopMenu ID '" + menuId + "'.");
                    }
                    else
                    {
                        expectedDenInventoryItems ??= Array.Empty<DenInventoryMenuItemViewModel>();

                        var remainingMenuItems = menu.Descendants("MenuItem").ToList();

                        foreach (var item in expectedDenInventoryItems)
                        {
                            if (item is null)
                                continue;

                            var modName = (item.ModItemName ?? "").Trim();
                            if (string.IsNullOrWhiteSpace(modName))
                                continue;

                            var candidates = remainingMenuItems
                                .Where(mi => string.Equals(((string?)mi.Element("ModItemName") ?? "").Trim(), modName, StringComparison.OrdinalIgnoreCase))
                                .ToList();

                            if (candidates.Count == 0)
                            {
                                issues.Add("ShopMenus.xml: den inventory missing item '" + modName + "' in ShopMenu '" + menuId + "'.");
                                continue;
                            }

                            XElement? chosen = null;

                            foreach (var c in candidates)
                            {
                                var raw = ((string?)c.Element("PurchasePrice") ?? "").Trim();
                                if (int.TryParse(raw, out var price) && price == item.PurchasePrice)
                                {
                                    chosen = c;
                                    break;
                                }
                            }

                            chosen ??= candidates[0];
                            remainingMenuItems.Remove(chosen);

                            var priceRaw = ((string?)chosen.Element("PurchasePrice") ?? "").Trim();
                            if (!int.TryParse(priceRaw, out var actualPrice))
                            {
                                issues.Add("ShopMenus.xml: den inventory item '" + modName + "' has non-integer PurchasePrice '" + priceRaw + "'.");
                                continue;
                            }

                            if (actualPrice != item.PurchasePrice)
                                issues.Add("ShopMenus.xml: den inventory item '" + modName + "' PurchasePrice mismatch. Expected " + item.PurchasePrice + " but found " + actualPrice + ".");
                        }
                    }
                }
            }

            if (validateDealerMenus)
            {
                var groupId = (expectedDealerGroupId ?? "").Trim();
                if (!string.IsNullOrWhiteSpace(groupId))
                {
                    var group = doc.Descendants("ShopMenuGroup")
                        .FirstOrDefault(x => string.Equals(((string?)x.Element("ID") ?? "").Trim(), groupId, StringComparison.OrdinalIgnoreCase));

                    if (group is null)
                    {
                        issues.Add("ShopMenus.xml: missing ShopMenuGroup ID '" + groupId + "'.");
                        return;
                    }

                    var menus = group.Descendants("PercentageSelectShopMenu").ToList();
                    expectedDealerMenuEdits ??= Array.Empty<(int MenuIndex, LSR.XmlHelper.Core.Models.DenInventoryMenuItem[] Items)>();

                    foreach (var edit in expectedDealerMenuEdits)
                    {
                        if (edit.MenuIndex < 0 || edit.MenuIndex >= menus.Count)
                        {
                            issues.Add("ShopMenus.xml: dealer menu index " + edit.MenuIndex + " out of range for group '" + groupId + "'.");
                            continue;
                        }

                        var shopMenu = menus[edit.MenuIndex].Element("ShopMenu");
                        if (shopMenu is null)
                        {
                            issues.Add("ShopMenus.xml: dealer menu index " + edit.MenuIndex + " missing ShopMenu element in group '" + groupId + "'.");
                            continue;
                        }

                        var items = edit.Items ?? Array.Empty<LSR.XmlHelper.Core.Models.DenInventoryMenuItem>();

                        foreach (var item in items)
                        {
                            if (item is null)
                                continue;

                            var modName = (item.ModItemName ?? "").Trim();
                            if (string.IsNullOrWhiteSpace(modName))
                                continue;

                            var menuItem = shopMenu.Descendants("MenuItem")
                                .FirstOrDefault(mi => string.Equals(((string?)mi.Element("ModItemName") ?? "").Trim(), modName, StringComparison.OrdinalIgnoreCase));

                            if (menuItem is null)
                            {
                                issues.Add("ShopMenus.xml: dealer menu item '" + modName + "' missing in group '" + groupId + "', menu index " + edit.MenuIndex + ".");
                                continue;
                            }

                            var priceRaw = ((string?)menuItem.Element("PurchasePrice") ?? "").Trim();
                            if (!int.TryParse(priceRaw, out var actualPrice))
                            {
                                issues.Add("ShopMenus.xml: dealer menu item '" + modName + "' has non-integer PurchasePrice '" + priceRaw + "'.");
                                continue;
                            }

                            if (actualPrice != item.PurchasePrice)
                                issues.Add("ShopMenus.xml: dealer menu item '" + modName + "' PurchasePrice mismatch. Expected " + item.PurchasePrice + " but found " + actualPrice + ".");
                        }
                    }
                }
            }
        }

        private static void ValidateDispatchableVehicles(
            string vehiclesPath,
            string expectedVehicleGroupId,
            IReadOnlyCollection<CustomDispatchableVehicleModelViewModel> expectedCustomModels,
            List<string> issues)
        {
            var groupId = (expectedVehicleGroupId ?? "").Trim();
            if (string.IsNullOrWhiteSpace(groupId))
                return;

            XDocument doc;
            try
            {
                doc = XDocument.Load(vehiclesPath, LoadOptions.None);
            }
            catch (Exception ex)
            {
                issues.Add("DispatchableVehicles.xml: failed to reload for validation: " + ex.Message);
                return;
            }

            var group = doc.Descendants("DispatchableVehicleGroup")
                .FirstOrDefault(x => string.Equals(((string?)x.Element("DispatchableVehicleGroupID") ?? "").Trim(), groupId, StringComparison.OrdinalIgnoreCase));

            if (group is null)
            {
                issues.Add("DispatchableVehicles.xml: missing DispatchableVehicleGroupID '" + groupId + "'.");
                return;
            }

            var modelsInXml = new HashSet<string>(
                group.Descendants("DispatchableVehicle")
                    .Select(v => ((string?)v.Element("ModelName") ?? "").Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x)),
                StringComparer.OrdinalIgnoreCase);

            expectedCustomModels ??= Array.Empty<CustomDispatchableVehicleModelViewModel>();

            foreach (var vm in expectedCustomModels)
            {
                if (vm is null)
                    continue;

                var model = (vm.ModelName ?? "").Trim();
                if (string.IsNullOrWhiteSpace(model))
                    continue;

                if (!modelsInXml.Contains(model))
                    issues.Add("DispatchableVehicles.xml: expected model '" + model + "' was not found in group '" + groupId + "'.");
            }
        }
        private static string ReadGangFieldValue(XElement gang, string primaryElementName, string fallbackElementName)
        {
            var primary = ((string?)gang.Element(primaryElementName) ?? "").Trim();
            if (!string.IsNullOrWhiteSpace(primary))
                return primary;

            return ((string?)gang.Element(fallbackElementName) ?? "").Trim();
        }

        private readonly record struct KeyedSpawn(double X, double Y, double Z, double Heading, int Percentage, bool IsValid)
        {
            public bool Matches(KeyedSpawn other)
            {
                return NearlyEqual(X, other.X) &&
                       NearlyEqual(Y, other.Y) &&
                       NearlyEqual(Z, other.Z) &&
                       NearlyEqual(Heading, other.Heading);
            }

            private static bool NearlyEqual(double a, double b)
            {
                return Math.Abs(a - b) < 0.01;
            }
        }
    }
}
