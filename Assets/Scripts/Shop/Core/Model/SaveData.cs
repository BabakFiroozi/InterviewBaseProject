using System;
using System.Collections.Generic;

namespace Shop.Model
{
    /// <summary>
    /// Everything the shop persists. Serialised with JsonUtility, hence lists of small entries
    /// instead of dictionaries. Only ids are stored, never prices or names, so re-balancing the
    /// JSON config never invalidates an existing save.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int coins;
        /// <summary>Ids of owned equippable items (themes). Default items are implicitly owned.</summary>
        public List<string> ownedItems = new List<string>();
        public List<CountEntry> powerUps = new List<CountEntry>();
        public List<EquipEntry> equipped = new List<EquipEntry>();
    }

    [Serializable]
    public class CountEntry
    {
        public string id;
        public int count;
    }

    [Serializable]
    public class EquipEntry
    {
        /// <summary>The item kind, e.g. "playerTheme".</summary>
        public string slot;
        public string itemId;
    }
}
