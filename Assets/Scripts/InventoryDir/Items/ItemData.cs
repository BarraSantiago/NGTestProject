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

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(guid))
            {
                guid = System.Guid.NewGuid().ToString();
                #if UNITY_EDITOR
                UnityEditor.EditorUtility.SetDirty(this);
                #endif
            }
        }
    }
}