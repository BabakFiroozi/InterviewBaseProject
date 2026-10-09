using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shop.Runtime
{
    public static class Threats
    {
        private const string ObstacleTag = "Obsticle"; // sic: the tag name used by the game
        private static readonly List<GameObject> RootBuffer = new List<GameObject>();

        /// <summary>Finds the live obstacles. They are instantiated into the active scene, so both it and the game scene are scanned.</summary>
        public static void Collect(Scene gameScene, List<ThreatMarker> result)
        {
            result.Clear();
            CollectFrom(gameScene, result);
            var active = SceneManager.GetActiveScene();
            if (active != gameScene)
                CollectFrom(active, result);
        }

        private static void CollectFrom(Scene scene, List<ThreatMarker> result)
        {
            if (!scene.isLoaded)
                return;

            scene.GetRootGameObjects(RootBuffer);
            foreach (var root in RootBuffer)
            {
                if (root == null || !root.CompareTag(ObstacleTag))
                    continue;

                var marker = root.GetComponent<ThreatMarker>();
                if (marker == null)
                {
                    marker = root.AddComponent<ThreatMarker>();
                    marker.Hitbox = root.GetComponent<Collider2D>();
                }

                result.Add(marker);
            }
        }

        public static void Vanish(ThreatMarker marker)
        {
            if (marker == null || marker.Vanished)
                return;

            marker.Vanished = true;
            marker.SuppressedByShield = false;
            if (marker.Hitbox != null)
                marker.Hitbox.enabled = false;

            marker.transform.DOScale(Vector3.zero, 0.25f).SetEase(Ease.InBack);
        }

        public static void Suppress(ThreatMarker marker)
        {
            if (marker == null || marker.Vanished || marker.Hitbox == null)
                return;

            marker.Hitbox.enabled = false;
            marker.SuppressedByShield = true;
        }

        public static void Release(ThreatMarker marker)
        {
            if (marker == null || !marker.SuppressedByShield)
                return;

            marker.SuppressedByShield = false;
            if (!marker.Vanished && marker.Hitbox != null)
                marker.Hitbox.enabled = true;
        }

        /// <summary>Circle (ball) vs. box (obstacle) overlap test, used while the obstacle's own collider is switched off.</summary>
        public static bool Touches(ThreatMarker marker, Vector2 circleCenter, float circleRadius)
        {
            if (marker == null || !(marker.Hitbox is BoxCollider2D box))
                return false;

            var local = (Vector2)marker.transform.InverseTransformPoint(circleCenter) - box.offset;
            var half = box.size * 0.5f;
            var dx = Mathf.Max(Mathf.Abs(local.x) - half.x, 0f);
            var dy = Mathf.Max(Mathf.Abs(local.y) - half.y, 0f);
            var distance = new Vector2(dx, dy).magnitude * Mathf.Abs(marker.transform.lossyScale.x);
            return distance <= circleRadius;
        }
    }
}
