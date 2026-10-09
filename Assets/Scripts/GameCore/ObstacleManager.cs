using ScriptableObjects.GameEvents;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace GameCore
{
    public class ObstacleManager : MonoBehaviour
    {
        [SerializeField] private IntEvent OnScoreChanged;
        [SerializeField] private NullEvent OnBallStartMovingEvent;
        [SerializeField] private NullEvent OnGameOverEvent;


        [SerializeField] private Transform center;
        [SerializeField] private GameObject obstaclePrefab;
        [SerializeField] private GameObject scoreItemPrefab;

        [SerializeField] private float insideRadius = 1.3f;
        [SerializeField] private float outsideRadius = 2f;

        [SerializeField] private int scoreStep = 100;

        [SerializeField] private float spawnAnimationTime = 0.2f;
        [SerializeField] private float removeAnimTime = 0.25f;

        [SerializeField] private float minDistanceBetweenObjects = 0.5f;

        private List<GameObject> _obsticles = new List<GameObject>();
        private GameObject _currentScoreItem;
        private int _lastScoreStep = 0;

        private bool _isGameOver = false;



        private void OnBallStartMoving()
        {
            var seq = DOTween.Sequence();
            seq.AppendCallback(() =>
            {
                _isGameOver = false;
                for (int i = 0; i < 4; i++)
                    SpawnNewObstacle();
                RefreshScoreItem();
            }).SetDelay(0.5f);
        }


        void OnEnable()
        {
            OnScoreChanged.OnEventRaised += OnScoreEvent;
            OnBallStartMovingEvent.OnEventRaised += OnBallStartMoving;
            OnGameOverEvent.OnEventRaised += CleaningRing;
        }

        private void CleaningRing()
        {
            _isGameOver = true;
            List<GameObject> toRemove = new List<GameObject>();

            for (int i = 0; i < _obsticles.Count; i++)
            {
                toRemove.Add(_obsticles[i]);
            }

            _obsticles.Clear();

            foreach (var ob in toRemove)
                RemoveWithAnimation(ob);

            RemoveScoreItem(_currentScoreItem);

        }

        void OnDisable()
        {
            OnScoreChanged.OnEventRaised -= OnScoreEvent;
            OnBallStartMovingEvent.OnEventRaised -= OnBallStartMoving;
            OnGameOverEvent.OnEventRaised -= CleaningRing;
        }

        void OnScoreEvent(int score)
        {
            int step = score / scoreStep;

            if (step > _lastScoreStep)
            {
                _lastScoreStep = step;
                SpawnNewObstacle();
            }

            RefreshObstacles();
            _currentScoreItem.GetComponentInChildren<ParticleSystem>().Play();
            RefreshScoreItem();
        }

        void RefreshObstacles()
        {
            if (_isGameOver) return;
            if (_obsticles.Count == 0)
                return;

            int removeCount = Mathf.CeilToInt(_obsticles.Count * 0.4f);
            List<GameObject> toRemove = new List<GameObject>();

            for (int i = 0; i < removeCount; i++)
            {
                int index = Random.Range(0, _obsticles.Count);
                toRemove.Add(_obsticles[index]);
                _obsticles.RemoveAt(index);
            }

            foreach (var ob in toRemove)
                RemoveWithAnimation(ob);

            for (int i = 0; i < removeCount; i++)
                SpawnNewObstacle();
        }

        void SpawnNewObstacle()
        {
            if (_isGameOver) return;

            Vector3 pos = GetValidSpawnPosition();
            GameObject ob = Instantiate(obstaclePrefab, pos, Quaternion.identity);
            ob.transform.localScale *= transform.parent.lossyScale.x;
            _obsticles.Add(ob);

            var firstScale = ob.transform.localScale;
            ob.transform.localScale = Vector3.zero;
            ob.transform.up = (pos - center.position).normalized;
            ob.transform.DOScale(firstScale, spawnAnimationTime).SetEase(Ease.OutBack);
        }

        void RefreshScoreItem()
        {
            if (_isGameOver) return;

            if (_currentScoreItem)
                RemoveScoreItem(_currentScoreItem);

            Vector3 pos = GetValidSpawnPosition();
            _currentScoreItem = Instantiate(scoreItemPrefab, pos, Quaternion.identity);
            _currentScoreItem.transform.localScale *= transform.parent.lossyScale.x;
            var firstScale = _currentScoreItem.transform.localScale;
            _currentScoreItem.transform.localScale = Vector3.zero;
            _currentScoreItem.transform.up = (pos - center.position).normalized;

            _currentScoreItem.transform.DOScale(firstScale, spawnAnimationTime)
                .SetEase(Ease.OutBack);

            _currentScoreItem.transform.DORotate(new Vector3(0, 0, 360f), 1.4f, RotateMode.FastBeyond360)
                .SetLoops(-1)
                .SetEase(Ease.Linear);
        }

        Vector3 GetValidSpawnPosition()
        {
            int attempts = 0;
            Vector3 pos = Vector3.zero;
            float radius = Random.value > 0.5f
                ? insideRadius * transform.parent.localScale.x
                : outsideRadius * transform.parent.localScale.x;

            while (attempts < 50)
            {
                float angle = Random.Range(0f, 360f);
                float rad = angle * Mathf.Deg2Rad;
                pos = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;

                bool tooClose = false;
                foreach (var ob in _obsticles)
                {
                    if (Vector3.Distance(pos, ob.transform.position) < minDistanceBetweenObjects)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (_currentScoreItem && Vector3.Distance(pos, _currentScoreItem.transform.position) <
                    minDistanceBetweenObjects)
                    tooClose = true;

                if (!tooClose)
                    break;

                attempts++;
            }

            return pos;
        }

        void RemoveWithAnimation(GameObject ob)
        {
            DOTween.Kill(ob.transform);
            ob.transform.DOScale(Vector3.zero, removeAnimTime)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    if (ob)
                        Destroy(ob);
                });
        }

        void RemoveScoreItem(GameObject ob)
        {

            DOTween.Kill(ob.transform);
            ob.transform.DOScale(Vector3.zero, removeAnimTime)
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {

                    if (ob)
                        Destroy(ob);
                });
        }
    }
}
