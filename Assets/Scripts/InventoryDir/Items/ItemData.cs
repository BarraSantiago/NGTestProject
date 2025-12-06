using UnityEngine;

namespace InventoryDir.Items
{
    [CreateAssetMenu(menuName = "Items/ItemData")]
    public class ItemData : ScriptableObject
    {
        public string id;
        public string displayName;
        public string description;
        public Sprite icon;
        public bool isConsumable;
        public bool isEquippable;
        public int maxStack = 99;
    }
}