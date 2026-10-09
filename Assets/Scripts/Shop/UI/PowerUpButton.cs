using System;
using DG.Tweening;
using Shop.Model;
using Shop.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop.Views
{
    /// <summary>A power-up control on the in-game HUD: icon, remaining stock and, while running, the effect state.</summary>
    public sealed class PowerUpButton : MonoBehaviour, ITapBlocker
    {
        public ShopItemConfig Item { get; private set; }
        public event Action<PowerUpButton> Pressed;

        private RectTransform _rect;
        private Image _background;
        private Image _icon;
        private TextMeshProUGUI _count;
        private TextMeshProUGUI _status;
        private CanvasGroup _hudGroup;
        private Camera _camera;

        private int _shownStock = -1;
        private int _shownSeconds = int.MinValue;
        private bool _shownActive;

        public static PowerUpButton Create(Transform parent, ShopItemConfig item, ShopResources art, float size, CanvasGroup hudGroup, Canvas canvas)
        {
            var rect = UiFactory.CreateRect("PowerUp_" + item.id, parent);
            rect.sizeDelta = new Vector2(size, size);

            var button = rect.gameObject.AddComponent<PowerUpButton>();
            button.Item = item;
            button._rect = rect;
            button._hudGroup = hudGroup;
            button._camera = canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;

            button._background = rect.gameObject.AddComponent<Image>();
            button._background.sprite = art.buttonSprite;
            button._background.type = art.buttonSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            button._background.color = UiFactory.Sky;

            var control = rect.gameObject.AddComponent<Button>();
            control.targetGraphic = button._background;
            control.transition = Selectable.Transition.SpriteSwap;
            control.spriteState = new SpriteState { pressedSprite = art.buttonPressedSprite };
            control.navigation = new Navigation { mode = Navigation.Mode.None };
            control.onClick.AddListener(() => button.Pressed?.Invoke(button));

            button._icon = UiFactory.CreateImage(rect, "Icon", art.GetIcon(item.icon), UiFactory.Navy, false);
            UiFactory.SetAnchored(button._icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, size * 0.04f), new Vector2(size * 0.52f, size * 0.52f));

            var badge = UiFactory.CreateImage(rect, "CountBadge", art.circleSprite, UiFactory.Navy, false);
            UiFactory.SetAnchored(badge.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(size * 0.08f, size * 0.02f), new Vector2(size * 0.36f, size * 0.36f));
            button._count = UiFactory.CreateText(badge.rectTransform, "Count", "0", size * 0.22f, Color.white, TextAlignmentOptions.Center);
            UiFactory.Stretch(button._count.rectTransform);

            button._status = UiFactory.CreateText(rect, "Status", string.Empty, size * 0.2f, UiFactory.Navy, TextAlignmentOptions.Top);
            UiFactory.SetOffsets(button._status.rectTransform, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, -size * 0.24f), new Vector2(0f, -size * 0.04f));
            return button;
        }

        private void OnEnable()
        {
            HudTapGate.Register(this);
        }

        private void OnDisable()
        {
            HudTapGate.Unregister(this);
        }

        public bool Blocks(Vector2 screenPoint)
        {
            return isActiveAndEnabled
                   && _hudGroup != null && _hudGroup.blocksRaycasts
                   && RectTransformUtility.RectangleContainsScreenPoint(_rect, screenPoint, _camera);
        }

        /// <param name="remainingSeconds">Negative when the effect has no timer.</param>
        public void Refresh(int stock, bool active, float remainingSeconds)
        {
            var seconds = active && remainingSeconds >= 0f ? Mathf.CeilToInt(remainingSeconds) : -1;
            if (stock == _shownStock && active == _shownActive && seconds == _shownSeconds)
                return;

            if (stock != _shownStock)
                _count.text = stock.ToString();

            _shownStock = stock;
            _shownActive = active;
            _shownSeconds = seconds;

            _background.color = active ? UiFactory.Gold : (stock > 0 ? UiFactory.Sky : UiFactory.Muted);
            var iconColor = UiFactory.Navy;
            iconColor.a = stock > 0 || active ? 1f : 0.4f;
            _icon.color = iconColor;
            _status.text = !active ? string.Empty : (seconds >= 0 ? seconds + "S" : "ON");
        }

        public void Pop()
        {
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.18f, 0.3f, 6, 0.6f);
        }

        public void Shake()
        {
            _rect.DOKill(true);
            _rect.DOShakeAnchorPos(0.35f, new Vector2(20f, 0f), 22, 0f, false, true);
        }
    }
}
