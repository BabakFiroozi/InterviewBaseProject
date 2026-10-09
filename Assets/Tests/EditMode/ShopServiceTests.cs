using System.Collections.Generic;
using NUnit.Framework;
using Shop.Core;
using Shop.Model;
using UnityEngine.TestTools;

namespace Shop.Tests
{
    public class ShopServiceTests
    {
        private const string ConfigJson = @"{
            ""economy"": { ""startingCoins"": 0, ""coinsPerScorePoint"": 2, ""minCoinsPerRun"": 1 },
            ""sections"": [
                { ""id"": ""powerups"", ""title"": ""POWER-UPS"" },
                { ""id"": ""player"", ""title"": ""PLAYER THEMES"" }
            ],
            ""items"": [
                { ""id"": ""shield"", ""section"": ""powerups"", ""kind"": ""powerup"", ""price"": 50, ""maxOwned"": 2, ""effect"": ""shield"" },
                { ""id"": ""ball_default"", ""section"": ""player"", ""kind"": ""playerTheme"", ""price"": 0, ""ownedByDefault"": true },
                { ""id"": ""ball_red"", ""section"": ""player"", ""kind"": ""playerTheme"", ""price"": 100 },
                { ""id"": ""ball_blue"", ""section"": ""player"", ""kind"": ""playerTheme"", ""price"": 120 }
            ]
        }";

        private sealed class MemoryStorage : ISaveStorage
        {
            public readonly List<string> Files = new List<string>();

            public IEnumerable<string> ReadCandidates()
            {
                for (var i = Files.Count - 1; i >= 0; i--)
                    yield return Files[i];
            }

            public void Write(string json)
            {
                Files.Add(json);
            }
        }

        private static ShopService CreateService(MemoryStorage storage, int coins, out SaveRepository repository)
        {
            var catalog = ShopCatalog.FromJson(ConfigJson);
            repository = new SaveRepository(storage);
            var data = repository.Load(coins);
            var repo = repository;
            return new ShopService(catalog, data, repo.Save);
        }

        [Test]
        public void Purchase_WithoutEnoughCoins_FailsAndChangesNothing()
        {
            var service = CreateService(new MemoryStorage(), 99, out _);

            Assert.AreEqual(PurchaseResult.NotEnoughCoins, service.Purchase("ball_red"));
            Assert.AreEqual(99, service.Coins);
            Assert.IsFalse(service.IsOwned("ball_red"));
        }

        [Test]
        public void Purchase_Theme_UnlocksButDoesNotEquip()
        {
            var service = CreateService(new MemoryStorage(), 150, out _);

            Assert.AreEqual(PurchaseResult.Success, service.Purchase("ball_red"));

            Assert.AreEqual(50, service.Coins);
            Assert.IsTrue(service.IsOwned("ball_red"));
            Assert.IsFalse(service.IsEquipped("ball_red"));
            Assert.AreEqual("ball_default", service.GetEquippedId(ItemKinds.PlayerTheme));
            Assert.AreEqual(PurchaseResult.AlreadyOwned, service.Purchase("ball_red"));
        }

        [Test]
        public void Equip_RequiresOwnership_AndKeepsOneItemPerSlot()
        {
            var service = CreateService(new MemoryStorage(), 500, out _);

            Assert.IsFalse(service.Equip("ball_red"));

            service.Purchase("ball_red");
            service.Purchase("ball_blue");
            Assert.IsTrue(service.Equip("ball_red"));
            Assert.IsTrue(service.Equip("ball_blue"));

            Assert.IsFalse(service.IsEquipped("ball_red"));
            Assert.IsTrue(service.IsEquipped("ball_blue"));
        }

        [Test]
        public void PowerUps_StackUpToMax_AndAreConsumedOneAtATime()
        {
            var service = CreateService(new MemoryStorage(), 500, out _);

            Assert.IsFalse(service.TryConsumePowerUp("shield"));

            service.Purchase("shield");
            service.Purchase("shield");
            Assert.AreEqual(PurchaseResult.MaxOwnedReached, service.Purchase("shield"));
            Assert.AreEqual(2, service.GetPowerUpCount("shield"));

            Assert.IsTrue(service.TryConsumePowerUp("shield"));
            Assert.AreEqual(1, service.GetPowerUpCount("shield"));
        }

        [Test]
        public void State_SurvivesRestart()
        {
            var storage = new MemoryStorage();
            var first = CreateService(storage, 400, out _);
            first.Purchase("ball_red");
            first.Equip("ball_red");
            first.Purchase("shield");
            first.AddCoins(7);

            var second = CreateService(storage, 0, out _);

            Assert.AreEqual(400 - 100 - 50 + 7, second.Coins);
            Assert.IsTrue(second.IsEquipped("ball_red"));
            Assert.AreEqual(1, second.GetPowerUpCount("shield"));
        }

        [Test]
        public void Load_CorruptLatestSave_FallsBackToPreviousCopy()
        {
            var storage = new MemoryStorage();
            var service = CreateService(storage, 300, out _);
            service.Purchase("ball_red");
            storage.Files.Add("{ this is not json");

            LogAssert.Expect(UnityEngine.LogType.Warning, new System.Text.RegularExpressions.Regex("unreadable save"));
            var restored = CreateService(storage, 0, out _);

            Assert.IsTrue(restored.IsOwned("ball_red"));
        }

        [Test]
        public void CoinsForScore_UsesEconomyConfig()
        {
            var service = CreateService(new MemoryStorage(), 0, out _);

            Assert.AreEqual(0, service.CoinsForScore(0));
            Assert.AreEqual(2, service.CoinsForScore(1));
            Assert.AreEqual(40, service.CoinsForScore(20));
        }

        [Test]
        public void Catalog_SkipsInvalidAndDuplicateItems()
        {
            LogAssert.ignoreFailingMessages = true;
            var catalog = ShopCatalog.FromJson(@"{
                ""sections"": [ { ""id"": ""a"" } ],
                ""items"": [
                    { ""id"": ""x"", ""section"": ""a"", ""kind"": ""powerup"" },
                    { ""id"": ""x"", ""section"": ""a"", ""kind"": ""powerup"" },
                    { ""id"": """", ""section"": ""a"", ""kind"": ""powerup"" },
                    { ""id"": ""y"", ""section"": ""missing"", ""kind"": ""powerup"" }
                ]
            }");
            LogAssert.ignoreFailingMessages = false;

            Assert.AreEqual(1, catalog.Sections.Count);
            Assert.AreEqual(1, catalog.Sections[0].Items.Count);
            Assert.IsTrue(catalog.TryGetItem("y", out _));
        }
    }
}
