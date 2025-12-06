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
            while (slots.Count < size) slots.Add(new ItemStack());
            PersistenceSystem.PersistenceManager.LoadInventory(this);
        }

        private void Start()
        {
            OnInventoryChanged?.Invoke();
        }

        public List<ItemStack> GetSlots()
        {
            return new List<ItemStack>(slots);
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
        
        public void SaveInventory()
        {
            PersistenceSystem.PersistenceManager.SaveInventory(this);
        }

        public ItemData GetItemData(string id)
        {
            return itemDatabase.items.Find(it => it.id == id);
        }

        public ItemStack GetSlot(int index)
        {
            if (index < 0 || index >= slots.Count) return new ItemStack();
            return slots[index];
        }

        public bool AddItem(string itemId, int count = 1)
        {
            ItemData data = GetItemData(itemId);
            int maxStack = data ? Mathf.Max(1, data.maxStack) : 99;

            // try to merge into existing stacks
            for (int i = 0; i < slots.Count; i++)
            {
                ItemStack slot = slots[i];
                if (slot.IsEmpty || slot.itemId != itemId || slot.count >= maxStack) continue;

                int canAdd = Mathf.Min(maxStack - slot.count, count);
                slot.count += canAdd;
                count -= canAdd;
                slots[i] = slot;

                if (count > 0) continue;
                OnInventoryChanged?.Invoke();
                return true;
            }

            // place into empty slots
            for (int i = 0; i < slots.Count; i++)
            {
                if (!slots[i].IsEmpty) continue;

                int put = Mathf.Min(maxStack, count);
                slots[i] = new ItemStack(itemId, put);
                count -= put;

                if (count > 0) continue;
                OnInventoryChanged?.Invoke();
                return true;
            }

            OnInventoryChanged?.Invoke();
            // return true if some or all were added
            return count < 1;
        }

        public bool RemoveItemAt(int index, int count = 1)
        {
            if (index < 0 || index >= slots.Count) return false;
            ItemStack s = slots[index];

            if (s.IsEmpty) return false;
            s.count -= count;

            if (s.count <= 0) s = new ItemStack();
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

            // try stacking
            if (!to.IsEmpty && to.itemId == from.itemId)
            {
                ItemData data = GetItemData(from.itemId);
                int maxStack = data ? Mathf.Max(1, data.maxStack) : 99;
                int canMove = Mathf.Min(from.count, maxStack - to.count);
                if (canMove > 0)
                {
                    to.count += canMove;
                    from.count -= canMove;
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
                // example: equipping could be toggled
                Debug.Log($"Equipped: {data.displayName}");
            }
        }

        private bool ValidIndex(int i) => i >= 0 && i < slots.Count;
    }
}