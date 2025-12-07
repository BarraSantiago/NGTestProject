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
        [SerializeField] private Transform cameraLookTarget;

        private int currentDialogueIndex;

        private void Awake()
        {
            if (!cameraLookTarget)
            {
                cameraLookTarget = transform;
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