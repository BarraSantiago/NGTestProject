using InventoryDir;
using UnityEngine;

namespace Interactables
{
    public class InteractableItem : MonoBehaviour, IInteractable
    {
        [Header("Item Settings")]
        [SerializeField] private string itemId;
        [SerializeField] private int itemCount = 1;
        [SerializeField] private string displayName;
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private bool destroyOnPickup = true;

        public string GetInteractionPrompt()
        {
            string name = !string.IsNullOrEmpty(displayName) ? displayName : "Item";
            return itemCount > 1 ? $"Pick up {name} (x{itemCount})" : $"Pick up {name}";
        }

        public void Interact(GameObject interactor)
        {
            Inventory inventory = interactor.GetComponent<Inventory>();
            if (!inventory)
            {
                inventory = interactor.GetComponentInChildren<Inventory>();
            }

            if (!inventory)
            {
                Debug.LogWarning("No inventory found on interactor!");
                return;
            }

            bool added = inventory.AddItem(itemId, itemCount);
            if (added)
            {
                Debug.Log($"Picked up {displayName} (x{itemCount})");
                if (destroyOnPickup)
                {
                    Destroy(gameObject);
                }
            }
            else
            {
                Debug.Log("Inventory full!");
            }
        }

        public bool CanInteract(GameObject interactor)
        {
            float distance = Vector3.Distance(transform.position, interactor.transform.position);
            return distance <= interactionRange && !string.IsNullOrEmpty(itemId);
        }

        public Transform GetTransform()
        {
            return transform;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, interactionRange);
        }
    }
}