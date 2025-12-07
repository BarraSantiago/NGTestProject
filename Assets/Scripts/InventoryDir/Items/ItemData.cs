using UnityEngine;

namespace InventoryDir.Items
{
    [CreateAssetMenu(menuName = "Items/ItemData")]
    public class ItemData : ScriptableObject
    {
        [SerializeField, HideInInspector]
        private string guid;

        public string id => guid;
        public string displayName;
        public string description;
        public Sprite icon;
        public bool isConsumable;
        public bool isEquippable;
        public int maxStack = 99;

        [Header("Effects (for consumables)")]
        public ItemEffect[] effects;

        /// <summary>
        /// Generates a non-changeable ID for the item when created.
        /// </summary>
        private void OnValidate()
        {
            if (!string.IsNullOrEmpty(guid)) return;
            guid = System.Guid.NewGuid().ToString();
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}