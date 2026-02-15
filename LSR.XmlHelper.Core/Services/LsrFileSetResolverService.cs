using LSR.XmlHelper.Core.Services.Resolvers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace LSR.XmlHelper.Core.Services
{
    public sealed class LsrFileSetResolverService
    {
        public LsrResolvedFileSet ResolveGangs(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.Gangs, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveGangTerritories(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.GangTerritories, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveLocations(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.Locations, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveShopMenus(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.ShopMenus, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveZones(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.Zones, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveDispatchablePeople(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.DispatchablePeople, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveDispatchableVehicles(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.DispatchableVehicles, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveIntoxicants(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.Itoxicants, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveModItems(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.ModItems, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolvePhysicalItems(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.PhysicalItems, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveIssuableWeapons(string rootFolderPath, string? configName)
        {
            return Resolve(LsrXmlRecipeCatalog.IssuableWeapons, rootFolderPath, configName);
        }

        public LsrResolvedFileSet ResolveGangRelationships(string rootFolderPath, string? configName)
        {
            return Resolve(rootFolderPath, "GangRelationships.xml", "GangRelationships_*.xml", cfg => $"GangRelationships_{cfg}.xml", "GangRelationships+_*.xml", configName);
        }
        private LsrResolvedFileSet Resolve(LsrXmlFileRecipe recipe, string rootFolderPath, string? configName)
        {
            var baseSelector = new LsrBaseFileSelector();

            var basePath = baseSelector.ResolveBaseFile(rootFolderPath, recipe.BaseFileName, recipe.LsrWildcardWhenConfigEmpty, recipe.FileNameForConfig, configName);

            if (!recipe.AllowAdditives)
            {
                if (string.IsNullOrWhiteSpace(basePath))
                    return LsrResolvedFileSet.Empty;

                return new LsrResolvedFileSet(basePath, Array.Empty<string>());
            }

            var additiveEnumerator = new LsrAdditiveFileEnumerator();

            var additivePaths = additiveEnumerator.GetAdditivePaths(rootFolderPath, recipe.AdditivePattern);

            if (string.IsNullOrWhiteSpace(basePath) && (additivePaths is null || additivePaths.Length == 0))
                return LsrResolvedFileSet.Empty;

            return new LsrResolvedFileSet(basePath, additivePaths);
        }

        private LsrResolvedFileSet Resolve(string rootFolderPath, string baseFileName, string wildcardWhenConfigEmpty, Func<string, string> fileNameForConfig, string additivePattern, string? configName)
        {
            var baseSelector = new LsrBaseFileSelector();
            var additiveEnumerator = new LsrAdditiveFileEnumerator();

            var basePath = baseSelector.ResolveBaseFile(rootFolderPath, baseFileName, wildcardWhenConfigEmpty, fileNameForConfig, configName);
            var additivePaths = additiveEnumerator.GetAdditivePaths(rootFolderPath, additivePattern);

            if (string.IsNullOrWhiteSpace(basePath) && (additivePaths is null || additivePaths.Length == 0))
                return LsrResolvedFileSet.Empty;

            return new LsrResolvedFileSet(basePath, additivePaths);
        }

        private LsrResolvedFileSet ResolveWithoutAdditives(string rootFolderPath, string baseFileName, string wildcardWhenConfigEmpty, Func<string, string> fileNameForConfig, string? configName)
        {
            var baseSelector = new LsrBaseFileSelector();

            var basePath = baseSelector.ResolveBaseFile(rootFolderPath, baseFileName, wildcardWhenConfigEmpty, fileNameForConfig, configName);

            if (string.IsNullOrWhiteSpace(basePath))
                return LsrResolvedFileSet.Empty;

            return new LsrResolvedFileSet(basePath, Array.Empty<string>());
        }

        private string NormalizeConfigName(string? configName)
        {
            var normalized = (configName ?? "").Trim();
            return string.IsNullOrWhiteSpace(normalized) ? "Default" : normalized;
        }
    }

    public sealed class LsrResolvedFileSet
    {
        public static LsrResolvedFileSet Empty { get; } = new LsrResolvedFileSet(null, Array.Empty<string>());

        public LsrResolvedFileSet(string? basePath, IReadOnlyList<string> additivePaths)
        {
            BasePath = string.IsNullOrWhiteSpace(basePath) ? null : basePath;
            AdditivePaths = additivePaths ?? Array.Empty<string>();
        }

        public string? BasePath { get; }
        public IReadOnlyList<string> AdditivePaths { get; }
        public IReadOnlyList<string> EnumerateReadOrder()
        {
            var list = new List<string>();

            if (!string.IsNullOrWhiteSpace(BasePath))
                list.Add(BasePath);

            if (AdditivePaths != null && AdditivePaths.Count > 0)
                list.AddRange(AdditivePaths);

            return list;
        }
    }
}
