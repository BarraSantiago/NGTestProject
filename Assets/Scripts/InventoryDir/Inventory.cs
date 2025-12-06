using System;
using System.Collections.Generic;
using InventoryDir.Items;
using UnityEngine;

namespace InventoryDir
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private List<ItemStack> slots = new();

        [Header("Config")]
        public int size = 20;
        public ItemDatabase itemDatabase;

        public event Action OnInventoryChanged;

        private void Awake()
        {
            slots ??= new List<ItemStack>();

            // Ensure fixed size
            while (slots.Count < size)
            {
                slots.Add(new ItemStack());
            }

            if (slots.Count > size)
            {
                slots.RemoveRange(size, slots.Count - size);
            }
        }

        public List<ItemStack> GetSlots()
        {
            return new List<ItemStack>(slots);
        }

        public ItemStack GetSlot(int index)
        {
            return !ValidIndex(index) ? new ItemStack() : slots[index];
        }

        public ItemData GetItemData(string id)
        {
            return itemDatabase.items.Find(it => it && it.id == id);
        }

        public bool AddItem(string itemId, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            if (count < 1) count = 1;

            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty) continue;

                slots[i] = new ItemStack(itemId, count);
                OnInventoryChanged?.Invoke();
                return true;
            }

            // Inventory full
            OnInventoryChanged?.Invoke();
            return false;
        }

        public bool RemoveItemAt(int index, int count = 1)
        {
            if (!ValidIndex(index)) return false;

            ItemStack s = slots[index];
            if (s.IsEmpty) return false;

            if (count < 1) count = 1;

            s.count -= count;
            if (s.count <= 0)
                s = new ItemStack();

            slots[index] = s;
            OnInventoryChanged?.Invoke();
            return true;
        }

        public void MoveItem(int fromIndex, int toIndex)
        {
            if (fromIndex == toIndex) return;
            if (!ValidIndex(fromIndex) || !ValidIndex(toIndex)) return;

            ItemStack from = slots[fromIndex];
            ItemStack to = slots[toIndex];

            slots[toIndex] = from;
            slots[fromIndex] = to;

            OnInventoryChanged?.Invoke();
        }

        public void UseItem(int index)
        {
            if (!ValidIndex(index)) return;

            ItemStack s = slots[index];
            if (s.IsEmpty) return;

            ItemData data = GetItemData(s.itemId);
            if (!data) return;

            if (data.isConsumable)
            {
                RemoveItemAt(index, 1);
                // TODO: Effect system
            }
            else if (data.isEquippable)
            {
                Debug.Log($"Equipped: {data.displayName}");
            }
        }

        private bool ValidIndex(int i) => i >= 0 && i < slots.Count;
    }
}
