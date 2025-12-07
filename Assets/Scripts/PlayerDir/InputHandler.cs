using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using View;

namespace PlayerDir
{
    public class InputHandler : MonoBehaviour
    {
        [SerializeField] private InventoryUI inventoryUI;
        [SerializeField] private InteractionDetector interactionDetector;
        
        private StarterAssetsInputs inputs;

        private void Awake()
        {
            inputs = GetComponent<StarterAssetsInputs>();
            if (!inventoryUI)
            {
                inventoryUI = FindFirstObjectByType<InventoryUI>();
            }
            if (!interactionDetector)
            {
                interactionDetector = GetComponent<InteractionDetector>();
            }
        }

        private void OnInventory(InputValue value)
        {
            if (!value.isPressed) return;
            bool isInventoryOpen = inventoryUI.ToggleInventory();

            if(inputs) inputs.UpdateCursorState(!isInventoryOpen);
        }

        private void OnInteract(InputValue value)
        {
            if (!value.isPressed) return;

            if (interactionDetector && interactionDetector.HasInteractable)
            {
                interactionDetector.Interact();
            }
        }
    }
}