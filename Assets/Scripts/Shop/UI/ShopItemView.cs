using System;
using DG.Tweening;
using Shop.Core;
using Shop.Model;
using Shop.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop.Views
{
    /// <summary>One row of the shop: preview, name, description and a single context-dependent button (price / equip / equipped).</summary>
    public sealed class ShopItemView : MonoBehaviour
    {
        private const float CardHeight = 250f;
        private static readonly Color AffordableText = UiFactory.Ink;
        private static readonly Color UnaffordableText = new Color(0.78f, 0.16f, 0.16f, 1f);

        public ShopItemConfig Item { get; private set; }
        public event Action<ShopItemView> Clicked;

        private ShopResources _art;
        private RectTransform _buttonRect;
        private Image _buttonImage;
        private TextMeshProUGUI _buttonLabel;
        private RectTransform _labelRect;
        private Image _coinIcon;
        private Image _stockBadge;
        private TextMeshProUGUI _stockText;

        public static ShopItemView Create(Transform parent, ShopItemConfig item, ShopResources art)
        {
            var card = UiFactory.CreateRect("Item_" + item.id, parent);
            var element = card.gameObject.AddComponent<LayoutElement>();
            element.minHeight = CardHeight;
            element.preferredHeight = CardHeight;

            var background = card.gameObject.AddComponent<Image>();
            background.sprite = art.cardSprite;
            background.type = art.cardSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            background.color = Color.white;
            background.raycastTarget = true;

            var view = card.gameObject.AddComponent<ShopItemView>();
            view.Item = item;
            view._art = art;
            view.BuildPreview(card);
            view.BuildTexts(card);
            view.BuildButton(card);
            return view;
        }

        private void BuildPreview(RectTransform card)
        {
            var holder = UiFactory.CreateRect("Preview", card);
            UiFactory.SetAnchored(holder, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(170f, 170f));

            if (Item.IsPowerUp)
            {
                UiFactory.CreateImage(holder, "Disc", _art.circleSprite, UiFactory.Navy, false).rectTransform.SetStretch();
                var icon = UiFactory.CreateImage(holder, "Icon", _art.GetIcon(Item.icon), Color.white, false);
                UiFactory.SetAnchored(icon.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(100f, 100f));
                BuildStockBadge(holder);
            }
            else if (Item.kind == ItemKinds.WorldTheme)
            {
                // The ring is drawn as a thin outline, so it reads well on the white card.
                UiFactory.CreateImage(holder, "Ring", _art.ringSprite, Item.GetColor(Color.black), false).rectTransform.SetStretch();
            }
            else
            {
                var ball = UiFactory.CreateImage(holder, "Ball", _art.circleSprite, Item.GetColor(Color.white), false);
                UiFactory.SetAnchored(ball.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(140f, 140f));
            }
        }

        private void BuildStockBadge(RectTransform holder)
        {
            _stockBadge = UiFactory.CreateImage(holder, "StockBadge", _art.circleSprite, UiFactory.Gold, false);
            UiFactory.SetAnchored(_stockBadge.rectTransform, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(14f, -6f), new Vector2(76f, 76f));

            _stockText = UiFactory.CreateText(_stockBadge.rectTransform, "Count", "0", 46f, UiFactory.Ink, TMPro.TextAlignmentOptions.Center);
            _stockText.rectTransform.SetStretch();
            _stockText.enableAutoSizing = true;
            _stockText.fontSizeMin = 24f;
            _stockText.fontSizeMax = 46f;
        }

        private void BuildTexts(RectTransform card)
        {
            var name = UiFactory.CreateText(card, "Name", Item.name, 62f, UiFactory.Navy, TMPro.TextAlignmentOptions.BottomLeft);
            UiFactory.SetOffsets(name.rectTransform, new Vector2(0f, 0.45f), new Vector2(1f, 1f), new Vector2(250f, 0f), new Vector2(-400f, -16f));

            var description = UiFactory.CreateText(card, "Description", Item.description, 38f, new Color(0.35f, 0.35f, 0.35f, 1f), TMPro.TextAlignmentOptions.TopLeft);
            UiFactory.SetOffsets(description.rectTransform, new Vector2(0f, 0f), new Vector2(1f, 0.45f), new Vector2(250f, 16f), new Vector2(-400f, -4f));
        }

        private void BuildButton(RectTransform card)
        {
            _buttonRect = UiFactory.CreateRect("Button", card);
            UiFactory.SetAnchored(_buttonRect, new Vector2(1f, 0.5f), new Vector2(1f, 0.5f), new Vector2(-36f, 0f), new Vector2(340f, 140f));

            _buttonImage = _buttonRect.gameObject.AddComponent<Image>();
            _buttonImage.sprite = _art.buttonSprite;
            _buttonImage.type = _art.buttonSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            _buttonImage.color = UiFactory.Sky;

            var button = _buttonRect.gameObject.AddComponent<Button>();
            button.targetGraphic = _buttonImage;
            button.transition = Selectable.Transition.SpriteSwap;
            button.spriteState = new SpriteState { pressedSprite = _art.buttonPressedSprite };
            button.navigation = new Navigation { mode = Navigation.Mode.None };
            button.onClick.AddListener(() => Clicked?.Invoke(this));

            _coinIcon = UiFactory.CreateImage(_buttonRect, "Coin", _art.GetIcon("coin"), Color.white, false);
            UiFactory.SetAnchored(_coinIcon.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(34f, 6f), new Vector2(66f, 66f));

            _buttonLabel = UiFactory.CreateText(_buttonRect, "Label", string.Empty, 60f, AffordableText, TMPro.TextAlignmentOptions.Center);
            _labelRect = _buttonLabel.rectTransform;
            _buttonLabel.enableAutoSizing = true;
            _buttonLabel.fontSizeMin = 30f;
            _buttonLabel.fontSizeMax = 60f;
            IconTextAligner.Attach(_coinIcon.rectTransform, _buttonLabel);
        }

        // ---- state ---------------------------------------------------------------------------------

        public void Refresh(ShopService service)
        {
            if (Item.IsPowerUp)
            {
                var count = service.GetPowerUpCount(Item.id);
                if (_stockBadge != null)
                {
                    _stockBadge.gameObject.SetActive(count > 0);
                    _stockText.text = count.ToString();
                }

                if (count >= Item.MaxOwnedOrDefault)
                    ShowStatic("MAX");
                else
                    ShowPrice(service.Coins >= Item.price);
                return;
            }

            if (!service.IsOwned(Item.id))
                ShowPrice(service.Coins >= Item.price);
            else if (service.IsEquipped(Item.id))
                ShowStatic("EQUIPPED");
            else
                ShowAction("EQUIP");
        }

        private void ShowPrice(bool affordable)
        {
            SetButtonLook(false, UiFactory.Sky, Item.price.ToString(), affordable ? AffordableText : UnaffordableText, true);
        }

        private void ShowAction(string text)
        {
            SetButtonLook(false, UiFactory.Sky, text, AffordableText, false);
        }

        private void ShowStatic(string text)
        {
            SetButtonLook(true, UiFactory.Muted, text, UiFactory.Navy, false);
        }

        private void SetButtonLook(bool pressedStyle, Color color, string text, Color textColor, bool showCoin)
        {
            _buttonImage.sprite = pressedStyle && _art.buttonPressedSprite != null ? _art.buttonPressedSprite : _art.buttonSprite;
            _buttonImage.color = color;
            _buttonLabel.text = text;
            _buttonLabel.color = textColor;
            _coinIcon.gameObject.SetActive(showCoin);
            UiFactory.SetOffsets(_labelRect, Vector2.zero, Vector2.one, new Vector2(showCoin ? 100f : 12f, 12f), new Vector2(-12f, -4f));
        }

        // ---- feedback ------------------------------------------------------------------------------

        public void Punch()
        {
            transform.DOKill(true);
            transform.DOPunchScale(Vector3.one * 0.05f, 0.3f, 6, 0.6f);
        }

        /// <summary>Shake + red flash on the button: the visible "can't buy this" signal.</summary>
        public void Reject()
        {
            _buttonRect.DOKill(true);
            _buttonImage.DOKill(true);
            _buttonRect.DOShakeAnchorPos(0.4f, new Vector2(24f, 0f), 22, 0f, false, true);
            _buttonImage.DOColor(UiFactory.Danger, 0.1f).SetLoops(4, LoopType.Yoyo);
        }
    }

    internal static class RectTransformExtensions
    {
        public static void SetStretch(this RectTransform rect)
        {
            UiFactory.Stretch(rect);
        }
    }
}
