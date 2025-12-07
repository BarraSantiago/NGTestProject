using InventoryDir;
using InventoryDir.Items;
using UnityEngine;

namespace Interactables
{
    public class InteractableItem : MonoBehaviour, IInteractable
    {
        [Header("Item Settings")]
        [SerializeField] private ItemData itemData;
        [SerializeField] private int itemCount = 1;
        [SerializeField] private float interactionRange = 2f;
        [SerializeField] private bool destroyOnPickup = true;

        public string GetInteractionPrompt()
        {
            if (!itemData) return "Pick up Item";
            
            string name = itemData.displayName;
            return itemCount > 1 ? $"Pick up {name} (x{itemCount})" : $"Pick up {name}";
        }

        public void Interact(GameObject interactor)
        {
            if (!itemData)
            {
                Debug.LogWarning("No ItemData assigned to InteractableItem!");
                return;
            }

            Inventory inventory = interactor.GetComponent<Inventory>();
            if (!inventory)
            {
                inventory = FindAnyObjectByType<Inventory>();
            }

            if (!inventory)
            {
                Debug.LogWarning("No inventory found on interactor!");
                return;
            }

            bool added = inventory.AddItem(itemData.id, itemCount);
            if (added)
            {
                Debug.Log($"Picked up {itemData.displayName} (x{itemCount})");
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
            return distance <= interactionRange && itemData;
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

        private void OnValidate()
        {
            // Auto-update visual representation when ItemData changes
            if (!itemData || !itemData.icon) return;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer)
            {
                spriteRenderer.sprite = itemData.icon;
            }
        }
    }
}