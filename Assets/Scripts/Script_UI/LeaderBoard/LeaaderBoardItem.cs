using ScriptableObjects.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class LeaderBoardItem : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI rankText;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField]private Image rankImage;
        [SerializeField] private Image nameImage;
        [SerializeField] private Image scoreImage;

        public void Initialize(int rank, string nameOfPlayer, int score,bool isUser)
        {
            rankText.text = rank.ToString();
            nameText.text = nameOfPlayer;
            scoreText.text = score.ToString();
            if (isUser)
            {
                rankImage.color = Color.yellow;
                nameImage.color = Color.yellow;
                scoreImage.color = Color.yellow;
                PlayerPrefsSaveService.Main.SaveInt("PlayerRank", rank);
            }
            else
            {
                rankImage.color = Color.white;
                nameImage.color = Color.white;
                scoreImage.color = Color.white;
            }
        }
    }
}