using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private Image backgroundImage;
        
        [Header("Animation Settings")]
        [SerializeField] private float fadeSpeed = 10f;
        [SerializeField] private Vector3 offset = new(0, 2f, 0);
        [SerializeField] private bool followTarget = true;
        
        [Header("Visual Settings")]
        [SerializeField] private Color defaultColor = new(0.2f, 0.2f, 0.2f, 0.9f);
        [SerializeField] private Color highlightColor = new(0.3f, 0.5f, 0.8f, 0.9f);
        
        private Transform targetTransform;
        private Camera mainCamera;
        private RectTransform rectTransform;
        private bool isVisible;
        private float targetAlpha;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            mainCamera = Camera.main;
            
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup) canvasGroup.alpha = 0;
            
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!isVisible) return;
            
            if (canvasGroup)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);
            }
            
            if (!followTarget || !targetTransform || !mainCamera) return;
            Vector3 worldPosition = targetTransform.position + offset;
            Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition);
                
            if (screenPosition.z > 0)
            {
                rectTransform.position = screenPosition;
                targetAlpha = 1f;
            }
            else
            {
                targetAlpha = 0f;
            }
        }

        public void Show(string prompt, Transform target, string key = "E", bool highlight = false)
        {
            gameObject.SetActive(true);
            isVisible = true;
            targetTransform = target;
            targetAlpha = 1f;
            
            if (promptText) promptText.text = prompt;
            if (keyText) keyText.text = $"[{key}]";
            
            if (backgroundImage)
            {
                backgroundImage.color = highlight ? highlightColor : defaultColor;
            }
        }

        public void Hide()
        {
            isVisible = false;
            targetAlpha = 0f;
            
            if (canvasGroup && canvasGroup.alpha < 0.01f)
            {
                gameObject.SetActive(false);
            }
        }

        public void UpdatePrompt(string newPrompt)
        {
            if (promptText) promptText.text = newPrompt;
        }
    }
}