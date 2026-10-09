using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class CustomizeButton : UnityEngine.UI.Button
    {
        [Header("Text Press Effect")]
        private RectTransform buttonText;
        private float pressOffsetY = -15f;
        private Vector3 originalTextPos;

        [Header("Auto Reactivate")]
        public float reactivateDelay = 3f;
        private bool waiting;

        protected override void Start()
        {
            base.Start();
            buttonText = transform.GetChild(0).GetComponent<RectTransform>();
            if (buttonText != null)
                originalTextPos = buttonText.localPosition;
        }
        
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (buttonText != null)
                buttonText.localPosition = originalTextPos + new Vector3(0, pressOffsetY, 0);
        }

        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (buttonText != null)
                buttonText.localPosition = originalTextPos;
        }

        public  void ActivateInteractableAfterDelay()
        {
            if (!interactable && !waiting && gameObject.activeInHierarchy)
                StartCoroutine(ReEnableAfterDelay());
        }

        private System.Collections.IEnumerator ReEnableAfterDelay()
        {
            waiting = true;
            yield return new WaitForSeconds(reactivateDelay);
            interactable = true;
            waiting = false;
        }
    }
}
