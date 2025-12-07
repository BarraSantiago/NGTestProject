using UnityEngine;

namespace Interactables
{
    public class InteractableNPC : MonoBehaviour, IInteractable
    {
        [Header("NPC Settings")]
        [SerializeField] private string npcName = "NPC";
        [SerializeField, TextArea(3, 10)] private string[] dialogueLines;
        [SerializeField] private float interactionRange = 3f;

        private int currentDialogueIndex;

        public string GetInteractionPrompt()
        {
            return $"Talk to {npcName}";
        }

        public void Interact(GameObject interactor)
        {
            if (dialogueLines == null || dialogueLines.Length == 0)
            {
                Debug.Log($"{npcName}: I have nothing to say.");
                return;
            }

            string dialogue = dialogueLines[currentDialogueIndex];
            Debug.Log($"{npcName}: {dialogue}");

            // TODO: Show dialogue UI here when implemented
            // DialogueManager.Instance.ShowDialogue(npcName, dialogue);

            currentDialogueIndex = (currentDialogueIndex + 1) % dialogueLines.Length;
        }

        public bool CanInteract(GameObject interactor)
        {
            float distance = Vector3.Distance(transform.position, interactor.transform.position);
            return distance <= interactionRange;
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}