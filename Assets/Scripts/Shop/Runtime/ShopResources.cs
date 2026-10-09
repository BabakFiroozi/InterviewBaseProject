using System;
using System.Collections.Generic;
using ScriptableObjects.GameEvents;
using TMPro;
using UnityEngine;

namespace Shop.Runtime
{
    /// <summary>
    /// Scene-independent references the shop needs: the existing game-event assets it listens to, and
    /// the sprites/font that give the shop UI the same look as the rest of the game. Lives in
    /// Resources/Shop so runtime code can load it without any scene wiring.
    /// </summary>
    [CreateAssetMenu(menuName = "Shop/Shop Resources")]
    public sealed class ShopResources : ScriptableObject
    {
        [Serializable]
        public struct IconEntry
        {
            public string key;
            public Sprite sprite;
        }

        [Header("Existing game events (listened to, never raised)")]
        public NullEvent onBallStartMoving;
        public NullEvent onGameEnded;
        public IntEvent onScoreChanged;

        [Header("UI style")]
        public TMP_FontAsset font;
        public Sprite buttonSprite;
        public Sprite buttonPressedSprite;
        public Sprite cardSprite;
        public Sprite pillSprite;
        public Sprite tabSprite;
        public Sprite circleSprite;
        public Sprite ringSprite;

        [Header("Icons (key used by ShopItemConfig.icon)")]
        public List<IconEntry> icons = new List<IconEntry>();

        private const string ResourcePath = "Shop/ShopResources";
        private static ShopResources _instance;

        public static ShopResources Load()
        {
            if (_instance == null)
                _instance = Resources.Load<ShopResources>(ResourcePath);
            if (_instance == null)
                Debug.LogError($"[Shop] Missing Resources/{ResourcePath}.asset");
            return _instance;
        }

        public Sprite GetIcon(string key)
        {
            if (string.IsNullOrEmpty(key))
                return null;

            foreach (var entry in icons)
            {
                if (entry.key == key)
                    return entry.sprite;
            }

            return null;
        }
    }
}
