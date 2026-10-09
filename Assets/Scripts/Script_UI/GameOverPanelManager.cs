
using ScriptableObjects.GameEvents;
using ScriptableObjects.Services;
using TMPro;
using UnityEngine;

namespace UI
{
    public class GameOverPanelManager : MonoBehaviour
    {
       [SerializeField] private IntEvent OnScoreChanged;

       
       [SerializeField] private TextMeshProUGUI playerNameText;
       [SerializeField] private TextMeshProUGUI playerScoreText;
       [SerializeField] private TextMeshProUGUI playerHighScoreText;
       

       void Start()
       {
           playerScoreText.text = "Score: " + 0;
           playerNameText.text = PlayerPrefsSaveService.Main.LoadString("PlayerName","Honey Drops");
           playerHighScoreText.text ="HighScore : " +  PlayerPrefsSaveService.Main.LoadInt("PlayerScore",5000).ToString();
           
       }

       private void OnEnable()
       {
           OnScoreChanged.OnEventRaised += OnScoreChange;
       }
       private void OnDisable()
       {
           OnScoreChanged.OnEventRaised -= OnScoreChange;
       }

       private void OnScoreChange(int score)
       {
           playerScoreText.text = "Score : " + score;
            playerHighScoreText.text = "High Score : " + PlayerPrefsSaveService.Main.LoadInt("PlayerScore",5000);
       }
   }
}
