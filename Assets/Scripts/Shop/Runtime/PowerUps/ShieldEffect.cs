using System.Collections.Generic;
using DG.Tweening;
using Enums;
using Shop.Model;
using UnityEngine;

namespace Shop.Runtime
{
    /// <summary>
    /// Absorbs the next obstacle hit. While active, obstacle colliders are switched off and the overlap with
    /// the ball is tested here instead; the first touched obstacle is removed and the shield is spent.
    /// </summary>
    public sealed class ShieldEffect : IPowerUpEffect
    {
        private readonly PowerUpContext _context;
        private readonly List<ThreatMarker> _threats = new List<ThreatMarker>();
        private GameObject _visual;
        private SpriteRenderer _visualRenderer;
        private bool _active;
        private float _time;

        public bool IsActive => _active;
        public float RemainingSeconds => -1f;

        public ShieldEffect(PowerUpContext context, ShopItemConfig item)
        {
            _context = context;
        }

        public bool CanActivate(out PowerUpUseResult blockedBy)
        {
            blockedBy = _active ? PowerUpUseResult.AlreadyActive : PowerUpUseResult.Used;
            return !_active && _context.Ball != null;
        }

        public void Activate()
        {
            _active = true;
            _time = 0f;
            CreateVisual();
        }

        public void Tick(float deltaTime)
        {
            if (!_active || _visual == null)
                return;

            _time += deltaTime;
            var pulse = 1f + 0.07f * Mathf.Sin(_time * 6f);
            _visual.transform.localScale = Vector3.one * (BaseVisualScale * pulse);
        }

        public void LateTick()
        {
            var ball = _context.Ball;
            if (!_active || ball == null)
                return;

            Threats.Collect(_context.Scene, _threats);

            var position = (Vector2)ball.transform.position;
            var radius = BallRadius(ball);
            foreach (var threat in _threats)
            {
                if (threat.Vanished)
                    continue;

                Threats.Suppress(threat);
                if (Threats.Touches(threat, position, radius))
                {
                    Absorb(threat);
                    return;
                }
            }
        }

        public void Cancel()
        {
            if (_active)
                ReleaseSuppressed();

            _active = false;
            DestroyVisual();
        }

        private void Absorb(ThreatMarker hit)
        {
            Threats.Vanish(hit);
            _active = false;
            ReleaseSuppressed();
            Sfx.Play(SoundType.Score);

            if (_visual == null)
                return;

            var visual = _visual;
            _visual = null;
            visual.transform.DOScale(BaseVisualScale * 2.2f, 0.3f).SetEase(Ease.OutQuad);
            _visualRenderer.DOFade(0f, 0.3f).OnComplete(() => Object.Destroy(visual));
        }

        private void ReleaseSuppressed()
        {
            Threats.Collect(_context.Scene, _threats);
            foreach (var threat in _threats)
                Threats.Release(threat);
        }

        private static float BallRadius(GameCore.BallController ball)
        {
            var circle = ball.GetComponent<CircleCollider2D>();
            var scale = Mathf.Abs(ball.transform.lossyScale.x);
            return circle != null ? circle.radius * scale : 0.26f * scale;
        }

        private const float BaseVisualScale = 0.34f;

        private void CreateVisual()
        {
            DestroyVisual();
            var art = _context.Art;
            var ball = _context.Ball;
            if (art == null || art.ringSprite == null || ball == null)
                return;

            _visual = new GameObject("ShieldVisual");
            _visual.transform.SetParent(ball.transform, false);
            _visual.transform.localScale = Vector3.one * BaseVisualScale;

            _visualRenderer = _visual.AddComponent<SpriteRenderer>();
            _visualRenderer.sprite = art.ringSprite;
            _visualRenderer.color = new Color(0.3725f, 0.7922f, 0.9961f, 1f);

            var ballRenderer = ball.GetComponent<SpriteRenderer>();
            if (ballRenderer != null)
            {
                _visualRenderer.sortingLayerID = ballRenderer.sortingLayerID;
                _visualRenderer.sortingOrder = ballRenderer.sortingOrder + 1;
            }
        }

        private void DestroyVisual()
        {
            if (_visual != null)
                Object.Destroy(_visual);
            _visual = null;
        }
    }
}
