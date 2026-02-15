using LSR.XmlHelper.Core.Services.Resolvers;
using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class LsrConfigFileResolverService
    {
        public string? ResolveGangFile(string rootFolderPath, string gangId)
        {
            return ResolveGangFile(rootFolderPath, gangId, "Default");
        }

        public string? ResolveGangFile(string rootFolderPath, string gangId, string? configName)
        {
            return ResolveById(rootFolderPath, LsrXmlRecipeCatalog.Gangs, "Gang", "ID", gangId, configName);
        }

        public string? ResolveDispatchablePeopleFile(string rootFolderPath, string dispatchablePeopleGroupId)
        {
            return ResolveDispatchablePeopleFile(rootFolderPath, dispatchablePeopleGroupId, "Default");
        }

        public string? ResolveDispatchablePeopleFile(string rootFolderPath, string dispatchablePeopleGroupId, string? configName)
        {
            return ResolveById(rootFolderPath, LsrXmlRecipeCatalog.DispatchablePeople, "DispatchablePersonGroup", "DispatchablePersonGroupID", dispatchablePeopleGroupId, configName);
        }

        public string? ResolveDispatchableVehiclesFile(string rootFolderPath, string dispatchableVehiclesGroupId)
        {
            return ResolveDispatchableVehiclesFile(rootFolderPath, dispatchableVehiclesGroupId, "Default");
        }

        public string? ResolveDispatchableVehiclesFile(string rootFolderPath, string dispatchableVehiclesGroupId, string? configName)
        {
            return ResolveById(rootFolderPath, LsrXmlRecipeCatalog.DispatchableVehicles, "DispatchableVehicleGroup", "DispatchableVehicleGroupID", dispatchableVehiclesGroupId, configName);
        }

        public string? ResolveGangTerritoriesFile(string rootFolderPath, string gangId)
        {
            return ResolveGangTerritoriesFile(rootFolderPath, gangId, "Default");
        }

        public string? ResolveGangTerritoriesFile(string rootFolderPath, string gangId, string? configName)
        {
            return ResolveById(rootFolderPath, LsrXmlRecipeCatalog.GangTerritories, "GangTerritory", "GangID", gangId, configName);
        }

        public string? ResolveLocationsFileForGangDen(string rootFolderPath, string gangId, string denName)
        {
            return ResolveLocationsFileForGangDen(rootFolderPath, gangId, denName, "Default");
        }

        public string? ResolveLocationsFileForGangDen(string rootFolderPath, string gangId, string denName, string? configName)
        {
            gangId = (gangId ?? "").Trim();
            denName = (denName ?? "").Trim();

            if (string.IsNullOrWhiteSpace(gangId) || string.IsNullOrWhiteSpace(denName))
                return null;

            var basePath = ResolveLsrBaseFile(
                rootFolderPath,
                LsrXmlRecipeCatalog.Locations,
                configName);

            return ResolveByPredicateWithBasePath(
                rootFolderPath,
                LsrXmlRecipeCatalog.Locations,
                basePath,
                doc =>

                {
                    return doc
                        .Descendants("GangDen")
                        .Any(x =>
                            string.Equals((((string?)x.Element("AssignedAssociationID") ?? "").Trim()), gangId, StringComparison.OrdinalIgnoreCase)
                            && string.Equals((((string?)x.Element("Name") ?? "").Trim()), denName, StringComparison.OrdinalIgnoreCase));
                });
        }

        public string? ResolveLocationsFileForGangDens(string rootFolderPath, string gangId)
        {
            return ResolveLocationsFileForGangDens(rootFolderPath, gangId, "Default");
        }

        public string? ResolveLocationsFileForGangDens(string rootFolderPath, string gangId, string? configName)
        {
            gangId = (gangId ?? "").Trim();

            if (string.IsNullOrWhiteSpace(gangId))
                return null;

            var basePath = ResolveLsrBaseFile(
                rootFolderPath,
                LsrXmlRecipeCatalog.Locations,
                configName);

            return ResolveByPredicateWithBasePath(
                rootFolderPath,
                LsrXmlRecipeCatalog.Locations,
                basePath,
                doc =>

                {
                    return doc
                        .Descendants("GangDen")
                        .Any(x => string.Equals((((string?)x.Element("AssignedAssociationID") ?? "").Trim()), gangId, StringComparison.OrdinalIgnoreCase));
                });
        }

        public string? ResolveShopMenusFile(string rootFolderPath, params string[] idsToFind)
        {
            return ResolveShopMenusFile(rootFolderPath, "Default", idsToFind);
        }

        public string? ResolveShopMenusFile(string rootFolderPath, string? configName, params string[] idsToFind)
        {
            var ids = (idsToFind ?? Array.Empty<string>())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (ids.Length == 0)
                return null;

            var basePath = ResolveLsrBaseFile(
                rootFolderPath,
                LsrXmlRecipeCatalog.ShopMenus,
                configName);

            return ResolveByPredicateWithBasePath(
                rootFolderPath,
                LsrXmlRecipeCatalog.ShopMenus,
                basePath,
                doc =>
                {
                    foreach (var id in ids)
                    {
                        var hit =
                            doc.Descendants()
                                .Any(x =>
                                    string.Equals((((string?)x.Element("ID") ?? "").Trim()), id, StringComparison.OrdinalIgnoreCase)
                                    || string.Equals((((string?)x.Element("ShopMenuID") ?? "").Trim()), id, StringComparison.OrdinalIgnoreCase));

                        if (hit)
                            return true;
                    }

                    return false;
                });
        }

        public string? ResolveZonesFile(string rootFolderPath)
        {
            return ResolveZonesFile(rootFolderPath, "Default");
        }

        public string? ResolveZonesFile(string rootFolderPath, string? configName)
        {
            return ResolveLsrBaseFile(
                 rootFolderPath,
                 LsrXmlRecipeCatalog.Zones,
                 configName);
        }

        private string? ResolveById(string rootFolderPath, LsrXmlFileRecipe recipe, string recordElementName, string idElementName, string idValue, string? configName)
        {
            idValue = (idValue ?? "").Trim();
            if (string.IsNullOrWhiteSpace(rootFolderPath) || string.IsNullOrWhiteSpace(idValue))
                return null;

            var basePath = ResolveLsrBaseFile(
                rootFolderPath,
                recipe,
                configName);

            return ResolveByPredicateWithBasePath(
                rootFolderPath,
                recipe,
                basePath,
                doc =>
                {
                    return doc
                        .Descendants(recordElementName)
                        .Any(x => string.Equals((((string?)x.Element(idElementName) ?? "").Trim()), idValue, StringComparison.OrdinalIgnoreCase));
                });
        }

        private string? ResolveLsrBaseFile(string rootFolderPath, LsrXmlFileRecipe recipe, string? configName)
        {
            var baseSelector = new LsrBaseFileSelector();
            return baseSelector.ResolveBaseFile(rootFolderPath, recipe.BaseFileName, recipe.LsrWildcardWhenConfigEmpty, recipe.FileNameForConfig, configName);
        }

        private string? ResolveByPredicateWithBasePath(string rootFolderPath, LsrXmlFileRecipe recipe, string? basePath, Func<XDocument, bool> match)
        {
            if (string.IsNullOrWhiteSpace(rootFolderPath) || !Directory.Exists(rootFolderPath))
                return null;

            if (match is null)
                return null;

            var winnerResolver = new LsrWinnerFileResolver();

            if (!recipe.AllowAdditives)
            {
                if (string.IsNullOrWhiteSpace(basePath) || !File.Exists(basePath))
                    return null;

                return winnerResolver.ResolveWinnerFile(basePath, Array.Empty<string>(), match);
            }

            var additiveEnumerator = new LsrAdditiveFileEnumerator();

            var additivePaths = additiveEnumerator.GetAdditivePaths(rootFolderPath, recipe.AdditivePattern);

            return winnerResolver.ResolveWinnerFile(basePath, additivePaths, match);
        }
    }
}
