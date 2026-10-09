using System.Collections.Generic;
using DG.Tweening;
using Enums;
using Shop.Core;
using Shop.Model;
using Shop.Runtime;
using UnityEngine;
using UnityEngine.UI;

namespace Shop.Views
{
    /// <summary>
    /// Power-up controls along the bottom of the game screen.
    /// Only usable (visible and tappable) while a run is active.
    /// </summary>
    public sealed class PowerUpHud : MonoBehaviour
    {
        private const float MaxButtonSize = 210f;
        private const float ButtonGap = 28f;

        private GameSession _session;
        private CanvasGroup _group;
        private Toast _toast;
        private readonly List<PowerUpButton> _buttons = new List<PowerUpButton>();

        public static PowerUpHud Create(Canvas canvas, GameSession session)
        {
            var art = ShopResources.Load();
            if (art == null)
                return null;

            var root = UiFactory.CreateRect("ShopHud", canvas.transform);
            UiFactory.Stretch(root);

            var hud = root.gameObject.AddComponent<PowerUpHud>();
            hud._session = session;

            // Only the controls fade with the run state; the toast lives outside the group.
            var controls = UiFactory.CreateRect("Controls", root);
            UiFactory.Stretch(controls);
            hud._group = controls.gameObject.AddComponent<CanvasGroup>();
            hud._group.alpha = 0f;
            hud._group.blocksRaycasts = false;
            hud._group.interactable = false;

            hud.BuildButtons(controls, art, canvas, session.Service);
            hud._toast = Toast.Create(root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 330f));
            return hud;
        }

        private void BuildButtons(RectTransform root, ShopResources art, Canvas canvas, ShopService service)
        {
            var items = new List<ShopItemConfig>();
            foreach (var item in service.Catalog.Items)
            {
                if (item.IsPowerUp && item.enabled)
                    items.Add(item);
            }

            if (items.Count == 0)
                return;

            var size = Mathf.Min(MaxButtonSize, (960f - ButtonGap * (items.Count - 1)) / items.Count);
            var total = items.Count * size + (items.Count - 1) * ButtonGap;
            var strip = UiFactory.CreateRect("Buttons", root);
            UiFactory.SetAnchored(strip, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 90f), new Vector2(total, size));

            for (var i = 0; i < items.Count; i++)
            {
                var button = PowerUpButton.Create(strip, items[i], art, size, _group, canvas);
                UiFactory.SetAnchored(button.GetComponent<RectTransform>(), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                    new Vector2(i * (size + ButtonGap), 0f), new Vector2(size, size));
                button.Pressed += OnPressed;
                _buttons.Add(button);
            }
        }

        private void Update()
        {
            foreach (var button in _buttons)
            {
                var effect = _session.GetEffect(button.Item.id);
                var active = effect != null && effect.IsActive;
                var remaining = effect != null ? effect.RemainingSeconds : -1f;
                button.Refresh(_session.Service.GetPowerUpCount(button.Item.id), active, remaining);
            }
        }

        public void SetRunActive(bool active)
        {
            _group.DOKill();
            _group.blocksRaycasts = active;
            _group.interactable = active;
            _group.DOFade(active ? 1f : 0f, 0.25f);
        }

        private void OnPressed(PowerUpButton button)
        {
            switch (_session.TryUsePowerUp(button.Item.id))
            {
                case PowerUpUseResult.Used:
                    Sfx.Play(SoundType.ButtonClick);
                    button.Pop();
                    break;
                case PowerUpUseResult.NoStock:
                    Reject(button, "NONE LEFT - GET MORE IN THE SHOP");
                    break;
                case PowerUpUseResult.AlreadyActive:
                    Reject(button, "ALREADY ACTIVE");
                    break;
                case PowerUpUseResult.NothingToAffect:
                    Reject(button, "NOTHING TO CLEAR");
                    break;
            }
        }

        private void Reject(PowerUpButton button, string message)
        {
            Sfx.Play(SoundType.UIMove);
            button.Shake();
            _toast.Show(message, UiFactory.Navy);
        }
    }
}
