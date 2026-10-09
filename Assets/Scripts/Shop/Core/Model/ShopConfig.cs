using System;
using System.Collections.Generic;
using UnityEngine;

namespace Shop.Model
{
    /// <summary>
    /// Root of Resources/Shop/ShopConfig.json. Everything the shop sells, what it costs and in which
    /// order sections/items appear is data. Field initialisers are the defaults used when a key is
    /// missing from the JSON.
    /// </summary>
    [Serializable]
    public class ShopConfig
    {
        public int version = 1;
        public EconomyConfig economy = new EconomyConfig();
        /// <summary>Display order of the sections in the shop.</summary>
        public List<ShopSectionConfig> sections = new List<ShopSectionConfig>();
        /// <summary>Display order of the items inside their section = order in this list.</summary>
        public List<ShopItemConfig> items = new List<ShopItemConfig>();
    }

    [Serializable]
    public class EconomyConfig
    {
        /// <summary>Coins granted once, when no save file exists yet.</summary>
        public int startingCoins = 0;
        /// <summary>Coins earned at the end of a run = floor(score * coinsPerScorePoint).</summary>
        public float coinsPerScorePoint = 1f;
        /// <summary>Lower bound for the reward of a run that scored at least one point.</summary>
        public int minCoinsPerRun = 0;
    }

    [Serializable]
    public class ShopSectionConfig
    {
        public string id;
        public string title;
        public bool visible = true;
    }

    [Serializable]
    public class ShopItemConfig
    {
        public string id;
        /// <summary>Id of the section the item is listed in.</summary>
        public string section;
        /// <summary>See <see cref="ItemKinds"/>.</summary>
        public string kind;
        public string name;
        public string description;
        public int price;
        /// <summary>Disabled items are hidden from the shop and cannot be bought; what the player already owns is kept.</summary>
        public bool enabled = true;
        /// <summary>Equippable items only: owned from the start (the default theme of its slot).</summary>
        public bool ownedByDefault;
        /// <summary>Key into the icon table of ShopResources (power-ups).</summary>
        public string icon;

        // ---- power-ups -------------------------------------------------------------------------
        /// <summary>Id of the registered effect implementation (see PowerUpEffectRegistry).</summary>
        public string effect;
        /// <summary>Effect length in seconds, for timed effects.</summary>
        public float duration;
        /// <summary>Maximum number of this power-up the player can hold.</summary>
        public int maxOwned = 99;

        // ---- themes ----------------------------------------------------------------------------
        /// <summary>Tint applied to the ball (player theme) or to the ring (world theme), "#RRGGBB".</summary>
        public string color = "#FFFFFF";

        public bool IsPowerUp => kind == ItemKinds.PowerUp;
        public int MaxOwnedOrDefault => maxOwned > 0 ? maxOwned : 99;

        public Color GetColor(Color fallback)
        {
            return !string.IsNullOrEmpty(color) && ColorUtility.TryParseHtmlString(color, out var parsed)
                ? parsed
                : fallback;
        }
    }
}
