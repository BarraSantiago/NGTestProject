using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using StarterAssets;

namespace Dialogue
{
    public class DialogueUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject dialoguePanel;
        [SerializeField] private TMP_Text speakerNameText;
        [SerializeField] private TMP_Text dialogueText;
        [SerializeField] private GameObject continueIndicator;
        [SerializeField] private Button continueButton;

        [Header("Animation")]
        [SerializeField] private float fadeSpeed = 5f;
        [SerializeField] private AnimationCurve showCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Input")]
        [SerializeField] private InputActionReference interactAction;

        private CanvasGroup canvasGroup;
        private bool isShowing;
        private string fullText;
        private bool isSubscribed;
        private StarterAssetsInputs starterInputs;

        private void Awake()
        {
            canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();
            if (!canvasGroup)
            {
                canvasGroup = dialoguePanel.AddComponent<CanvasGroup>();
            }

            if (continueButton)
            {
                continueButton.onClick.AddListener(OnContinueClicked);
            }

            // Find player inputs
            starterInputs = FindFirstObjectByType<StarterAssetsInputs>();

            dialoguePanel.SetActive(false);
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void OnEnable()
        {
            if (DialogueManager.Instance)
            {
                SubscribeToEvents();
            }

            if (interactAction)
            {
                interactAction.action.Enable();
            }
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();

            if (interactAction)
            {
                interactAction.action.Disable();
            }
        }

        private void SubscribeToEvents()
        {
            if (isSubscribed || !DialogueManager.Instance) return;

            DialogueManager.Instance.OnDialogueStart += ShowDialogue;
            DialogueManager.Instance.OnDialogueLineChanged += UpdateDialogueText;
            DialogueManager.Instance.OnDialogueEnd += HideDialogue;
            DialogueManager.Instance.OnTypingComplete += ShowContinueIndicator;

            isSubscribed = true;
        }

        private void UnsubscribeFromEvents()
        {
            if (!isSubscribed || !DialogueManager.Instance) return;

            DialogueManager.Instance.OnDialogueStart -= ShowDialogue;
            DialogueManager.Instance.OnDialogueLineChanged -= UpdateDialogueText;
            DialogueManager.Instance.OnDialogueEnd -= HideDialogue;
            DialogueManager.Instance.OnTypingComplete -= ShowContinueIndicator;

            isSubscribed = false;
        }

        private void Update()
        {
            if (!isShowing) return;

            // Use Input System instead of old Input class
            if (interactAction && interactAction.action.WasPressedThisFrame())
            {
                OnContinueClicked();
            }
        }

        private void ShowDialogue(string speakerName, string firstLine)
        {
            isShowing = true;
            dialoguePanel.SetActive(true);

            if (speakerNameText) speakerNameText.text = speakerName;
            if (dialogueText) dialogueText.text = "";

            if (continueIndicator) continueIndicator.SetActive(false);

            // Lock camera rotation during dialogue
            if (starterInputs)
            {
                starterInputs.cursorInputForLook = false;
                starterInputs.UpdateCursorState(false);
            }

            StopAllCoroutines();
            StartCoroutine(FadeIn());
        }

        private void UpdateDialogueText(string text)
        {
            if (dialogueText) dialogueText.text = text;
            fullText = text;
        }

        private void ShowContinueIndicator()
        {
            if (continueIndicator) continueIndicator.SetActive(true);
        }

        private void HideDialogue()
        {
            isShowing = false;
        
            // Unlock camera rotation after dialogue
            if (starterInputs)
            {
                starterInputs.cursorInputForLook = true;
                starterInputs.cursorLocked = true;
                starterInputs.UpdateCursorState(true);
            }
        
            StopAllCoroutines();
            StartCoroutine(FadeOut());
        }

        private void OnContinueClicked()
        {
            if (!DialogueManager.Instance) return;

            if (continueIndicator) continueIndicator.SetActive(false);

            DialogueManager.Instance.DisplayNextLine();
        }

        private IEnumerator FadeIn()
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                canvasGroup.alpha = showCurve.Evaluate(elapsed);
                yield return null;
            }
            canvasGroup.alpha = 1f;
        }

        private IEnumerator FadeOut()
        {
            float elapsed = 0f;
            while (elapsed < 1f)
            {
                elapsed += Time.deltaTime * fadeSpeed;
                canvasGroup.alpha = 1f - showCurve.Evaluate(elapsed);
                yield return null;
            }
            canvasGroup.alpha = 0f;
            dialoguePanel.SetActive(false);
        }
    }
}