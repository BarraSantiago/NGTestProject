using UnityEngine;
using Dialogue;

namespace Interactables
{
    public class InteractableNPC : MonoBehaviour, IInteractable
    {
        [Header("NPC Settings")]
        [SerializeField] private string npcName = "NPC";
        [SerializeField, TextArea(3, 10)] private string[] dialogueLines;
        [SerializeField] private float interactionRange = 3f;
        [SerializeField] private bool repeatDialogue = true;

        [Header("Camera Focus")]
        [SerializeField] public Transform cameraLookTarget;

        [Header("Animation")]
        [SerializeField] private Animator npcAnimator;

        private int currentDialogueIndex;
        private GameObject currentInteractor;

        private void Awake()
        {
            if (!cameraLookTarget)
            {
                cameraLookTarget = transform;
            }

            if (!npcAnimator)
            {
                npcAnimator = GetComponent<Animator>();
            }
        }

        private void OnEnable()
        {
            if (DialogueManager.Instance)
            {
                DialogueManager.Instance.OnDialogueEnd += OnDialogueEnded;
            }
        }

        private void OnDisable()
        {
            if (DialogueManager.Instance)
            {
                DialogueManager.Instance.OnDialogueEnd -= OnDialogueEnded;
            }
        }

        public string GetInteractionPrompt()
        {
            return $"Talk to {npcName}";
        }

        public void Interact(GameObject interactor)
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
            {
                Debug.LogWarning($"{npcName}: No dialogue lines assigned!");
                return;
            }

            if (DialogueManager.Instance && DialogueManager.Instance.IsActive)
            {
                return;
            }

            if (DialogueManager.Instance)
            {
                currentInteractor = interactor;

                // Trigger NPC animation
                if (npcAnimator)
                {
                    npcAnimator.SetTrigger("BeginDialogue");
                }

                // Trigger Player animation
                Animator playerAnimator = interactor.GetComponent<Animator>();
                if (playerAnimator)
                {
                    playerAnimator.SetTrigger("BeginDialogue");
                }

                DialogueManager.Instance.StartDialogue(npcName, dialogueLines);

                if (repeatDialogue)
                {
                    currentDialogueIndex = (currentDialogueIndex + 1) % dialogueLines.Length;
                }
            }
            else
            {
                Debug.LogError("DialogueManager not found! Make sure it exists in the scene.");
            }
        }

        private void OnDialogueEnded()
        {
            // Trigger NPC end animation
            if (npcAnimator)
            {
                npcAnimator.SetTrigger("EndDialogue");
            }

            // Trigger Player end animation
            if (!currentInteractor) return;
            Animator playerAnimator = currentInteractor.GetComponent<Animator>();
            if (playerAnimator)
            {
                playerAnimator.SetTrigger("EndDialogue");
            }
            currentInteractor = null;
        }

        public bool CanInteract(GameObject interactor)
        {
            if (DialogueManager.Instance && DialogueManager.Instance.IsActive)
            {
                return false;
            }

            float distance = Vector3.Distance(transform.position, interactor.transform.position);
            return distance <= interactionRange;
        }

        public Transform GetTransform()
        {
            return cameraLookTarget ? cameraLookTarget : transform;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);

            if (cameraLookTarget && cameraLookTarget != transform)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawWireSphere(cameraLookTarget.position, 0.2f);
                Gizmos.DrawLine(transform.position, cameraLookTarget.position);
            }
        }
    }
}