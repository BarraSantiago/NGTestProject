using System;
using System.Collections.Generic;
using InventoryDir.Items;
using UnityEngine;

namespace InventoryDir
{
    public class Inventory : MonoBehaviour
    {
        [SerializeField] private List<ItemStack> slots = new();

        [Header("Config")] public int size = 20;
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

        public void LoadFromSlots(List<ItemStack> loaded)
        {
            slots = new List<ItemStack>();

            for (int i = 0; i < size; i++)
            {
                if (loaded != null && i < loaded.Count)
                    slots.Add(loaded[i]);
                else
                    slots.Add(new ItemStack());
            }

            OnInventoryChanged?.Invoke();
        }

        private int GetMaxStack(string itemId)
        {
            ItemData data = GetItemData(itemId);
            return data ? Mathf.Max(1, data.maxStack) : 99;
        }

        public bool AddItem(string itemId, int count = 1)
        {
            if (string.IsNullOrWhiteSpace(itemId)) return false;
            if (count < 1) count = 1;

            int remaining = count;
            int maxStack = GetMaxStack(itemId);

            // Merge into existing stacks
            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                ItemStack slot = slots[i];

                if (slot.IsEmpty || slot.itemId != itemId) continue;
                if (slot.count >= maxStack) continue;

                int canAdd = Mathf.Min(maxStack - slot.count, remaining);
                slot.count += canAdd;
                remaining -= canAdd;

                slots[i] = slot;
            }

            // Place into empty slots
            for (int i = 0; i < slots.Count && remaining > 0; i++)
            {
                if (!slots[i].IsEmpty) continue;

                int put = Mathf.Min(maxStack, remaining);
                slots[i] = new ItemStack(itemId, put);
                remaining -= put;
            }

            OnInventoryChanged?.Invoke();
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

            if (to.IsEmpty)
            {
                slots[toIndex] = from;
                slots[fromIndex] = new ItemStack();
                OnInventoryChanged?.Invoke();
                return;
            }

            if (!to.IsEmpty && to.itemId == from.itemId)
            {
                int maxStack = GetMaxStack(from.itemId);
                int space = maxStack - to.count;

                if (space > 0)
                {
                    int transfer = Mathf.Min(space, from.count);
                    to.count += transfer;
                    from.count -= transfer;

                    slots[toIndex] = to;
                    slots[fromIndex] = from.count > 0 ? from : new ItemStack();

                    OnInventoryChanged?.Invoke();
                    return;
                }
            }

            // otherwise swap
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