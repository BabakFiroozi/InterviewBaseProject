using UnityEngine;
using DG.Tweening;
namespace UI
{
    public class RotateLoop : MonoBehaviour
    {
        [SerializeField] private Vector3 rotationAmount = new Vector3(0, 0, 360f);
        [SerializeField] private float duration = 2f;

        private void Start()
        {
            transform
                .DORotate(rotationAmount, duration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)
                .SetLoops(-1, LoopType.Restart);
        }
    }

}