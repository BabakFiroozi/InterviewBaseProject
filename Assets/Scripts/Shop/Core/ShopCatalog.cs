using System;
using System.Collections.Generic;
using Shop.Model;
using UnityEngine;

namespace Shop.Core
{
    /// <summary>A visible section of the shop together with its (enabled) items, in display order.</summary>
    public sealed class CatalogSection
    {
        public ShopSectionConfig Config { get; }
        public List<ShopItemConfig> Items { get; } = new List<ShopItemConfig>();

        public string Id => Config.id;
        public string Title => string.IsNullOrEmpty(Config.title) ? Config.id : Config.title;

        public CatalogSection(ShopSectionConfig config)
        {
            Config = config;
        }
    }

    /// <summary>
    /// Read-only, validated view of the JSON shop config. Invalid entries (missing/duplicate ids,
    /// unknown sections) are skipped with a warning rather than breaking the whole shop.
    /// </summary>
    public sealed class ShopCatalog
    {
        private readonly Dictionary<string, ShopItemConfig> _itemsById = new Dictionary<string, ShopItemConfig>();
        private readonly List<ShopItemConfig> _items = new List<ShopItemConfig>();
        private readonly List<CatalogSection> _sections = new List<CatalogSection>();

        public EconomyConfig Economy { get; }
        public IReadOnlyList<CatalogSection> Sections => _sections;
        /// <summary>All valid items (also disabled ones), in config order.</summary>
        public IReadOnlyList<ShopItemConfig> Items => _items;

        public ShopCatalog(ShopConfig config)
        {
            config = config ?? new ShopConfig();
            Economy = config.economy ?? new EconomyConfig();

            var sectionsById = new Dictionary<string, CatalogSection>();
            if (config.sections != null)
            {
                foreach (var section in config.sections)
                {
                    if (section == null || string.IsNullOrEmpty(section.id))
                    {
                        Debug.LogWarning("[Shop] Skipping a section without id.");
                        continue;
                    }

                    if (sectionsById.ContainsKey(section.id))
                    {
                        Debug.LogWarning($"[Shop] Duplicate section id '{section.id}' skipped.");
                        continue;
                    }

                    var catalogSection = new CatalogSection(section);
                    sectionsById.Add(section.id, catalogSection);
                    if (section.visible)
                        _sections.Add(catalogSection);
                }
            }

            if (config.items == null)
                return;

            foreach (var item in config.items)
            {
                if (item == null || string.IsNullOrEmpty(item.id) || string.IsNullOrEmpty(item.kind))
                {
                    Debug.LogWarning("[Shop] Skipping an item without id or kind.");
                    continue;
                }

                if (_itemsById.ContainsKey(item.id))
                {
                    Debug.LogWarning($"[Shop] Duplicate item id '{item.id}' skipped.");
                    continue;
                }

                if (item.price < 0)
                    item.price = 0;

                _itemsById.Add(item.id, item);
                _items.Add(item);

                if (!item.enabled)
                    continue;

                if (!string.IsNullOrEmpty(item.section) && sectionsById.TryGetValue(item.section, out var owner))
                    owner.Items.Add(item);
                else
                    Debug.LogWarning($"[Shop] Item '{item.id}' references unknown section '{item.section}' and is not listed.");
            }
        }

        public static ShopCatalog FromJson(string json)
        {
            ShopConfig config = null;
            try
            {
                config = JsonUtility.FromJson<ShopConfig>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Shop] Could not parse shop config: {e.Message}");
            }

            return new ShopCatalog(config);
        }

        public bool TryGetItem(string id, out ShopItemConfig item)
        {
            item = null;
            return !string.IsNullOrEmpty(id) && _itemsById.TryGetValue(id, out item);
        }

        /// <summary>The item of the given kind that is owned from the start (what is equipped before the player chooses).</summary>
        public ShopItemConfig GetDefaultItem(string kind)
        {
            foreach (var item in _items)
            {
                if (item.kind == kind && item.ownedByDefault)
                    return item;
            }

            return null;
        }
    }
}
