using GameCore;
using Shop.Core;
using Shop.Model;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shop.Runtime
{
    /// <summary>
    /// Applies the equipped themes to a freshly loaded game scene: the player theme tints the ball (and its
    /// particles), the world theme tints the ring. Pure tinting of existing sprites, so no gameplay code is touched.
    /// </summary>
    public static class ThemeApplier
    {
        private const string RingSpriteName = "ring_0";

        public static void Apply(Scene scene, ShopService service)
        {
            var playerTheme = service.GetEquippedItem(ItemKinds.PlayerTheme);
            var worldTheme = service.GetEquippedItem(ItemKinds.WorldTheme);

            foreach (var root in scene.GetRootGameObjects())
            {
                if (playerTheme != null)
                {
                    foreach (var ball in root.GetComponentsInChildren<BallController>(true))
                        TintBall(ball, playerTheme.GetColor(Color.white));
                }

                if (worldTheme != null)
                {
                    foreach (var renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
                    {
                        if (renderer.sprite != null && renderer.sprite.name == RingSpriteName)
                        {
                            var color = worldTheme.GetColor(renderer.color);
                            color.a = renderer.color.a;
                            renderer.color = color;
                        }
                    }
                }
            }
        }

        private static void TintBall(BallController ball, Color color)
        {
            var spriteRenderer = ball.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                color.a = spriteRenderer.color.a;
                spriteRenderer.color = color;
            }

            foreach (var particles in ball.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = particles.main;
                main.startColor = new ParticleSystem.MinMaxGradient(color);
            }
        }
    }
}
