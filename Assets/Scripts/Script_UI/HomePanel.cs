using ScriptableObjects.GameEvents;
using ScriptableObjects.Services;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Button = UnityEngine.UI.Button;

namespace UI
{
    public class HomePanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI playerNameText;
        [SerializeField] private TextMeshProUGUI playerHighScoreText;
        [SerializeField] private Button startGameButton;
        [SerializeField] private NullEvent onStartGame;
        [SerializeField] private NullEvent onSceneMenuAnimationEnded;
        private void Start()
        {
            playerNameText.text = PlayerPrefsSaveService.Main.LoadString("PlayerName","HoneyDrops");
            playerHighScoreText.text = "HighScore : " + PlayerPrefsSaveService.Main.LoadInt("PlayerHighScore", 5000).ToString();
            startGameButton.onClick.AddListener(StartGame);
        }

        private void OnEnable()
        {
            onStartGame.OnEventRaised += NextScene;
            onSceneMenuAnimationEnded.OnEventRaised += UnloadMenuScene;
        }
        private void OnDisable()
        {
            onStartGame.OnEventRaised -= NextScene;
            onSceneMenuAnimationEnded.OnEventRaised -= UnloadMenuScene;
        }

        private void NextScene()
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1,LoadSceneMode.Additive);
        }

        private void StartGame()
        {
            var eventSystem = FindFirstObjectByType<EventSystem>();
            Destroy(eventSystem.gameObject);
            var old_Camera = FindFirstObjectByType<Camera>();
            Destroy(old_Camera.gameObject);
            onStartGame.Raise();

        }
        
        private void UnloadMenuScene()
        {
            SceneManager.UnloadSceneAsync("Scene_Menu");
        }
    }
}