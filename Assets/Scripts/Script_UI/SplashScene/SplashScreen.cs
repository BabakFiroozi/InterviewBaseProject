using UnityEngine;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace UI
{
    public class SplashScreen : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private float fadeDuration = 1f;
        [SerializeField] private float splashDuration = 2f;

        private void Start()
        {
            Application.targetFrameRate = 60;
            PlaySplashSequence();
        }

        private void PlaySplashSequence()
        {
            canvasGroup.alpha = 0f;
            
            Sequence seq = DOTween.Sequence();

            seq.Append(canvasGroup.DOFade(1f, fadeDuration))      
                .AppendInterval(splashDuration)                   
                .Append(canvasGroup.DOFade(0f, fadeDuration))     
                .OnComplete(() =>
                {
                    SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
                });
        }
    }
}