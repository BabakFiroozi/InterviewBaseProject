using Shop.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop.Views
{
    /// <summary>
    /// Keeps a coin icon to the right of the (centred) score text of the game-over screen. The icon is a child of the
    /// text, so it moves, scales and hides together with the game-over animation; only its horizontal position is
    /// updated when the text changes width.
    /// </summary>
    public sealed class ScoreCoinIcon : MonoBehaviour
    {
        private const float IconSize = 80f;
        private const float Gap = 18f;

        private TextMeshProUGUI _text;
        private RectTransform _icon;
        private string _lastText;

        public static void Attach(TextMeshProUGUI scoreText)
        {
            var art = ShopResources.Load();
            if (scoreText == null || art == null || scoreText.GetComponent<ScoreCoinIcon>() != null)
                return;

            var icon = UiFactory.CreateImage(scoreText.transform, "CoinIcon", art.GetIcon("coin"), Color.white, false);
            var rect = icon.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(IconSize, IconSize);

            var follower = scoreText.gameObject.AddComponent<ScoreCoinIcon>();
            follower._text = scoreText;
            follower._icon = rect;
            follower.Reposition();
        }

        private void OnEnable()
        {
            // The game-over panel starts inactive; measure again as soon as it is shown.
            _lastText = null;
        }

        private void LateUpdate()
        {
            if (_text.text != _lastText)
                Reposition();
        }

        private void Reposition()
        {
            _lastText = _text.text;

            // X: right of the text. Y: centre of the drawn letters (the text rect's own centre is not the visual one).
            var textWidth = _text.GetPreferredValues(_lastText).x;
            var y = GlyphMetrics.TryGetCenterY(_text, out var glyphCenter)
                ? glyphCenter - _text.rectTransform.rect.center.y
                : 0f;
            _icon.anchoredPosition = new Vector2(textWidth * 0.5f + Gap + IconSize * 0.5f, y);
        }
    }
}
