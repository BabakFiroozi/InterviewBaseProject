
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Enums;
using ScriptableObjects.GameEvents;

namespace UI
{
    public class LeaderBoardTabButton : MonoBehaviour
    {
        [SerializeField] private LeaderBoardTabType tab;     
        [SerializeField] private IntEvent onTabSelected;      

        [Header("Button References")]
        [SerializeField] private Button button;

        [Header("Animation Settings")]
        [SerializeField] private Color activeButtonColor = Color.white;
        [SerializeField] private Color inactiveButtonColor = Color.white;
        [SerializeField] private Sprite activeButtonSprite;
        [SerializeField] private Sprite inactiveButtonSprite;

        private Image buttonImage;

        private void Awake()
        {
            if (button)
            {
                button.onClick.AddListener(OnClick);
                buttonImage = button.GetComponent<Image>();
            }
        }

        private void OnEnable()
        {
            onTabSelected.OnEventRaised += HandleTabSelected;
        }

        private void OnDisable()
        {
            onTabSelected.OnEventRaised -= HandleTabSelected;
        }

        private void OnClick()
        {
            onTabSelected.Raise((int)tab);
        }

        private void HandleTabSelected(int selectedIndex)
        {
            bool isActive = (int)tab == selectedIndex;
            
            Animate(isActive);
            
            if (isActive)
                transform.SetAsLastSibling();
        }


        private void Animate(bool isActive)
        {
            transform.DOKill();
            buttonImage.sprite = isActive ? activeButtonSprite : inactiveButtonSprite;
            buttonImage.DOColor(isActive ? activeButtonColor : inactiveButtonColor, 0.1f);
        }
        
    }
}
