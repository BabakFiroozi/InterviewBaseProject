using System;
using DG.Tweening;
using ScriptableObjects.GameEvents;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

namespace UI
{
    public class GameOverPanelAnimator : MonoBehaviour
    {
        [SerializeField] private NullEvent onGameOver;
        [SerializeField] private NullEvent onGameReset;
        [SerializeField] private NullEvent onGoHome;
        [SerializeField] private NullEvent onGameStart;
        [Header("UI References")]
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI highScoreText;

        [SerializeField] private GameObject resetButton;
        [SerializeField] private GameObject homeButton;
        [SerializeField] private GameObject shareButton;

        [Header("Animation Settings")]
        [SerializeField] private float moveDuration = 0.8f;
        [SerializeField] private float staggerDelay = 0.2f;

        private Vector3 playerNameStartPos;
        private Vector3 scoreStartPos;
        private Vector3 highScoreStartPos;

        private Vector3 resetButtonStartPos;
        private Vector3 homeButtonStartPos;
        private Vector3 shareButtonStartPos;

        private void Start()
        {
            if (playerNameText) playerNameStartPos = playerNameText.transform.position;
            if (scoreText) scoreStartPos = scoreText.transform.position;
            if (highScoreText) highScoreStartPos = highScoreText.transform.position;

            if (resetButton) resetButtonStartPos = resetButton.transform.position;
            if (homeButton) homeButtonStartPos = homeButton.transform.position;
            if (shareButton) shareButtonStartPos = shareButton.transform.position;
        }

        private void OnEnable()
        {
            onGameOver.OnEventRaised += PlayShowAnimation;
            onGameReset.OnEventRaised += GameReset;
            onGoHome.OnEventRaised += GoHome;
        }

        private void GoHome()
        {
            PlayHideAnimation(() => { SceneManager.LoadSceneAsync("Scene_Menu");}
                );
        }

        private void GameReset()
        {
            PlayHideAnimation(()=>{ onGameStart.Raise();}
                ,true);
        }

        private void OnDisable()
        {
            onGameOver.OnEventRaised -= PlayShowAnimation;
            onGameReset.OnEventRaised -= GameReset;
            onGoHome.OnEventRaised -= GoHome;
        }


        private void PlayShowAnimation()
        {
            gameOverPanel.SetActive(true);
            if (playerNameText) playerNameText.transform.position = playerNameStartPos + Vector3.up * 1500;
            if (scoreText) scoreText.transform.position = scoreStartPos + Vector3.up * 1500;
            if (highScoreText) highScoreText.transform.position = highScoreStartPos + Vector3.up * 1500;

            if (resetButton) resetButton.transform.position = resetButtonStartPos + Vector3.down * 1500;
            if (homeButton) homeButton.transform.position = homeButtonStartPos + Vector3.right * 1500;
            if (shareButton) shareButton.transform.position = shareButtonStartPos + Vector3.left * 1500;

            Sequence seq = DOTween.Sequence();
            seq.AppendInterval(1.5f);
            
            if (playerNameText)
                seq.Append(playerNameText.transform.DOMoveY(playerNameStartPos.y, moveDuration));
            if (scoreText)
                seq.Join(scoreText.transform.DOMoveY(scoreStartPos.y, moveDuration).SetDelay(staggerDelay));
            if (highScoreText)
                seq.Join(highScoreText.transform.DOMoveY(highScoreStartPos.y, moveDuration).SetDelay(staggerDelay * 2));

  
            if (resetButton)
                seq.Join(resetButton.transform.DOMoveY(resetButtonStartPos.y, moveDuration).SetDelay(staggerDelay));
            if (homeButton)
                seq.Join(homeButton.transform.DOMoveX(homeButtonStartPos.x, moveDuration).SetDelay(staggerDelay));
            if (shareButton)
                seq.Join(shareButton.transform.DOMoveX(shareButtonStartPos.x, moveDuration));

            seq.Play();
        }
        
        private void PlayHideAnimation(Action onComplete,bool reset = false)
        {

            Sequence seq = DOTween.Sequence();


            if (playerNameText)
                seq.Append(playerNameText.transform.DOMoveY(playerNameStartPos.y + 1500, moveDuration).SetEase(Ease.InCubic));
            if (scoreText)
                seq.Join(scoreText.transform.DOMoveY(scoreStartPos.y + 1500, moveDuration).SetEase(Ease.InCubic));
            if (highScoreText)
                seq.Join(highScoreText.transform.DOMoveY(highScoreStartPos.y + 1500, moveDuration).SetEase(Ease.InCubic));
            if (resetButton)
                seq.Join(resetButton.transform.DOMoveY(resetButtonStartPos.y - 1500, moveDuration).SetEase(Ease.InCubic));
            if (homeButton)
                seq.Join(homeButton.transform.DOMoveX(homeButtonStartPos.x + 1500, moveDuration).SetEase(Ease.InCubic));
            if (shareButton)
                seq.Join(shareButton.transform.DOMoveX(shareButtonStartPos.x - 1500, moveDuration).SetEase(Ease.InCubic));

            if(reset) seq.AppendCallback(() => onComplete.Invoke());
            else
                seq.OnComplete(() =>
                {
                    onComplete?.Invoke();
                });
            seq.Play();
        }
    }
}
