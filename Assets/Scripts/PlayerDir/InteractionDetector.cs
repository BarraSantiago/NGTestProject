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
        [SerializeField] private InteractionPromptUI promptUI;
        [SerializeField] private string interactionKey = "E";

        private IInteractable currentInteractable;
        private List<IInteractable> nearbyInteractables = new();
        private IInteractable previousInteractable;

        public IInteractable CurrentInteractable => currentInteractable;
        public bool HasInteractable => currentInteractable != null;

        private void Awake()
        {
            if (!promptUI)
            {
                promptUI = FindFirstObjectByType<InteractionPromptUI>();
            }
        }

        private void Update()
        {
            DetectInteractables();
            UpdatePromptUI();
        }

        private void DetectInteractables()
        {
            nearbyInteractables.Clear();
            Collider[] colliders = Physics.OverlapSphere(transform.position, detectionRadius, interactableLayer);

            foreach (Collider col in colliders)
            {
                IInteractable interactable = col.GetComponent<IInteractable>();
                if (interactable != null && interactable.CanInteract(gameObject))
                {
                    Vector3 directionToInteractable = (interactable.GetTransform().position - transform.position).normalized;
                    float angle = Vector3.Angle(transform.forward, directionToInteractable);

                    if (angle <= detectionAngle)
                    {
                        nearbyInteractables.Add(interactable);
                    }
                }
            }

            previousInteractable = currentInteractable;
            currentInteractable = nearbyInteractables
                .OrderBy(i => Vector3.Distance(transform.position, i.GetTransform().position))
                .FirstOrDefault();
        }

        private void UpdatePromptUI()
        {
            if (!promptUI) return;

            if (currentInteractable != null)
            {
                bool isNewInteractable = currentInteractable != previousInteractable;
                string prompt = currentInteractable.GetInteractionPrompt();
                
                if (isNewInteractable)
                {
                    promptUI.Show(prompt, currentInteractable.GetTransform(), interactionKey);
                }
                else
                {
                    promptUI.UpdatePrompt(prompt);
                }
            }
            else
            {
                promptUI.Hide();
            }
        }

        public void Interact()
        {
            currentInteractable?.Interact(gameObject);
        }

        private void OnDisable()
        {
            if (promptUI) promptUI.Hide();
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