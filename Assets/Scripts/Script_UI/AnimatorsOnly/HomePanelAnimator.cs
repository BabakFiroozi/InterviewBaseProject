using UnityEngine;
using DG.Tweening;
using NaughtyAttributes;
using ScriptableObjects.GameEvents;
using UnityEngine.UI;

namespace UI
{
    public class HomePanelAnimator : MonoBehaviour
    {
        [SerializeField] private Image backgroundImage;
        [SerializeField] private NullEvent OnSceneMenuAnimationEnd;
        [SerializeField] private NullEvent OnStartGame;
        [Header("Refs")]
        [SerializeField] private RectTransform playerName;
        [SerializeField] private RectTransform gameName;
        [SerializeField] private RectTransform startButton;

        [Header("Settings")]
        [SerializeField] private float moveDistance = 200f;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private float flipRotation = 90f;
        
        private Vector2 playerNameOriginal;
        private Vector2 gameNameOriginal;
        private Vector2 startButtonOriginal;
        private Quaternion gameNameRotationOriginal;
        

        private void Awake()
        {
            playerNameOriginal = playerName.anchoredPosition;
            gameNameOriginal = gameName.anchoredPosition;
            startButtonOriginal = startButton.anchoredPosition;
            gameNameRotationOriginal = gameName.localRotation;
        }

        private void OnEnable()
        {
            OnStartGame.OnEventRaised += PlayExitAnimation;
            
        }
        private void OnDisable()
        {
            OnStartGame.OnEventRaised -= PlayExitAnimation;
        }

        private void Start()
        {
            AnimatePlayerName();
            AnimateGameName();
            AnimateStartButton();
        }

        private void AnimatePlayerName()
        {
            var playerNameSeq = DOTween.Sequence();
            
            playerName.anchoredPosition = playerNameOriginal + new Vector2(-moveDistance, 0);
            
            playerNameSeq.Append(
                playerName.DOAnchorPos(playerNameOriginal, moveDuration)
                    .SetEase(Ease.OutCubic)
            );
        }

        private void AnimateGameName()
        {
            var gameNameSeq = DOTween.Sequence();
            
            gameName.anchoredPosition = gameNameOriginal + new Vector2(moveDistance, 0);
            gameName.localRotation = Quaternion.Euler(0, 0, flipRotation);
            
            gameNameSeq.Append(
                gameName.DOAnchorPos(gameNameOriginal + new Vector2(-40f, 0), moveDuration * 0.6f)
                    .SetEase(Ease.OutBack)
            );
            
            gameNameSeq.Join(
                gameName.DOLocalRotate(new Vector3(0, 0, -flipRotation), moveDuration * 0.6f)
            );
            
            gameNameSeq.Append(
                gameName.DOAnchorPos(gameNameOriginal, moveDuration * 0.4f)
                    .SetEase(Ease.OutBack)
            );
            gameNameSeq.Join(
                gameName.DOLocalRotate(gameNameRotationOriginal.eulerAngles, moveDuration * 0.4f)
            );
        }

        private void AnimateStartButton()
        {
            var startButtonSeq = DOTween.Sequence();

            startButton.anchoredPosition = startButtonOriginal + new Vector2(0, -moveDistance);

            startButtonSeq.Append(
                startButton.DOAnchorPos(startButtonOriginal, moveDuration)
                    .SetEase(Ease.OutBack)
            );
        }


        private Sequence exitSequence;

        [Button]
        public void PlayExitAnimation()
        {
            backgroundImage.enabled = false;

            if (exitSequence != null && exitSequence.IsActive())
                exitSequence.Kill();

            exitSequence = DOTween.Sequence();


            exitSequence.Join(
                playerName.DOAnchorPos(playerNameOriginal + new Vector2(-moveDistance, 0), moveDuration)
                    .SetEase(Ease.InCubic)
            );

            


            exitSequence.Join(
                gameName.DOAnchorPos(gameNameOriginal + new Vector2(0, moveDistance), moveDuration)
                    .SetEase(Ease.InBack)
            );

            exitSequence.Join(
                gameName.DOLocalRotate(new Vector3(0, 0, 180f), moveDuration)
                    .SetEase(Ease.InOutQuad)
            );


            exitSequence.Join(
                startButton.DOAnchorPos(startButtonOriginal + new Vector2(0, -moveDistance), moveDuration)
                    .SetEase(Ease.InBack)
            );

            exitSequence.Join(
                startButton.DOScale(0f, moveDuration)
                    .SetEase(Ease.InBack)
            );


            exitSequence.OnComplete(() =>
            {
                OnSceneMenuAnimationEnd.Raise();
            });
        }

    }
}
