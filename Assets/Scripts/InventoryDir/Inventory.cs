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

        public int maxStackSize = 99;

        public ItemDatabase itemDatabase;

        public event Action OnInventoryChanged;

        private void Awake()
        {
            slots ??= new List<ItemStack>();

            while (slots.Count < size)
                slots.Add(new ItemStack());

            if (slots.Count > size)
                slots.RemoveRange(size, slots.Count - size);
        }

        public List<ItemStack> GetSlots()
        {
            return new List<ItemStack>(slots);
        }

        public ItemStack GetSlot(int index)
        {
            if (!ValidIndex(index)) return new ItemStack();
            return slots[index];
        }

        public ItemData GetItemData(string id)
        {
            return itemDatabase.items.Find(it => it && it.id == id);
        }

        public bool AddItem(string itemId, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            if (count < 1) count = 1;

            int remaining = count;

            // Fill existing stacks
            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                ItemStack s = slots[i];
                if (s.IsEmpty) continue;
                if (s.itemId != itemId) continue;

                int space = maxStackSize - s.count;
                if (space <= 0) continue;

                int toAdd = Mathf.Min(space, remaining);
                s.count += toAdd;
                remaining -= toAdd;

                slots[i] = s;
            }

            // Fill empty slots
            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty) continue;

                int toAdd = Mathf.Min(maxStackSize, remaining);
                slots[i] = new ItemStack(itemId, toAdd);
                remaining -= toAdd;
            }

            OnInventoryChanged?.Invoke();

            // true only if we added everything requested
            return remaining == 0;
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

            if (from.IsEmpty) return;

            // If destination empty -> move
            if (to.IsEmpty)
            {
                slots[toIndex] = from;
                slots[fromIndex] = new ItemStack();
                OnInventoryChanged?.Invoke();
                return;
            }

            // If same item -> merge
            if (to.itemId == from.itemId)
            {
                int space = maxStackSize - to.count;
                if (space > 0)
                {
                    int transfer = Mathf.Min(space, from.count);
                    to.count += transfer;
                    from.count -= transfer;

                    slots[toIndex] = to;
                    slots[fromIndex] = (from.count <= 0) ? new ItemStack() : from;

                    OnInventoryChanged?.Invoke();
                    return;
                }
            }

            // Otherwise swap
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
