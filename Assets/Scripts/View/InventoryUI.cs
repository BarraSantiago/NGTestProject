using System.Collections.Generic;
using UnityEngine;
using InventoryDir;
using InventoryDir.Items;

namespace View
{
    public class InventoryUI : MonoBehaviour
    {
        public Inventory inventory;
        public GameObject slotPrefab;
        public Transform slotsParent;

        public TooltipUI tooltip;

        private readonly List<InventorySlot> _slotComponents = new();

        private void Awake()
        {
            if (!inventory)
                inventory = FindFirstObjectByType<Inventory>();
        }

        private void Start()
        {
            BuildSlots();

            if (inventory)
                inventory.OnInventoryChanged += RefreshUI;

            RefreshUI();
        }

        private void OnDestroy()
        {
            if (inventory)
                inventory.OnInventoryChanged -= RefreshUI;
        }

        private void BuildSlots()
        {
            if (!slotPrefab || !slotsParent || !inventory) return;

            foreach (Transform t in slotsParent)
                Destroy(t.gameObject);

            _slotComponents.Clear();

            List<ItemStack> slots = inventory.GetSlots();

            for (int i = 0; i < slots.Count; i++)
            {
                GameObject go = Instantiate(slotPrefab, slotsParent);

                InventorySlot slotComp = go.GetComponent<InventorySlot>();
                if (!slotComp) slotComp = go.AddComponent<InventorySlot>();

                slotComp.Setup(inventory, i, this);
                _slotComponents.Add(slotComp);
            }
        }

        public void RefreshUI()
        {
            if (!inventory || _slotComponents.Count == 0) return;

            List<ItemStack> slots = inventory.GetSlots();

            for (int i = 0; i < _slotComponents.Count; i++)
            {
                _slotComponents[i].SetSlotData(slots[i]);
            }
        }

        public void ShowTooltip(string title, string description, Sprite icon, Vector2 position)
        {
            if (!tooltip) return;
            tooltip.Show(title, description, icon, position);
        }

        public void HideTooltip()
        {
            if (!tooltip) return;
            tooltip.Hide();
        }
    }
}
