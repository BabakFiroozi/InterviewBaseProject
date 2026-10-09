using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Enums;
using ScriptableObjects.GameEvents;
using DataStructure;
using ScriptableObjects.Services;


namespace Data
{
    public class LeaderboardData : MonoBehaviour
    {
        public static LeaderboardData Instance { get; private set; }

        [SerializeField] private TextAsset namesFile;
        [SerializeField] private IntEvent OnLeaderBoardPanelSelected;
        [SerializeField] private NullEvent OnDataLoaded;
        [SerializeField] private NullEvent OnLoadingPanelShow;
        [SerializeField] private IntEvent OnPanelSelected;

        public List<PlayerInfo> players;
        
        [SerializeField] private int loadFrameCount = 1;
        [SerializeField] private int insertChunkSize = 5000;
        private bool _isLoading;
        private LeaderBoardTabType _loadingTab = LeaderBoardTabType.Nothing;
        private Coroutine loadingCoroutine;
        private Coroutine insertCoroutine;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            OnLeaderBoardPanelSelected.OnEventRaised += CheckForLoad;
            OnPanelSelected.OnEventRaised += CheckForFirstShow;
        }

        private void CheckForFirstShow(int value)
        {
            if (value == (int)TabType.LeaderBoard)
                OnLeaderBoardPanelSelected.Raise((int)LeaderBoardTabType.Daily);
        }

        private void OnDisable()
        {
            OnLeaderBoardPanelSelected.OnEventRaised -= CheckForLoad;
            OnPanelSelected.OnEventRaised -= CheckForFirstShow;
        }

        private void CheckForLoad(int tabType)
        {
            if(tabType == (int)_loadingTab) return;
            if (_isLoading) return;
            
            OnLoadingPanelShow.Raise();
            
            _loadingTab = (LeaderBoardTabType)tabType;
            if(loadingCoroutine!=null) StopCoroutine(loadingCoroutine);
            if(insertCoroutine!=null) StopCoroutine(insertCoroutine);
            loadingCoroutine = StartCoroutine(LoadLeaderboardCoroutine());
        }

        private IEnumerator LoadLeaderboardCoroutine()
        {
            _isLoading = true;
            string[] lines = namesFile.text.Split('\n');
            int total = lines.Length;
            int chunkSize = Mathf.CeilToInt((float)total / loadFrameCount);

            players = new List<PlayerInfo>(total);


            for (int f = 0; f < loadFrameCount; f++)
            {
                int start = f * chunkSize;
                int end = Mathf.Min(start + chunkSize, total);

                for (int i = start; i < end; i++)
                {
                    string nameOfUser = lines[i].Trim();
                    players.Add(new PlayerInfo
                    {
                        name = nameOfUser,
                        rank = i + 1,
                        score = Random.Range(0, 100000),
                        isUser = false
                    });
                }

                yield return null; 
            }


            players.Sort((a, b) => b.score.CompareTo(a.score));


            PlayerInfo user = new PlayerInfo
            {
                name = PlayerPrefsSaveService.Main.LoadString("PlayerName", "Honey Drops"),
                score = PlayerPrefsSaveService.Main.LoadInt("PlayerScore", 5000),
                isUser = true
            };

            yield return insertCoroutine = StartCoroutine(InsertUserCoroutine(user));
            
        }

        private IEnumerator InsertUserCoroutine(PlayerInfo user)
        {
            int index = players.BinarySearch(user, new PlayerScoreComparer());
            if (index < 0) index = ~index;

            players.Insert(index, user);
            user.rank = index + 1;

            int total = players.Count;
            
            for (int i = 1; i < total; i += insertChunkSize)
            {
                int end = Mathf.Min(i + insertChunkSize, total);
                for (int j = i; j < end; j++)
                {
                    players[j].rank = j;
                }
                yield return null;
            }
            OnDataLoaded.Raise();
            _isLoading = false;
        }
    }
}


