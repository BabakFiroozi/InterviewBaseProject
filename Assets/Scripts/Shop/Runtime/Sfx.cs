using AudioManger;
using Enums;

namespace Shop.Runtime
{
    /// <summary>Null-safe access to the existing AudioManager (it is absent when a scene is run on its own).</summary>
    public static class Sfx
    {
        public static void Play(SoundType type)
        {
            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySFX(type);
        }
    }
}
