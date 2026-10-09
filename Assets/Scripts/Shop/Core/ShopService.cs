using System;
using Shop.Model;
using UnityEngine;

namespace Shop.Core
{
    public enum PurchaseResult
    {
        Success,
        NotEnoughCoins,
        AlreadyOwned,
        MaxOwnedReached,
        UnknownItem,
        ItemDisabled
    }

    /// <summary>
    /// The rules of the shop: currency, ownership, equipping and power-up stock. It owns no UI and no
    /// scene references; every mutation is written through <c>persist</c> immediately so nothing is lost
    /// if the app is killed, and then announced through <see cref="Changed"/>.
    /// </summary>
    public sealed class ShopService
    {
        private readonly ShopCatalog _catalog;
        private readonly SaveData _data;
        private readonly Action _persist;

        public event Action Changed;

        public ShopCatalog Catalog => _catalog;
        public int Coins => _data.coins;

        public ShopService(ShopCatalog catalog, SaveData data, Action persist)
        {
            _catalog = catalog;
            _data = data;
            _persist = persist ?? (() => { });
        }

        // ---- queries -----------------------------------------------------------------------------

        /// <summary>Themes: unlocked. Power-ups: at least one in stock.</summary>
        public bool IsOwned(string id)
        {
            if (!_catalog.TryGetItem(id, out var item))
                return false;

            if (item.IsPowerUp)
                return GetPowerUpCount(id) > 0;

            return item.ownedByDefault || _data.ownedItems.Contains(id);
        }

        public int GetPowerUpCount(string id)
        {
            foreach (var entry in _data.powerUps)
            {
                if (entry.id == id)
                    return entry.count;
            }

            return 0;
        }

        public string GetEquippedId(string kind)
        {
            foreach (var entry in _data.equipped)
            {
                if (entry.slot != kind)
                    continue;

                if (_catalog.TryGetItem(entry.itemId, out var item) && item.kind == kind && IsOwned(item.id))
                    return item.id;
                break;
            }

            var fallback = _catalog.GetDefaultItem(kind);
            return fallback?.id;
        }

        public ShopItemConfig GetEquippedItem(string kind)
        {
            _catalog.TryGetItem(GetEquippedId(kind), out var item);
            return item;
        }

        public bool IsEquipped(string id)
        {
            return _catalog.TryGetItem(id, out var item) && !item.IsPowerUp && GetEquippedId(item.kind) == id;
        }

        public int CoinsForScore(int score)
        {
            if (score <= 0)
                return 0;

            var economy = _catalog.Economy;
            var coins = Mathf.FloorToInt(score * economy.coinsPerScorePoint);
            return Mathf.Max(coins, economy.minCoinsPerRun);
        }

        // ---- commands ----------------------------------------------------------------------------

        public int AddCoins(int amount)
        {
            if (amount <= 0)
                return 0;

            _data.coins = (int)Math.Min((long)_data.coins + amount, int.MaxValue);
            Commit();
            return amount;
        }

        /// <summary>Buys one item. Themes are unlocked (not equipped); power-ups add one to the stock.</summary>
        public PurchaseResult Purchase(string id)
        {
            if (!_catalog.TryGetItem(id, out var item))
                return PurchaseResult.UnknownItem;
            if (!item.enabled)
                return PurchaseResult.ItemDisabled;

            if (item.IsPowerUp)
            {
                if (GetPowerUpCount(id) >= item.MaxOwnedOrDefault)
                    return PurchaseResult.MaxOwnedReached;
            }
            else if (IsOwned(id))
            {
                return PurchaseResult.AlreadyOwned;
            }

            if (_data.coins < item.price)
                return PurchaseResult.NotEnoughCoins;

            _data.coins -= item.price;
            if (item.IsPowerUp)
                SetPowerUpCount(id, GetPowerUpCount(id) + 1);
            else
                _data.ownedItems.Add(id);

            Commit();
            return PurchaseResult.Success;
        }

        /// <summary>Equips an owned theme, replacing the one in the same slot.</summary>
        public bool Equip(string id)
        {
            if (!_catalog.TryGetItem(id, out var item) || item.IsPowerUp || !IsOwned(id))
                return false;

            var entry = _data.equipped.Find(e => e.slot == item.kind);
            if (entry == null)
                _data.equipped.Add(new EquipEntry { slot = item.kind, itemId = id });
            else
                entry.itemId = id;

            Commit();
            return true;
        }

        /// <summary>Takes one power-up out of the stock. Fails when none is left.</summary>
        public bool TryConsumePowerUp(string id)
        {
            if (!_catalog.TryGetItem(id, out var item) || !item.IsPowerUp)
                return false;

            var count = GetPowerUpCount(id);
            if (count <= 0)
                return false;

            SetPowerUpCount(id, count - 1);
            Commit();
            return true;
        }

        // ---- internals ---------------------------------------------------------------------------

        private void SetPowerUpCount(string id, int count)
        {
            var entry = _data.powerUps.Find(e => e.id == id);
            if (entry == null)
                _data.powerUps.Add(new CountEntry { id = id, count = count });
            else
                entry.count = count;
        }

        private void Commit()
        {
            _persist();
            Changed?.Invoke();
        }
    }
}
