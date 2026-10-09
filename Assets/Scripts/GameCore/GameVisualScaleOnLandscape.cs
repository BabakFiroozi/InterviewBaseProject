using UnityEngine;

namespace GameCore
{

    public class GameVisualScaleOnLandscape : MonoBehaviour
    {
        private void Awake()
        {
            ApplyScale();
        }

        private void OnRectTransformDimensionsChange()
        {
            ApplyScale();
        }

        private void ApplyScale()
        {
            bool isLandscape = Screen.width > Screen.height;

            transform.localScale = isLandscape
                ? new Vector3(1.75f, 1.75f, 1f)
                : Vector3.one;
        }
    }

}