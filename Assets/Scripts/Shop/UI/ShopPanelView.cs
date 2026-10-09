using System.Collections.Generic;
using DG.Tweening;
using Enums;
using Shop.Core;
using Shop.Model;
using Shop.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Shop.Views
{
    /// <summary>
    /// Content of the menu's Shop tab (sits on Prefab_ShopPanel, replacing the old "Coming soon" label).
    /// The list is generated from the catalog: sections and items in the order given by the JSON config.
    /// </summary>
    public sealed class ShopPanelView : MonoBehaviour
    {
        private const float HeaderHeight = 250f;

        private ShopSystem _shop;
        private ShopResources _art;
        private Toast _toast;
        private RectTransform _balancePill;
        private Image _balanceBackground;
        private TextMeshProUGUI _balanceText;
        private readonly List<ShopItemView> _itemViews = new List<ShopItemView>();
        private bool _built;

        private void Awake()
        {
            _shop = ShopSystem.Instance;
            _art = ShopResources.Load();
            if (_shop == null || _art == null)
            {
                Debug.LogError("[Shop] ShopSystem or ShopResources missing; the shop panel stays empty.");
                return;
            }

            Build();
        }

        private void OnEnable()
        {
            if (!_built)
                return;

            _shop.Service.Changed += Refresh;
            Refresh();
        }

        private void OnDisable()
        {
            if (_built)
                _shop.Service.Changed -= Refresh;
        }

        // ---- construction --------------------------------------------------------------------------

        private void Build()
        {
            var root = (RectTransform)transform;
            BuildHeader(root);
            var content = BuildScrollArea(root);

            foreach (var section in _shop.Catalog.Sections)
            {
                if (section.Items.Count == 0)
                    continue;

                BuildSectionHeader(content, section.Title);
                foreach (var item in section.Items)
                {
                    var view = ShopItemView.Create(content, item, _art);
                    view.Clicked += OnItemClicked;
                    _itemViews.Add(view);
                }
            }

            _toast = Toast.Create(root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 40f));
            _built = true;
        }

        private void BuildHeader(RectTransform root)
        {
            var title = UiFactory.CreateText(root, "Title", "Shop", 150f, UiFactory.Navy, TextAlignmentOptions.TopLeft);
            UiFactory.SetAnchored(title.rectTransform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(70f, -50f), new Vector2(520f, 170f));

            _balancePill = UiFactory.CreateRect("Balance", root);
            UiFactory.SetAnchored(_balancePill, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-60f, -70f), new Vector2(400f, 130f));

            _balanceBackground = _balancePill.gameObject.AddComponent<Image>();
            _balanceBackground.sprite = _art.pillSprite;
            _balanceBackground.type = _art.pillSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            _balanceBackground.color = UiFactory.Navy;
            _balanceBackground.raycastTarget = false;

            var coin = UiFactory.CreateImage(_balancePill, "Coin", _art.GetIcon("coin"), Color.white, false);
            UiFactory.SetAnchored(coin.rectTransform, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(26f, 0f), new Vector2(80f, 80f));

            _balanceText = UiFactory.CreateText(_balancePill, "Amount", "0", 80f, Color.white, TextAlignmentOptions.Center);
            UiFactory.Stretch(_balanceText.rectTransform, 120f, 0f, 24f, 0f);
            _balanceText.enableAutoSizing = true;
            _balanceText.fontSizeMin = 36f;
            _balanceText.fontSizeMax = 80f;
            IconTextAligner.Attach(coin.rectTransform, _balanceText);
        }

        private RectTransform BuildScrollArea(RectTransform root)
        {
            var scrollRect = UiFactory.CreateRect("Scroll", root);
            UiFactory.SetOffsets(scrollRect, Vector2.zero, Vector2.one, Vector2.zero, new Vector2(0f, -HeaderHeight));

            var viewport = UiFactory.CreateRect("Viewport", scrollRect);
            UiFactory.Stretch(viewport);
            var hitArea = viewport.gameObject.AddComponent<Image>();
            hitArea.color = new Color(1f, 1f, 1f, 0f); // invisible, but lets empty areas receive drags
            viewport.gameObject.AddComponent<RectMask2D>();

            var content = UiFactory.CreateRect("Content", viewport);
            content.anchorMin = new Vector2(0f, 1f);
            content.anchorMax = new Vector2(1f, 1f);
            content.pivot = new Vector2(0.5f, 1f);
            content.offsetMin = Vector2.zero;
            content.offsetMax = Vector2.zero;

            var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(50, 50, 10, 180);
            layout.spacing = 28f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            var fitter = content.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollRect.gameObject.AddComponent<ShopScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            scroll.horizontal = false;
            scroll.vertical = true;
            scroll.movementType = ScrollRect.MovementType.Elastic;
            scroll.scrollSensitivity = 40f;
            return content;
        }

        private void BuildSectionHeader(RectTransform content, string title)
        {
            var header = UiFactory.CreateRect("Section_" + title, content);
            var element = header.gameObject.AddComponent<LayoutElement>();
            element.minHeight = 100f;
            element.preferredHeight = 100f;

            var background = header.gameObject.AddComponent<Image>();
            background.sprite = _art.tabSprite;
            background.type = _art.tabSprite != null ? Image.Type.Sliced : Image.Type.Simple;
            background.color = UiFactory.Navy;
            background.raycastTarget = true;

            var label = UiFactory.CreateText(header, "Title", title, 56f, Color.white, TextAlignmentOptions.MidlineLeft);
            UiFactory.Stretch(label.rectTransform, 40f, 0f, 40f, 0f);
        }

        // ---- behaviour -----------------------------------------------------------------------------

        private void Refresh()
        {
            var service = _shop.Service;
            _balanceText.text = service.Coins.ToString();
            foreach (var view in _itemViews)
                view.Refresh(service);
        }

        private void OnItemClicked(ShopItemView view)
        {
            var service = _shop.Service;
            var item = view.Item;

            // Owned themes: the button equips. Owning and equipping are separate steps.
            if (!item.IsPowerUp && service.IsOwned(item.id))
            {
                if (!service.IsEquipped(item.id) && service.Equip(item.id))
                {
                    Sfx.Play(SoundType.ButtonClick);
                    view.Punch();
                }

                return;
            }

            switch (service.Purchase(item.id))
            {
                case PurchaseResult.Success:
                    Sfx.Play(SoundType.Score);
                    view.Punch();
                    PunchBalance();
                    if (item.IsPowerUp)
                    {
                        _toast.Show(item.name + " ADDED", UiFactory.Navy);
                    }
                    else
                    {
                        // A freshly bought theme is equipped right away (owning and equipping stay separate in the
                        // service, so it can still be switched back to any owned theme later).
                        service.Equip(item.id);
                        _toast.Show(item.name + " UNLOCKED & EQUIPPED", UiFactory.Navy);
                    }

                    break;

                case PurchaseResult.NotEnoughCoins:
                    Sfx.Play(SoundType.UIMove);
                    view.Reject();
                    RejectBalance();
                    _toast.Show("NOT ENOUGH COINS - NEED " + (item.price - service.Coins) + " MORE", UiFactory.Danger);
                    break;

                case PurchaseResult.MaxOwnedReached:
                    Sfx.Play(SoundType.UIMove);
                    view.Reject();
                    _toast.Show("YOU CAN'T CARRY MORE " + item.name, UiFactory.Danger);
                    break;

                default:
                    Refresh();
                    break;
            }
        }

        private void PunchBalance()
        {
            _balancePill.DOKill(true);
            _balancePill.DOPunchScale(Vector3.one * 0.12f, 0.3f, 6, 0.6f);
        }

        private void RejectBalance()
        {
            _balancePill.DOKill(true);
            _balanceBackground.DOKill(true);
            _balancePill.DOShakeAnchorPos(0.4f, new Vector2(18f, 0f), 22, 0f, false, true);
            _balanceBackground.DOColor(UiFactory.Danger, 0.1f).SetLoops(4, LoopType.Yoyo);
        }
    }
}
