using System.Collections.Generic;
using UnityEngine;

namespace Shop.Runtime
{
    /// <summary>
    /// Instantly removes every obstacle on the ring. The game's own spawner then brings new ones back gradually
    /// as stars are collected, so the board refills instead of staying empty.
    /// </summary>
    public sealed class ClearThreatsEffect : IPowerUpEffect
    {
        private readonly PowerUpContext _context;
        private readonly List<ThreatMarker> _threats = new List<ThreatMarker>();

        public bool IsActive => false;
        public float RemainingSeconds => -1f;

        public ClearThreatsEffect(PowerUpContext context)
        {
            _context = context;
        }

        public bool CanActivate(out PowerUpUseResult blockedBy)
        {
            Threats.Collect(_context.Scene, _threats);
            foreach (var threat in _threats)
            {
                if (!threat.Vanished)
                {
                    blockedBy = PowerUpUseResult.Used;
                    return true;
                }
            }

            blockedBy = PowerUpUseResult.NothingToAffect;
            return false;
        }

        public void Activate()
        {
            Threats.Collect(_context.Scene, _threats);
            foreach (var threat in _threats)
                Threats.Vanish(threat);
            Sfx.Play(Enums.SoundType.Explosion);
        }

        public void Tick(float deltaTime)
        {
        }

        public void LateTick()
        {
        }

        public void Cancel()
        {
        }
    }
}
