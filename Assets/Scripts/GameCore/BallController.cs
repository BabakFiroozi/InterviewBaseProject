using DG.Tweening;
using Enums;
using UnityEngine;
using UnityEngine.InputSystem;
using ScriptableObjects.GameEvents;

namespace GameCore
{
    public class BallController : MonoBehaviour
    {
        [SerializeField] private IntEvent OnScoreChanged;
        [SerializeField] private NullEvent OnGameEnded;
        [SerializeField] private NullEvent OnStartMoving;
        [SerializeField] private ParticleSystem dieParticle;

        [SerializeField] private Transform center;
        [SerializeField] private float outsideRadius = 2f;
        [SerializeField] private float insideRadius = 1.3f;
        [SerializeField] private float moveTime = 0.2f;

        [Header("Rotation")]
        [SerializeField] private float speed = 360f;       
        [SerializeField] private float maxSpeed = 1200f;  
        [SerializeField] private float speedIncreaseRate = 50f; 

        [SerializeField] private int increasedScoreByPickingStars = 1;

        [Header("Death Animation")]
        [SerializeField] private float dieScaleTime = 0.5f;
        [SerializeField] private float dieRotationTime = 0.5f;
        [SerializeField] private float dieFadeTime = 0.5f;

        /// <summary>Multiplies the points a star gives (1 = normal). Used by the double-score power-up.</summary>
        public int ScoreMultiplier { get; set; } = 1;

        private int _score;
        private float angle;
        private float radius;
        private bool isInside;
        private bool canMove; 

        private SpriteRenderer _spriteRenderer;
        private Vector3 _firstLocalScale;
        private float _firstSpeed ;
        private Color _firstColor;

        private void Start()
        {
            _firstSpeed = speed;
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _firstColor = _spriteRenderer.color;
            _firstLocalScale = transform.localScale;
            radius = outsideRadius* transform.parent.localScale.x;
        }

        private void OnEnable()
        {
            OnGameEnded.OnEventRaised += Die;
            OnStartMoving.OnEventRaised += StartMovement; 
        }

        private void OnDisable()
        {
            OnGameEnded.OnEventRaised -= Die;
            OnStartMoving.OnEventRaised -= StartMovement;
        }

        private void StartMovement()
        {
            canMove = true;
        }

        private void Update()
        {
            if (!canMove)
                return;

            if (speed < maxSpeed)
                speed = Mathf.Min(speed + speedIncreaseRate * Time.deltaTime, maxSpeed);

            angle += speed * Time.deltaTime;
            angle %= 360f;

            // Taps on the power-up buttons must not also flip the ball between the lanes.
            // Pointer covers mouse, touch and pen with the new Input System.
            var pointer = Pointer.current;
            if (pointer != null && pointer.press.wasPressedThisFrame
                && !Shop.Views.HudTapGate.IsBlocked(pointer.position.ReadValue()))
            {
                AudioManger.AudioManager.Instance.PlaySFX(SoundType.Move);
                isInside = !isInside;
                float target = isInside ? insideRadius * transform.parent.localScale.x : outsideRadius* transform.parent.localScale.x;
                DOTween.To(() => radius, r => radius = r, target, moveTime)
                       .SetEase(Ease.OutBack);
            }

            float rad = angle * Mathf.Deg2Rad;
            transform.position = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!canMove)
                return;

            if (other.CompareTag("Obsticle"))
                OnGameEnded.Raise();
            else
            {
                _score += increasedScoreByPickingStars * ScoreMultiplier;
                OnScoreChanged.Raise(_score);
                AudioManger.AudioManager.Instance.PlaySFX(SoundType.Score);
            }
        }

        private void Die()
        {
            AudioManger.AudioManager.Instance.PlaySFX(SoundType.Explosion);
            speed = 0;

            if (dieParticle)
                dieParticle.Play();
            
            Sequence dieSequence = DOTween.Sequence();
            dieSequence.Append(transform.DOScale(Vector3.zero, dieScaleTime).SetEase(Ease.InBack));
            dieSequence.Join(transform.DORotate(new Vector3(0, 0, 720f), dieRotationTime, RotateMode.FastBeyond360));
            
            if (_spriteRenderer)
                dieSequence.Join(_spriteRenderer.DOFade(0f, dieFadeTime));

            dieSequence.OnComplete(() =>
            {
                float rad = 0f;
                transform.position = center.position + new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0) * radius;
                
                gameObject.SetActive(false);
                DOTween.Kill(transform);
                
                if (_spriteRenderer)
                    _spriteRenderer.color = _firstColor;
                
                angle = 0f;
                radius = outsideRadius* transform.parent.localScale.x;
                transform.localScale = _firstLocalScale;
                isInside = false;
                canMove = false;
                speed = _firstSpeed;
                _score = 0;
            });
        }
    }
    
}
