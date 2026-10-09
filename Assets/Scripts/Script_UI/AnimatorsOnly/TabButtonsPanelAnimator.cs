using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using ScriptableObjects.GameEvents;

namespace UI
{
    public class TabButtonsPanelAnimator : MonoBehaviour
    {
        [SerializeField] private NullEvent OnStartGameEvent;
        [Header("Refs")]
        [SerializeField] private RectTransform targetPanel;

        [Header("Settings")]
        [SerializeField] private float moveDuration = 0.4f;

        [Header("Animation Offset")]
        [SerializeField] private float moveOffsetY = -150f;

        private Vector2 originalPos;
        private Sequence panelSeq;

        private void Awake()
        {
            originalPos = targetPanel.anchoredPosition;
        }

        private void OnEnable()
        {
            OnStartGameEvent.OnEventRaised += ReverseAnimation;
        }
        private void OnDisable()
        {
            OnStartGameEvent.OnEventRaised -= ReverseAnimation;
        }

        private void Start()
        {
            AnimateForward();
        }

        private void AnimateForward()
        {
            panelSeq = DOTween.Sequence().SetAutoKill(false);
            
            targetPanel.anchoredPosition = originalPos + new Vector2(0, moveOffsetY);
            
            panelSeq.Append(
                targetPanel.DOAnchorPos(originalPos, moveDuration)
                    .SetEase(Ease.OutCubic)
            );
        }

        [Button] 
        public void ReverseAnimation()
        {
            if (panelSeq != null && panelSeq.IsActive())
                panelSeq.PlayBackwards();
        }
        
    }
}