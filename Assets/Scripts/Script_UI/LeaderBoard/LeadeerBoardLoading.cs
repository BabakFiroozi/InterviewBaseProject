using ScriptableObjects.GameEvents;
using UnityEngine;
using DG.Tweening;

namespace UI
{
    public class LeadeerBoardLoading : MonoBehaviour
    {
        public NullEvent OnLeaderBoardTabClicked;
        public NullEvent OnLeaderBoardDataLoaded;
        [SerializeField] private Transform icon;
        [SerializeField]private CanvasGroup canvasGroup;

        private void Awake()
        {
            icon.gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            OnLeaderBoardTabClicked.OnEventRaised += ShowLoading;
            OnLeaderBoardDataLoaded.OnEventRaised += HideLoading;
        }

        private void OnDisable()
        {
            OnLeaderBoardTabClicked.OnEventRaised -= ShowLoading;
            OnLeaderBoardDataLoaded.OnEventRaised -= HideLoading;
        }

        public void ShowLoading()
        {
            icon.gameObject.SetActive(true);
            icon.DOKill(true);


            canvasGroup.DOFade(1f, 0.05f).SetEase(Ease.OutCubic);
            icon.localScale = Vector3.one * 0.8f;
            icon.DOScale(1f, 0.3f).SetEase(Ease.OutBack);

            
            icon.DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear);

        }

        public void HideLoading()
        {
            icon.DOKill();
            
            canvasGroup.DOFade(0f, 0.25f)
                .SetEase(Ease.InCubic)
                .OnComplete(() =>
                {
                    icon.gameObject.SetActive(false);
                });
        }
    }
}