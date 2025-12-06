using System.Collections.Generic;
using UnityEngine;

namespace InventoryDir.Items
{
    [CreateAssetMenu(menuName = "Items/ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<ItemData> items = new();
    }
}