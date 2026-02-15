using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using LSR.XmlHelper.Core.Models;

namespace LSR.XmlHelper.Core.Services.Validation
{
    public class WeaponModelValidationService
    {
        public List<string> RemoveInvalidWeaponItems(string rootFolderPath, List<DenInventoryMenuItem> items)
        {
            List<string> removed = new List<string>();
            if (items == null || items.Count == 0)
            {
                return removed;
            }

            string weaponsPath = Path.Combine(rootFolderPath, "Weapons.xml");
            if (!File.Exists(weaponsPath))
            {
                return removed;
            }

            HashSet<string> weaponModels = LoadWeaponModels(weaponsPath);
            if (weaponModels.Count == 0)
            {
                return removed;
            }

            Dictionary<string, string> weaponItemNameToModel = LoadWeaponItemModels(rootFolderPath);
            if (weaponItemNameToModel.Count == 0)
            {
                return removed;
            }

            for (int i = items.Count - 1; i >= 0; i--)
            {
                DenInventoryMenuItem item = items[i];
                if (item == null || string.IsNullOrWhiteSpace(item.ModItemName))
                {
                    continue;
                }

                if (!weaponItemNameToModel.TryGetValue(item.ModItemName, out string? modelName))
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(modelName) || !weaponModels.Contains(modelName))
                {
                    removed.Add(item.ModItemName);
                    items.RemoveAt(i);
                }
            }

            removed.Reverse();
            return removed;
        }

        private HashSet<string> LoadWeaponModels(string weaponsPath)
        {
            HashSet<string> models = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            XDocument doc = XDocument.Load(weaponsPath);
            foreach (XElement modelElement in doc.Descendants("ModelName"))
            {
                string value = modelElement.Value?.Trim() ?? "";
                if (!string.IsNullOrWhiteSpace(value))
                {
                    models.Add(value);
                }
            }

            return models;
        }

        private Dictionary<string, string> LoadWeaponItemModels(string rootFolderPath)
        {
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            List<string> modItemFiles = Directory.GetFiles(rootFolderPath, "ModItems*.xml", SearchOption.TopDirectoryOnly)
                .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                .ToList();

            foreach (string file in modItemFiles)
            {
                if (!File.Exists(file))
                {
                    continue;
                }

                XDocument doc = XDocument.Load(file);

                foreach (XElement weaponItem in doc.Descendants("WeaponItem"))
                {
                    string name = weaponItem.Element("Name")?.Value?.Trim() ?? "";
                    string model = weaponItem.Element("ModelName")?.Value?.Trim() ?? "";

                    if (string.IsNullOrWhiteSpace(name))
                    {
                        continue;
                    }

                    if (!map.ContainsKey(name))
                    {
                        map[name] = model;
                    }
                }
            }

            return map;
        }
    }
}
