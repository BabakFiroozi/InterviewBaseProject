using UnityEngine;

namespace Shop.Runtime
{
    /// <summary>
    /// Bookkeeping attached to an obstacle the first time a power-up touches it. Obstacles are owned by the
    /// game's ObstacleManager (it keeps references to them and destroys them itself), so power-ups never destroy
    /// them: they only switch their collider off and shrink them, and the manager retires them later as usual.
    /// </summary>
    public sealed class ThreatMarker : MonoBehaviour
    {
        public Collider2D Hitbox;
        /// <summary>Removed by a power-up: harmless for the rest of its life.</summary>
        public bool Vanished;
        /// <summary>Collider temporarily off because a shield is up.</summary>
        public bool SuppressedByShield;
    }
}
