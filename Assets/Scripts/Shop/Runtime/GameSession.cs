using System.Collections.Generic;
using GameCore;
using Shop.Core;
using Shop.Model;
using Shop.Views;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shop.Runtime
{
    /// <summary>
    /// Shop logic for one loaded game scene. It listens to the game's existing events (run started, score changed,
    /// run ended) to pay out coins and to know whether a run is active, and it executes power-ups on request of
    /// the HUD. It never raises game events itself.
    /// </summary>
    public sealed class GameSession : MonoBehaviour
    {
        private ShopSystem _shop;
        private ShopResources _art;
        private PowerUpContext _context;
        private PowerUpHud _hud;
        private readonly Dictionary<string, IPowerUpEffect> _effects = new Dictionary<string, IPowerUpEffect>();

        private bool _runActive;
        private int _lastScore;

        public bool RunActive => _runActive;
        public ShopService Service => _shop.Service;

        public static GameSession Create(Scene scene, ShopSystem shop)
        {
            var host = new GameObject("ShopGameSession");
            // New objects land in the active scene (the menu); move it so it is destroyed together with the game scene.
            SceneManager.MoveGameObjectToScene(host, scene);
            var session = host.AddComponent<GameSession>();
            session.Initialize(scene, shop);
            return session;
        }

        private void Initialize(Scene scene, ShopSystem shop)
        {
            _shop = shop;
            _art = shop.Art;
            _context = new PowerUpContext { Scene = scene, Art = _art, Ball = FindBall(scene) };

            var canvas = FindCanvas(scene);
            if (canvas != null)
                _hud = PowerUpHud.Create(canvas, this);

            AttachCoinIconToScore(scene);

            if (_art != null)
            {
                if (_art.onBallStartMoving != null)
                    _art.onBallStartMoving.OnEventRaised += OnRunStarted;
                if (_art.onGameEnded != null)
                    _art.onGameEnded.OnEventRaised += OnRunEnded;
                if (_art.onScoreChanged != null)
                    _art.onScoreChanged.OnEventRaised += OnScoreChanged;
            }
        }

        /// <summary>
        /// Puts a coin icon beside the score text of the game-over screen. The in-game HUD has a text with the same
        /// name ("PlayerScore"), so the right one is recognised by sitting next to the game-over "RetryButton".
        /// </summary>
        private static void AttachCoinIconToScore(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                foreach (var text in root.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    if (text.gameObject.name == "PlayerScore" && text.transform.parent != null
                        && text.transform.parent.Find("RetryButton") != null)
                    {
                        ScoreCoinIcon.Attach(text);
                        return;
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (_art != null)
            {
                if (_art.onBallStartMoving != null)
                    _art.onBallStartMoving.OnEventRaised -= OnRunStarted;
                if (_art.onGameEnded != null)
                    _art.onGameEnded.OnEventRaised -= OnRunEnded;
                if (_art.onScoreChanged != null)
                    _art.onScoreChanged.OnEventRaised -= OnScoreChanged;
            }

            CancelAllEffects();
        }

        private void Update()
        {
            if (!_runActive)
                return;

            var deltaTime = Time.deltaTime;
            foreach (var effect in _effects.Values)
                effect.Tick(deltaTime);
        }

        private void LateUpdate()
        {
            if (!_runActive)
                return;

            foreach (var effect in _effects.Values)
                effect.LateTick();
        }

        // ---- game events ---------------------------------------------------------------------------

        private void OnRunStarted()
        {
            CancelAllEffects();
            _runActive = true;
            _lastScore = 0;
            if (_hud != null)
                _hud.SetRunActive(true);
        }

        private void OnScoreChanged(int score)
        {
            _lastScore = score;
        }

        private void OnRunEnded()
        {
            // The game can raise "ended" more than once for a single death; pay out only once.
            if (!_runActive)
                return;

            _runActive = false;
            CancelAllEffects();

            var coins = _shop.Service.CoinsForScore(_lastScore);
            if (coins > 0)
                _shop.Service.AddCoins(coins);

            if (_hud != null)
                _hud.SetRunActive(false);
        }

        // ---- power-ups -----------------------------------------------------------------------------

        public IPowerUpEffect GetEffect(string itemId)
        {
            _effects.TryGetValue(itemId, out var effect);
            return effect;
        }

        public PowerUpUseResult TryUsePowerUp(string itemId)
        {
            if (!_runActive)
                return PowerUpUseResult.NoActiveRun;

            var service = _shop.Service;
            if (!service.Catalog.TryGetItem(itemId, out var item) || !item.IsPowerUp || !item.enabled)
                return PowerUpUseResult.Unavailable;

            var effect = GetOrCreateEffect(item);
            if (effect == null)
                return PowerUpUseResult.Unavailable;

            if (service.GetPowerUpCount(itemId) <= 0)
                return PowerUpUseResult.NoStock;

            if (!effect.CanActivate(out var blockedBy))
                return blockedBy;

            // Spend the power-up first (this also writes the save), then trigger it.
            if (!service.TryConsumePowerUp(itemId))
                return PowerUpUseResult.NoStock;

            effect.Activate();
            return PowerUpUseResult.Used;
        }

        private IPowerUpEffect GetOrCreateEffect(ShopItemConfig item)
        {
            if (_effects.TryGetValue(item.id, out var existing))
                return existing;

            var created = PowerUpEffectRegistry.Create(item.effect, _context, item);
            if (created != null)
                _effects.Add(item.id, created);
            return created;
        }

        private void CancelAllEffects()
        {
            foreach (var effect in _effects.Values)
                effect.Cancel();
        }

        // ---- scene lookup --------------------------------------------------------------------------

        private static BallController FindBall(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var ball = root.GetComponentInChildren<BallController>(true);
                if (ball != null)
                    return ball;
            }

            return null;
        }

        private static Canvas FindCanvas(Scene scene)
        {
            foreach (var root in scene.GetRootGameObjects())
            {
                var canvas = root.GetComponent<Canvas>();
                if (canvas != null)
                    return canvas;
            }

            return null;
        }
    }
}
