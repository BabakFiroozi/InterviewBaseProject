using Shop.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop.Views
{
    /// <summary>Palette of the game plus small helpers to build uGUI hierarchies from code.</summary>
    public static class UiFactory
    {
        public static readonly Color Ink = new Color(0.196f, 0.196f, 0.196f, 1f);
        public static readonly Color Navy = new Color(0.1098f, 0.2706f, 0.4431f, 1f);
        public static readonly Color Sky = new Color(0.3725f, 0.7922f, 0.9961f, 1f);
        public static readonly Color Teal = new Color(0.2196f, 0.4941f, 0.6235f, 1f);
        public static readonly Color Danger = new Color(1f, 0.42f, 0.42f, 1f);
        public static readonly Color Gold = new Color(1f, 0.82f, 0.25f, 1f);
        public static readonly Color Muted = new Color(0.78f, 0.88f, 0.94f, 1f);

        private const int UiLayer = 5;

        public static RectTransform CreateRect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.layer = UiLayer;
            var rect = (RectTransform)go.transform;
            rect.SetParent(parent, false);
            return rect;
        }

        public static void Stretch(RectTransform rect, float left = 0f, float top = 0f, float right = 0f, float bottom = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
        }

        public static void SetAnchored(RectTransform rect, Vector2 anchor, Vector2 pivot, Vector2 position, Vector2 size)
        {
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        public static void SetOffsets(RectTransform rect, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        public static Image CreateImage(Transform parent, string name, Sprite sprite, Color color, bool sliced, bool raycastTarget = false)
        {
            var rect = CreateRect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = sprite != null && sliced ? Image.Type.Sliced : Image.Type.Simple;
            image.preserveAspect = !sliced;
            image.raycastTarget = raycastTarget;
            return image;
        }

        public static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, Color color, TextAlignmentOptions alignment)
        {
            var rect = CreateRect(name, parent);
            var label = rect.gameObject.AddComponent<TextMeshProUGUI>();
            var art = ShopResources.Load();
            if (art != null && art.font != null)
                label.font = art.font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = alignment;
            label.raycastTarget = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
            return label;
        }
    }
}
