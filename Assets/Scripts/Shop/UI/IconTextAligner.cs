using TMPro;
using UnityEngine;

namespace Shop.Views
{
    /// <summary>Measures where the letters/digits of a TMP text are actually drawn.</summary>
    public static class GlyphMetrics
    {
        /// <summary>
        /// Vertical centre of the drawn glyphs in the text's local space. This is not the centre of the text rect: the
        /// rect (and TMP's own bounds) include the font's ascender/descender space, so the letters sit slightly above or
        /// below it. Returns false when nothing visible could be measured.
        /// </summary>
        public static bool TryGetCenterY(TMP_Text text, out float centerY)
        {
            centerY = 0f;
            if (text == null)
                return false;

            text.ForceMeshUpdate();
            var info = text.textInfo;
            var min = float.MaxValue;
            var max = float.MinValue;
            for (var i = 0; i < info.characterCount; i++)
            {
                var character = info.characterInfo[i];
                if (!character.isVisible || !char.IsLetterOrDigit(character.character))
                    continue;

                min = Mathf.Min(min, character.bottomLeft.y);
                max = Mathf.Max(max, character.topRight.y);
            }

            if (min > max)
                return false;

            centerY = (min + max) * 0.5f;
            return true;
        }
    }

    /// <summary>
    /// Keeps an icon vertically centred on the glyphs of a text that sits in the same parent (icon and text are
    /// siblings). Only Y is touched, the icon's X stays whatever the layout set. The icon must have its pivot at 0.5 on Y.
    /// </summary>
    public sealed class IconTextAligner : MonoBehaviour
    {
        private TMP_Text _text;
        private RectTransform _icon;
        private RectTransform _parent;
        private string _measuredText;
        private float _glyphCenterY;

        public static void Attach(RectTransform icon, TMP_Text text)
        {
            if (icon == null || text == null || icon.GetComponent<IconTextAligner>() != null)
                return;

            var aligner = icon.gameObject.AddComponent<IconTextAligner>();
            aligner._icon = icon;
            aligner._text = text;
            aligner._parent = icon.parent as RectTransform;
        }

        private void OnEnable()
        {
            _measuredText = null;
        }

        private void LateUpdate()
        {
            if (_parent == null)
                return;

            if (_text.text != _measuredText)
            {
                _measuredText = _text.text;
                if (!GlyphMetrics.TryGetCenterY(_text, out _glyphCenterY))
                    _glyphCenterY = _text.rectTransform.rect.center.y;
            }

            // Glyph centre -> parent space -> offset from the icon's anchor line.
            var world = _text.rectTransform.TransformPoint(new Vector3(0f, _glyphCenterY, 0f));
            var localY = _parent.InverseTransformPoint(world).y;
            var anchorY = _parent.rect.yMin + _parent.rect.height * _icon.anchorMin.y;
            var position = _icon.anchoredPosition;
            position.y = localY - anchorY;
            _icon.anchoredPosition = position;
        }
    }
}
