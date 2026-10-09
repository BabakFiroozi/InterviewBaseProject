using Enums;
using ScriptableObjects.GameEvents;
using ScriptableObjects.Services;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameSceneManager : MonoBehaviour
    {
        [SerializeField] private IntEvent OnScoreChanged;
        [SerializeField] private NullEvent OnGameEnded;
        [SerializeField] private NullEvent OnGameStarted;

        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerScoreText;
        [SerializeField] private TextMeshProUGUI playerHighScoreText;

        private int playerScore;
        private int playerHighScore;
        private bool highScoreSoundPlayed;

        private void Start()
        {
            playerNameText.text = PlayerPrefsSaveService.Main.LoadString("PlayerName", "Honey Drops");
            
            playerScore = 0;
            playerScoreText.text = "Score : 0";

            playerHighScore = PlayerPrefsSaveService.Main.LoadInt("PlayerScore", 5000);
            playerHighScoreText.text = "HighScore : " + playerHighScore;
        }

        private void OnEnable()
        {
            OnScoreChanged.OnEventRaised += OnScoreChange;
            OnGameStarted.OnEventRaised += OnGameStart;
        }

        private void OnDisable()
        {
            OnScoreChanged.OnEventRaised -= OnScoreChange;
            OnGameStarted.OnEventRaised -= OnGameStart;
        }

        private void OnGameStart()
        {

            highScoreSoundPlayed = false;

            playerScore = 0;
            playerScoreText.text = "Score : 0";
        }

        private void OnScoreChange(int score)
        {
            playerScore = score;
            playerScoreText.text = "Score : " + score;
            
            if (playerScore > playerHighScore)
            {
                playerHighScore = playerScore;
                PlayerPrefsSaveService.Main.SaveInt("PlayerScore", playerHighScore);
                playerHighScoreText.text = "HighScore : " + playerHighScore;
                
                if (!highScoreSoundPlayed)
                {
                    highScoreSoundPlayed = true;
                    AudioManger.AudioManager.Instance.PlaySFX(SoundType.HighScore);
                }
            }
        }

    }
}
