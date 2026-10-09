using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Shop.Runtime;

namespace Shop.Views
{
    /// <summary>A short message pill that fades in and out. One instance is reused for every message.</summary>
    public sealed class Toast : MonoBehaviour
    {
        private CanvasGroup _group;
        private Image _background;
        private TextMeshProUGUI _label;
        private Sequence _sequence;

        public static Toast Create(Transform parent, Vector2 anchor, Vector2 pivot, Vector2 position)
        {
            var art = ShopResources.Load();
            var rect = UiFactory.CreateRect("Toast", parent);
            UiFactory.SetAnchored(rect, anchor, pivot, position, new Vector2(900f, 120f));

            var toast = rect.gameObject.AddComponent<Toast>();
            toast._group = rect.gameObject.AddComponent<CanvasGroup>();
            toast._group.alpha = 0f;
            toast._group.blocksRaycasts = false;
            toast._group.interactable = false;

            toast._background = rect.gameObject.AddComponent<Image>();
            toast._background.sprite = art != null ? art.pillSprite : null;
            toast._background.type = toast._background.sprite != null ? Image.Type.Sliced : Image.Type.Simple;
            toast._background.color = UiFactory.Navy;
            toast._background.raycastTarget = false;

            toast._label = UiFactory.CreateText(rect, "Text", string.Empty, 48f, Color.white, TextAlignmentOptions.Center);
            UiFactory.Stretch(toast._label.rectTransform, 30f, 0f, 30f, 0f);
            toast._label.enableAutoSizing = true;
            toast._label.fontSizeMin = 26f;
            toast._label.fontSizeMax = 48f;
            return toast;
        }

        public void Show(string message, Color background)
        {
            _sequence?.Kill();
            _label.text = message;
            _background.color = background;
            _group.alpha = 0f;
            transform.localScale = Vector3.one * 0.9f;

            _sequence = DOTween.Sequence().SetLink(gameObject);
            _sequence.Append(_group.DOFade(1f, 0.12f));
            _sequence.Join(transform.DOScale(1f, 0.2f).SetEase(Ease.OutBack));
            _sequence.AppendInterval(1.6f);
            _sequence.Append(_group.DOFade(0f, 0.25f));
        }
    }
}
