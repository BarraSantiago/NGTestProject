using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    public class InteractionPromptUI : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private TMP_Text promptText;
        [SerializeField] private TMP_Text keyText;
        [SerializeField] private Image backgroundImage;

        [Header("Animation Settings")]
        [SerializeField] private float fadeSpeed = 10f;
        [SerializeField] private Vector3 offset = new(0, 0.5f, 0);

        [Header("Visual Settings")]
        [SerializeField] private Color defaultColor = new(0.2f, 0.2f, 0.2f, 0.9f);
        [SerializeField] private Color highlightColor = new(0.3f, 0.5f, 0.8f, 0.9f);
        [SerializeField] private float worldCanvasScale = 0.005f;

        private Transform targetTransform;
        private bool isVisible;
        private float targetAlpha;
        private bool hasInteracted;
        private Camera _camera;
        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            if (!canvas) canvas = GetComponent<Canvas>();
            if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();

            // Setup world space canvas
            if (canvas)
            {
                canvas.renderMode = RenderMode.WorldSpace;
                transform.SetParent(canvas.transform);
            }

            if (canvasGroup) canvasGroup.alpha = 0;

            gameObject.SetActive(false);
        }

        private void Start()
        {
            _camera = Camera.main;
        }

        private void LateUpdate()
        {
            // Smooth fade
            if (canvasGroup)
            {
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, Time.deltaTime * fadeSpeed);

                // Disable after fade out
                if (!isVisible && canvasGroup.alpha < 0.01f)
                {
                    gameObject.SetActive(false);
                    return;
                }
            }

            // Follow target position and face camera
            if (!targetTransform || !_camera) return;
            
            transform.position = targetTransform.position + offset;
            
            // Face camera directly (billboard effect)
            Vector3 directionToCamera = _camera.transform.position - transform.position;
            transform.rotation = Quaternion.LookRotation(-directionToCamera);
        }

        public void Show(string prompt, Transform target, string key = "E", bool highlight = false)
        {
            if (!gameObject.activeSelf)
            {
                gameObject.SetActive(true);
            }

            isVisible = true;
            targetTransform = target;
            targetAlpha = 1f;
            hasInteracted = false;

            if (promptText) promptText.text = prompt;
            if (keyText) keyText.text = $"[{key}]";

            if (backgroundImage)
            {
                backgroundImage.color = highlight ? highlightColor : defaultColor;
            }

            // Position immediately
            if (targetTransform)
            {
                transform.position = targetTransform.position + offset;
            }
        }

        public void Hide()
        {
            isVisible = false;
            targetAlpha = 0f;
        }

        public void OnInteracted()
        {
            hasInteracted = true;
            Hide();
        }

        public void UpdatePrompt(string newPrompt)
        {
            if (promptText) promptText.text = newPrompt;
        }

        public bool HasBeenInteracted => hasInteracted;
    }
}