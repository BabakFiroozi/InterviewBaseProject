using System.Collections.Generic;
using UnityEngine;

namespace Shop.Views
{
    public interface ITapBlocker
    {
        bool Blocks(Vector2 screenPoint);
    }

    /// <summary>
    /// Lets HUD controls claim a screen position. The game reads raw mouse/touch input to flip the ball, so a tap on
    /// a power-up button would otherwise also move the ball; BallController asks this gate before reacting.
    /// </summary>
    public static class HudTapGate
    {
        private static readonly List<ITapBlocker> Blockers = new List<ITapBlocker>();

        public static void Register(ITapBlocker blocker)
        {
            if (!Blockers.Contains(blocker))
                Blockers.Add(blocker);
        }

        public static void Unregister(ITapBlocker blocker)
        {
            Blockers.Remove(blocker);
        }

        public static bool IsBlocked(Vector2 screenPoint)
        {
            for (var i = Blockers.Count - 1; i >= 0; i--)
            {
                if (Blockers[i].Blocks(screenPoint))
                    return true;
            }

            return false;
        }
    }
}
