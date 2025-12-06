using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using InventoryDir;
using InventoryDir.Items;

namespace View
{
    public class InventoryDebugMenu : MonoBehaviour
    {
        [Header("References")]
        public Inventory inventory;
        public TMP_Dropdown itemDropdown;
        public TMP_InputField countInput;
        public Button addItemButton;
        public Button removeAllButton;
        public Button removeOneButton;
        public Button clearInventoryButton;
        public Button fillInventoryButton;
        public Button saveButton;
        public Button loadButton;
        public TMP_Text statusText;

        private List<ItemData> availableItems = new();

        private void Awake()
        {
            if (!inventory) inventory = FindFirstObjectByType<Inventory>();
        }

        private void Start()
        {
            PopulateItemDropdown();
            SetupButtons();
            
            if (countInput)
                countInput.text = "1";
        }

        private void PopulateItemDropdown()
        {
            if (!itemDropdown || !inventory || !inventory.itemDatabase) return;

            availableItems = inventory.itemDatabase.items;
            itemDropdown.ClearOptions();

            List<string> options = availableItems.Select(item => item.displayName).ToList();
            itemDropdown.AddOptions(options);
        }

        private void SetupButtons()
        {
            if (addItemButton)
                addItemButton.onClick.AddListener(OnAddItem);

            if (removeAllButton)
                removeAllButton.onClick.AddListener(OnRemoveAll);

            if (removeOneButton)
                removeOneButton.onClick.AddListener(OnRemoveOne);

            if (clearInventoryButton)
                clearInventoryButton.onClick.AddListener(OnClearInventory);

            if (fillInventoryButton)
                fillInventoryButton.onClick.AddListener(OnFillInventory);

            if (saveButton)
                saveButton.onClick.AddListener(OnSave);

            if (loadButton)
                loadButton.onClick.AddListener(OnLoad);
        }

        private void OnAddItem()
        {
            if (!inventory || availableItems.Count == 0) return;

            int selectedIndex = itemDropdown.value;
            if (selectedIndex < 0 || selectedIndex >= availableItems.Count)
            {
                ShowStatus("Invalid item selected", Color.red);
                return;
            }

            ItemData selectedItem = availableItems[selectedIndex];
            int count = GetInputCount();

            if (count <= 0)
            {
                ShowStatus("Invalid count", Color.red);
                return;
            }

            bool success = inventory.AddItem(selectedItem.id, count);
            ShowStatus(success 
                ? $"Added {count}x {selectedItem.displayName}" 
                : $"Failed to add {selectedItem.displayName} (inventory full)", 
                success ? Color.green : Color.yellow);
        }

        private void OnRemoveAll()
        {
            if (!inventory || availableItems.Count == 0) return;

            int selectedIndex = itemDropdown.value;
            if (selectedIndex < 0 || selectedIndex >= availableItems.Count) return;

            ItemData selectedItem = availableItems[selectedIndex];
            int removed = 0;

            for (int i = 0; i < inventory.GetSlots().Count; i++)
            {
                ItemStack slot = inventory.GetSlot(i);
                if (slot.itemId == selectedItem.id)
                {
                    removed += slot.count;
                    inventory.RemoveItemAt(i, slot.count);
                }
            }

            ShowStatus($"Removed {removed}x {selectedItem.displayName}", Color.green);
        }

        private void OnRemoveOne()
        {
            if (!inventory || availableItems.Count == 0) return;

            int selectedIndex = itemDropdown.value;
            if (selectedIndex < 0 || selectedIndex >= availableItems.Count) return;

            ItemData selectedItem = availableItems[selectedIndex];

            for (int i = 0; i < inventory.GetSlots().Count; i++)
            {
                ItemStack slot = inventory.GetSlot(i);
                if (slot.itemId == selectedItem.id)
                {
                    inventory.RemoveItemAt(i, 1);
                    ShowStatus($"Removed 1x {selectedItem.displayName}", Color.green);
                    return;
                }
            }

            ShowStatus($"No {selectedItem.displayName} found", Color.yellow);
        }

        private void OnClearInventory()
        {
            if (!inventory) return;

            List<ItemStack> emptySlots = new();
            for (int i = 0; i < inventory.size; i++)
            {
                emptySlots.Add(new ItemStack());
            }

            inventory.LoadFromSlots(emptySlots);
            ShowStatus("Inventory cleared", Color.green);
        }

        private void OnFillInventory()
        {
            if (!inventory || availableItems.Count == 0) return;

            int itemIndex = 0;
            List<ItemStack> slots = inventory.GetSlots();

            for (int i = 0; i < slots.Count && itemIndex < availableItems.Count; i++)
            {
                ItemData item = availableItems[itemIndex];
                int maxStack = Mathf.Max(1, item.maxStack);
                
                slots[i] = new ItemStack(item.id, maxStack);
                itemIndex = (itemIndex + 1) % availableItems.Count;
            }

            inventory.LoadFromSlots(slots);
            ShowStatus("Inventory filled with items", Color.green);
        }

        private void OnSave()
        {
            if (!inventory) return;
            
            inventory.SaveInventory();
            ShowStatus("Inventory saved", Color.green);
        }

        private void OnLoad()
        {
            if (!inventory) return;
            
            PersistenceSystem.PersistenceManager.LoadInventory(inventory);
            ShowStatus("Inventory loaded", Color.green);
        }

        private int GetInputCount()
        {
            if (!countInput) return 1;
            
            if (int.TryParse(countInput.text, out int count))
            {
                return Mathf.Clamp(count, 1, 99);
            }

            return 1;
        }

        private void ShowStatus(string message, Color color)
        {
            if (!statusText) return;

            statusText.text = message;
            statusText.color = color;
            CancelInvoke(nameof(ClearStatus));
            Invoke(nameof(ClearStatus), 3f);
        }

        private void ClearStatus()
        {
            if (statusText) statusText.text = "";
        }

        private void OnDestroy()
        {
            if (addItemButton) addItemButton.onClick.RemoveListener(OnAddItem);
            if (removeAllButton) removeAllButton.onClick.RemoveListener(OnRemoveAll);
            if (removeOneButton) removeOneButton.onClick.RemoveListener(OnRemoveOne);
            if (clearInventoryButton) clearInventoryButton.onClick.RemoveListener(OnClearInventory);
            if (fillInventoryButton) fillInventoryButton.onClick.RemoveListener(OnFillInventory);
            if (saveButton) saveButton.onClick.RemoveListener(OnSave);
            if (loadButton) loadButton.onClick.RemoveListener(OnLoad);
        }
    }
}