using System;
using System.Collections.Generic;
using GameCore;
using Shop.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shop.Runtime
{
    public enum PowerUpUseResult
    {
        Used,
        NoActiveRun,
        NoStock,
        AlreadyActive,
        NothingToAffect,
        Unavailable
    }

    /// <summary>What an effect gets to work with while a run is in progress.</summary>
    public sealed class PowerUpContext
    {
        public BallController Ball;
        public Scene Scene;
        public ShopResources Art;
    }

    /// <summary>
    /// Behaviour of one power-up during a run. Instances live for the duration of a game session and are driven
    /// by <see cref="GameSession"/>; consuming stock and run-state checks are done there, not in the effect.
    /// </summary>
    public interface IPowerUpEffect
    {
        bool IsActive { get; }
        /// <summary>Seconds left for timed effects, negative for effects without a timer.</summary>
        float RemainingSeconds { get; }
        bool CanActivate(out PowerUpUseResult blockedBy);
        void Activate();
        void Tick(float deltaTime);
        void LateTick();
        /// <summary>Ends the effect and releases everything it changed (run over, scene closing).</summary>
        void Cancel();
    }

    /// <summary>
    /// Maps the "effect" id of a power-up in the JSON config to its implementation. Adding a power-up with new
    /// behaviour = write an <see cref="IPowerUpEffect"/> and register it here; its price/name/icon stay in JSON.
    /// </summary>
    public static class PowerUpEffectRegistry
    {
        private static readonly Dictionary<string, Func<PowerUpContext, ShopItemConfig, IPowerUpEffect>> Factories =
            new Dictionary<string, Func<PowerUpContext, ShopItemConfig, IPowerUpEffect>>
            {
                { "shield", (context, item) => new ShieldEffect(context, item) },
                { "doubleScore", (context, item) => new DoubleScoreEffect(context, item) },
                { "clearThreats", (context, item) => new ClearThreatsEffect(context) }
            };

        public static void Register(string effectId, Func<PowerUpContext, ShopItemConfig, IPowerUpEffect> factory)
        {
            Factories[effectId] = factory;
        }

        public static IPowerUpEffect Create(string effectId, PowerUpContext context, ShopItemConfig item)
        {
            if (!string.IsNullOrEmpty(effectId) && Factories.TryGetValue(effectId, out var factory))
                return factory(context, item);

            Debug.LogWarning($"[Shop] No power-up effect registered for '{effectId}' (item '{item.id}').");
            return null;
        }
    }
}
