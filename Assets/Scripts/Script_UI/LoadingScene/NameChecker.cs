using UnityEngine;
using DG.Tweening;
using ScriptableObjects.GameEvents;
using UnityEngine.SceneManagement;
using System.Collections;
using ScriptableObjects.Services;
using TMPro;
using UnityEngine.UI;

namespace UI
{
    public class NameChecker : MonoBehaviour
    {
        [SerializeField] private NullEvent OnLoadingEnd;
        [Header("Refs")]
        [SerializeField] private RectTransform messagePanel;
        [SerializeField] private RectTransform inputFieldRectTransform;
        [SerializeField] private RectTransform saveButtonRectTransform;
        [SerializeField] private Button saveButton;
        [SerializeField] private TMP_InputField nameInputField;

        [Header("Settings")]
        [SerializeField] private float moveDistance = 150f;
        [SerializeField] private float moveDuration = 0.5f;
        [SerializeField] private float bounceDuration = 0.4f;
        [SerializeField] private float shakeStrength = 10f;
        [SerializeField] private float buttonScaleDuration = 0.4f;

        private Vector2 messageOriginal;
        private Vector2 inputOriginal;
        private Vector2 buttonOriginal;

        private void Awake()
        {
            messageOriginal = messagePanel.anchoredPosition;
            inputOriginal = inputFieldRectTransform.anchoredPosition;
            buttonOriginal = saveButtonRectTransform.anchoredPosition;
            OnLoadingEnd.OnEventRaised += CheckForFirstTimeLogin;
            saveButton.onClick.AddListener(SaveName);
        }
        
        private void OnDisable()
        {
            OnLoadingEnd.OnEventRaised -= CheckForFirstTimeLogin;
        }

        private void SaveName()
        {
            var nameText = nameInputField.text;
            if(nameText == "")
                PlayerPrefsSaveService.Main.SaveString("PlayerName","Honey Drops");
            else
                PlayerPrefsSaveService.Main.SaveString("PlayerName", nameText);
            
            PlayerPrefsSaveService.Main.SaveInt("FirstLogin", 1);
            StartCoroutine(ReverseAnimationsCoroutine());
        }

        private void CheckForFirstTimeLogin()
        {
            if (PlayerPrefsSaveService.Main.LoadInt("FirstLogin", 0) == 0)
            {
                for (int i = 0; i < transform.childCount; i++)
                {
                    transform.GetChild(i).gameObject.SetActive(true);
                }
                AnimateMessageUI();
            }
            else
            {
                LoadNextScene();
            }
        }

        private void LoadNextScene()
        {
            SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        }


        private void AnimateMessageUI()
        {
            messagePanel.anchoredPosition = messageOriginal + new Vector2(0, moveDistance);
            messagePanel.DOAnchorPos(messageOriginal, bounceDuration);
            
            inputFieldRectTransform.anchoredPosition = inputOriginal + new Vector2(-moveDistance, 0);
            inputFieldRectTransform.DOAnchorPos(inputOriginal, moveDuration)
                      .SetEase(Ease.OutCubic)
                      .OnComplete(() =>
                      {
                          inputFieldRectTransform.DOShakeAnchorPos(0.2f, shakeStrength, 10, 90, false);
                      });
            
            saveButtonRectTransform.localScale = Vector3.zero;
            saveButtonRectTransform.localRotation = Quaternion.Euler(0, 0, 15f);
            Sequence btnSeq = DOTween.Sequence();
            btnSeq.Append(
                saveButtonRectTransform.DOScale(1f, buttonScaleDuration).SetEase(Ease.OutBack)
            );
            btnSeq.Join(
                saveButtonRectTransform.DOLocalRotate(Vector3.zero, buttonScaleDuration)
            );
        }

        public void ReverseAnimations()
        {
            StartCoroutine(ReverseAnimationsCoroutine());
        }

        private IEnumerator ReverseAnimationsCoroutine()
        {
            messagePanel.DOAnchorPos(messageOriginal + new Vector2(0, moveDistance), moveDuration)
                .SetEase(Ease.InCubic);
            
            inputFieldRectTransform.DOAnchorPos(inputOriginal + new Vector2(-moveDistance, 0), moveDuration)
                .SetEase(Ease.InCubic);
            
            saveButtonRectTransform.DOScale(0f, buttonScaleDuration)
                .SetEase(Ease.InBack);
            
            float longestDuration = Mathf.Max(moveDuration, buttonScaleDuration);
            yield return new WaitForSeconds(longestDuration);
            
            LoadNextScene();
        }
    }
}

