using System.Collections.Generic;
using System.IO;
using UnityEngine;
using InventoryDir;
using InventoryDir.Items;

namespace PersistenceSystem
{
    [DefaultExecutionOrder(-100)]
    public static class PersistenceManager
    {
        private static string SavePath => Path.Combine(Application.persistentDataPath, "inventory.json");

        public static void SaveInventory(Inventory inv)
        {
            if (!inv) return;

            InventorySaveWrapper wrapper = new InventorySaveWrapper { slots = inv.GetSlots() };
            string json = JsonUtility.ToJson(wrapper, true);
            
            try
            {
                File.WriteAllText(SavePath, json);
                Debug.Log($"Inventory saved to: {SavePath}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to save inventory: {e.Message}");
            }
        }

        public static void LoadInventory(Inventory inv)
        {
            if (!inv) return;
            if (!File.Exists(SavePath))
            {
                Debug.Log("No save file found.");
                return;
            }

            try
            {
                string json = File.ReadAllText(SavePath);
                InventorySaveWrapper wrapper = JsonUtility.FromJson<InventorySaveWrapper>(json);

                if (wrapper?.slots == null) return;

                inv.LoadFromSlots(wrapper.slots);
                Debug.Log("Inventory loaded from file.");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load inventory: {e.Message}");
            }
        }

        [System.Serializable]
        private class InventorySaveWrapper
        {
            public List<ItemStack> slots;
        }
    }
}