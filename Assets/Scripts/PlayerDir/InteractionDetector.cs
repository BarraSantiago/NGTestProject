using UnityEngine;
using Interactables;
using System.Collections.Generic;
using System.Linq;
using View;

namespace PlayerDir
{
    public class InteractionDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField] private float detectionRadius = 3f;
        [SerializeField] private LayerMask interactableLayer = -1;
        [SerializeField] private float detectionAngle = 60f;

        [Header("UI")]
        [SerializeField] private InteractionPromptUI promptUIPrefab;
        [SerializeField] private string interactionKey = "E";

        public static InteractableNPC CurrentNPC { get; private set; }
        private IInteractable currentInteractable;
        private List<IInteractable> nearbyInteractables = new();
        private Dictionary<IInteractable, InteractionPromptUI> activePrompts = new();

        public IInteractable CurrentInteractable => currentInteractable;
        public bool HasInteractable => currentInteractable != null;

        private void Update()
        {
            DetectInteractables();
            UpdatePrompts();
        }

        private void DetectInteractables()
        {
            nearbyInteractables.Clear();
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);

            foreach (Collider col in colliders)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable == null || !interactable.CanInteract(gameObject)) continue;
                Vector3 directionToInteractable = (interactable.GetTransform().position - transform.position).normalized;
                float angle = Vector3.Angle(transform.forward, directionToInteractable);

                if (angle <= detectionAngle)
                {
                    nearbyInteractables.Add(interactable);
                }
            }

            currentInteractable = nearbyInteractables
                .OrderBy(i => Vector3.Distance(transform.position, i.GetTransform().position))
                .FirstOrDefault();
        }

        private void UpdatePrompts()
        {
            // Get interactables that are no longer in range
            List<IInteractable> toRemove = new();
            foreach (KeyValuePair<IInteractable, InteractionPromptUI> kvp in activePrompts)
            {
                if (!nearbyInteractables.Contains(kvp.Key))
                {
                    kvp.Value.Hide();
                    toRemove.Add(kvp.Key);
                }
            }

            // Clean up prompts after a delay
            foreach (IInteractable interactable in toRemove.ToList())
            {
                InteractionPromptUI prompt = activePrompts[interactable];
                if (prompt && prompt.gameObject.activeSelf == false)
                {
                    Destroy(prompt.gameObject);
                    activePrompts.Remove(interactable);
                }
            }

            // Show or update prompts for nearby interactables
            foreach (IInteractable interactable in nearbyInteractables)
            {
                if (!activePrompts.TryGetValue(interactable, out InteractionPromptUI prompt))
                {
                    CreatePromptFor(interactable);
                }
                else
                {
                    if (prompt.HasBeenInteracted) continue;
                    
                    bool isClosest = interactable == currentInteractable;
                    prompt.Show(interactable.GetInteractionPrompt(), interactable.GetTransform(), 
                        interactionKey, isClosest);
                }
            }
        }

        private void CreatePromptFor(IInteractable interactable)
        {
            if (!promptUIPrefab) return;

            InteractionPromptUI prompt = Instantiate(promptUIPrefab);
            bool isClosest = interactable == currentInteractable;
            prompt.Show(interactable.GetInteractionPrompt(), interactable.GetTransform(), 
                interactionKey, isClosest);
            
            activePrompts[interactable] = prompt;
        }

        public void Interact()
        {
            if (currentInteractable == null) return;
            currentInteractable.Interact(gameObject);
            
            if (currentInteractable is InteractableNPC npc)
            {
                CurrentNPC = npc;
            }

            if (!activePrompts.TryGetValue(currentInteractable, out InteractionPromptUI prompt)) return;
            prompt.OnInteracted();
            Destroy(prompt.gameObject, 0.5f);
            activePrompts.Remove(currentInteractable);
        }

        private void OnDisable()
        {
            foreach (InteractionPromptUI prompt in activePrompts.Values)
            {
                if (prompt) prompt.Hide();
            }
        }

        private void OnDestroy()
        {
            foreach (InteractionPromptUI prompt in activePrompts.Values)
            {
                if (prompt) Destroy(prompt.gameObject);
            }
            activePrompts.Clear();
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, detectionRadius);

            Vector3 forward = transform.forward * detectionRadius;
            Vector3 right = Quaternion.Euler(0, detectionAngle, 0) * forward;
            Vector3 left = Quaternion.Euler(0, -detectionAngle, 0) * forward;

            Gizmos.DrawLine(transform.position, transform.position + right);
            Gizmos.DrawLine(transform.position, transform.position + left);
        }
    }
}