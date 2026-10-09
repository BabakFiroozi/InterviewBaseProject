using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using Enums;
using ScriptableObjects.GameEvents;

namespace AudioManger
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        
        [Header("Events")]
        [SerializeField] private NullEvent playButtonSoundEvent;
        [SerializeField] private NullEvent playUiMoveSound;

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        [Header("Clips")]
        [SerializeField] private List<SoundClip> soundClips;

        private Dictionary<SoundType, AudioClip> clipMap;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            clipMap = new Dictionary<SoundType, AudioClip>();
    
            foreach (var s in soundClips)
            {
                clipMap[s.type] = s.clip;
                
                if (s.clip != null && !s.clip.preloadAudioData)
                {
                    s.clip.LoadAudioData();
                }
            }
        }


        private void OnEnable()
        {

            playButtonSoundEvent.OnEventRaised += PlayButtonSound;
            playUiMoveSound.OnEventRaised += PlayMoveUISound;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {

            playButtonSoundEvent.OnEventRaised -= PlayButtonSound;
            playUiMoveSound.OnEventRaised -= PlayMoveUISound;
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void PlayButtonSound()
        {
            PlaySFX(SoundType.ButtonClick);
        }

        private void PlayMoveUISound()
        {
            PlaySFX(SoundType.UIMove);
        }

        public void PlaySFX(SoundType type)
        {
            if (!clipMap.ContainsKey(type)) return;
            sfxSource.PlayOneShot(clipMap[type]);
        }


        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                case "Scene_Game":
                    FadeMusicTo(SoundType.GameSceneMusic, 1f);
                    break;

                default:
                    FadeMusicTo(SoundType.MenuSceneMusic, 1f);
                    break;
            }
        }
        
        private void FadeMusicTo(SoundType type, float duration = 1f)
        {
            if (!clipMap.ContainsKey(type)) return;

            if (musicSource.isPlaying && musicSource.clip == clipMap[type])
                return;

            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
                musicSource.volume = 1f; 
            }

            fadeRoutine = StartCoroutine(FadeRoutine(type, duration));
        }


        private IEnumerator FadeRoutine(SoundType type, float duration)
        {
            float initialVolume = musicSource.volume;
            AudioClip nextClip = clipMap[type];
            
            float t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                musicSource.volume = initialVolume * (1f - t / duration);
                yield return null;
            }

            musicSource.clip = nextClip;
            musicSource.Play();
            
            t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                musicSource.volume = (t / duration) * initialVolume;
                yield return null;
            }

            fadeRoutine = null;
        }
    }

    [Serializable]
    public class SoundClip
    {
        public SoundType type;
        public AudioClip clip;
    }
}
