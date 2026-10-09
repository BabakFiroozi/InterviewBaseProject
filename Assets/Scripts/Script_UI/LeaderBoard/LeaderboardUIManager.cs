using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using ScriptableObjects.GameEvents;

namespace UI
{
    public class LeaderboardUIManager : MonoBehaviour
    {
        [SerializeField] private NullEvent OnDataLoaded;
        [SerializeField] private NullEvent OnLeaderboardLoading;
        [SerializeField] private RectTransform content;
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private int visibleCount = 20;
        [SerializeField] private float itemHeight = 80f;

        private readonly List<GameObject> itemPool = new List<GameObject>();
        private int totalCount;


        private void Start()
        {
            EnsurePoolSize(visibleCount);
        }
        private void OnEnable()
        {
            OnDataLoaded.OnEventRaised += LoadUIFromData;
            OnDataLoaded.OnEventRaised += EnableScrollRect;
            OnLeaderboardLoading.OnEventRaised += DisableScrollRect;
        }

        private void OnDisable()
        {
            OnDataLoaded.OnEventRaised -= EnableScrollRect;
            OnDataLoaded.OnEventRaised -= LoadUIFromData;
            scrollRect.onValueChanged.RemoveAllListeners();
            OnLeaderboardLoading.OnEventRaised -= DisableScrollRect;
        }

        private void DisableScrollRect()
        {
            scrollRect.enabled = false;
        }
        private void EnableScrollRect()
        {
            scrollRect.enabled = true;
        }
        

        private void LoadUIFromData()
        {
            totalCount = Data.LeaderboardData.Instance.players.Count;

            content.sizeDelta = new Vector2(content.sizeDelta.x, totalCount * itemHeight);
            
            scrollRect.onValueChanged.RemoveAllListeners();
            scrollRect.onValueChanged.AddListener(OnScrollValueChanged);

            AutoScrollToUser();
        }


        private void EnsurePoolSize(int needed)
        {
            while (itemPool.Count < needed)
            {
                GameObject go = Instantiate(itemPrefab, content);
                itemPool.Add(go);
            }


            for (int i = 0; i < itemPool.Count; i++)
                itemPool[i].SetActive(i < needed);
        }


        private void AutoScrollToUser()
        {
            int userIndex = Data.LeaderboardData.Instance.players.FindIndex(p => p.isUser);
            if (userIndex == -1) return;

            int startIndex = Mathf.Clamp(userIndex - visibleCount / 2, 0, totalCount - visibleCount);

            UpdateItems(startIndex);

            float normalized = 1f - ((float)startIndex / (totalCount - visibleCount));
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(normalized);
        }


        private void OnScrollValueChanged(Vector2 pos)
        {
            if (totalCount <= visibleCount) return;

            int startIndex = Mathf.FloorToInt((1 - pos.y) * (totalCount - visibleCount));
            startIndex = Mathf.Clamp(startIndex+5, 0, totalCount - visibleCount);

            UpdateItems(startIndex);
        }


        private void UpdateItems(int startIndex)
        {
            var players = Data.LeaderboardData.Instance.players;

            for (int i = 0; i < visibleCount; i++)
            {
                int dataIndex = startIndex + i;

                if (dataIndex >= totalCount)
                {
                    itemPool[i].SetActive(false);
                    continue;
                }

                GameObject go = itemPool[i];
                go.SetActive(true);

                var player = players[dataIndex];

                var ui = go.GetComponent<LeaderBoardItem>();
                ui.Initialize(player.rank, player.name, player.score, player.isUser);

                RectTransform rt = go.GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(0, -dataIndex * itemHeight);
            }
        }
    }
}
