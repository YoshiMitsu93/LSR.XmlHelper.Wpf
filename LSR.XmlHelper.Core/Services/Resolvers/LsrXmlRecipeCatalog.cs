namespace LSR.XmlHelper.Core.Services.Resolvers
{
    public static class LsrXmlRecipeCatalog
    {
        public static LsrXmlFileRecipe Gangs { get; } =
            new LsrXmlFileRecipe("Gangs.xml", "Gangs_*.xml", cfg => $"Gangs_{cfg}.xml", true, "Gangs+_*.xml");

        public static LsrXmlFileRecipe GangTerritories { get; } =
            new LsrXmlFileRecipe("GangTerritories.xml", "GangTerritories_*.xml", cfg => $"GangTerritories_{cfg}.xml", true, "GangTerritories+_*.xml");

        public static LsrXmlFileRecipe Locations { get; } =
            new LsrXmlFileRecipe("Locations.xml", "Locations_*.xml", cfg => $"Locations_{cfg}.xml", true, "Locations+_*.xml");

        public static LsrXmlFileRecipe ShopMenus { get; } =
            new LsrXmlFileRecipe("ShopMenus.xml", "ShopMenus_*.xml", cfg => $"ShopMenus_{cfg}.xml", true, "ShopMenus+_*.xml");

        public static LsrXmlFileRecipe Zones { get; } =
            new LsrXmlFileRecipe("Zones.xml", "Zones*.xml", cfg => $"Zones_{cfg}.xml", false, "");

        public static LsrXmlFileRecipe DispatchablePeople { get; } =
            new LsrXmlFileRecipe("DispatchablePeople.xml", "DispatchablePeople_*.xml", cfg => $"DispatchablePeople_{cfg}.xml", true, "DispatchablePeople+_*.xml");

        public static LsrXmlFileRecipe DispatchableVehicles { get; } =
            new LsrXmlFileRecipe("DispatchableVehicles.xml", "DispatchableVehicles_*.xml", cfg => $"DispatchableVehicles_{cfg}.xml", true, "DispatchableVehicles+_*.xml");

        public static LsrXmlFileRecipe Itoxicants { get; } =
            new LsrXmlFileRecipe("Itoxicants.xml", "Itoxicants_*.xml", cfg => $"Itoxicants_{cfg}.xml", true, "Itoxicants+_*.xml");

        public static LsrXmlFileRecipe ModItems { get; } =
            new LsrXmlFileRecipe("ModItems.xml", "ModItems_*.xml", cfg => $"ModItems_{cfg}.xml", true, "ModItems+_*.xml");

        public static LsrXmlFileRecipe PhysicalItems { get; } =
            new LsrXmlFileRecipe("PhysicalItems.xml", "PhysicalItems_*.xml", cfg => $"PhysicalItems_{cfg}.xml", true, "PhysicalItems+_*.xml");

        public static LsrXmlFileRecipe IssuableWeapons { get; } =
            new LsrXmlFileRecipe("IssuableWeapons.xml", "IssuableWeapons_*.xml", cfg => $"IssuableWeapons_{cfg}.xml", true, "IssuableWeapons+_*.xml");
    }
}
