using Shop.Model;
using UnityEngine;

namespace Shop.Runtime
{
    /// <summary>Stars are worth twice as much for <c>duration</c> seconds (BallController.ScoreMultiplier).</summary>
    public sealed class DoubleScoreEffect : IPowerUpEffect
    {
        private const float FallbackDuration = 10f;

        private readonly PowerUpContext _context;
        private readonly float _duration;
        private float _remaining;
        private bool _active;

        public bool IsActive => _active;
        public float RemainingSeconds => _active ? _remaining : -1f;

        public DoubleScoreEffect(PowerUpContext context, ShopItemConfig item)
        {
            _context = context;
            _duration = item.duration > 0f ? item.duration : FallbackDuration;
        }

        public bool CanActivate(out PowerUpUseResult blockedBy)
        {
            blockedBy = _active ? PowerUpUseResult.AlreadyActive : PowerUpUseResult.Used;
            return !_active && _context.Ball != null;
        }

        public void Activate()
        {
            _active = true;
            _remaining = _duration;
            _context.Ball.ScoreMultiplier = 2;
        }

        public void Tick(float deltaTime)
        {
            if (!_active)
                return;

            _remaining -= deltaTime;
            if (_remaining <= 0f)
                End();
        }

        public void LateTick()
        {
        }

        public void Cancel()
        {
            if (_active)
                End();
        }

        private void End()
        {
            _active = false;
            _remaining = 0f;
            if (_context.Ball != null)
                _context.Ball.ScoreMultiplier = 1;
        }
    }
}
