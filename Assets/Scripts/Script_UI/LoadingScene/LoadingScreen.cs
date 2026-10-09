using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;
using System;
using System.Collections;
using ScriptableObjects.GameEvents;


namespace UI
{
    public class LoadingScreen : MonoBehaviour
    {
        [SerializeField] private NullEvent OnEndAction;
        [Header("Main Root")]
        [SerializeField] private CanvasGroup rootGroup;

        [Header("Animated Parts")]
        [SerializeField] private RectTransform topPart;
        [SerializeField] private RectTransform bottomPart;

        [Header("Loading UI")]
        [SerializeField] private Slider fillBar;
        [SerializeField] private TMP_Text progressText;

        [Header("Animation Settings")]
        [SerializeField] private float introDuration = 0.5f;
        [SerializeField] private float fillDuration = 1f;
        [SerializeField] private float outroDuration = 0.5f;
        [SerializeField] private float moveOffset = 150f;

        private Vector2 topStartPos;
        private Vector2 bottomStartPos;

        private void Start()
        {
            ShowLoadingBar(fillDuration, () =>
            {
                OnEndAction?.Raise();
            });
        }


        public void ShowLoadingBar(float fillTime, Action onFinished)
        {
            StartCoroutine(LoadingRoutine(fillTime, onFinished));
        }


        private IEnumerator LoadingRoutine(float fillTime, Action onFinished)
        {
            SetupInitialState();

            DoIntro();
            yield return new WaitForSeconds(introDuration);

            yield return FillBarOverTime(fillTime);

            DoOutro();
            yield return new WaitForSeconds(outroDuration);

            onFinished?.Invoke();
        }


        private void SetupInitialState()
        {
            fillBar.value = 0f;
            progressText.text = "0%";

            rootGroup.alpha = 0f;
            
            topStartPos = topPart.anchoredPosition;
            bottomStartPos = bottomPart.anchoredPosition;
            
            topPart.anchoredPosition += new Vector2(0, moveOffset);
            bottomPart.anchoredPosition -= new Vector2(0, moveOffset);
        }


        private void DoIntro()
        {
            rootGroup.DOFade(1f, introDuration);

            topPart.DOAnchorPos(topStartPos, introDuration)
                   .SetEase(Ease.OutCubic);

            bottomPart.DOAnchorPos(bottomStartPos, introDuration)
                      .SetEase(Ease.OutCubic);
        }


        private void DoOutro()
        {
            rootGroup.DOFade(0f, outroDuration);

            topPart.DOAnchorPos(topStartPos + new Vector2(0, moveOffset), outroDuration)
                   .SetEase(Ease.InCubic);

            bottomPart.DOAnchorPos(bottomStartPos - new Vector2(0, moveOffset), outroDuration)
                      .SetEase(Ease.InCubic);
        }


        private IEnumerator FillBarOverTime(float duration)
        {
            float t = 0f;

            while (t < duration)
            {
                t += Time.deltaTime;
                float progress = Mathf.Clamp01(t / duration);

                fillBar.value = progress;
                progressText.text = Mathf.RoundToInt(progress * 100f) + "%";

                yield return null;
            }

            fillBar.value = 1f;
            progressText.text = "100%";
        }
    }
}
