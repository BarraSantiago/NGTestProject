#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Interactables;
using InventoryDir.Items;

namespace EditorDir
{
    [CustomEditor(typeof(InteractableItem))]
    public class InteractableItemEditor : Editor
    {
        private SerializedProperty itemDataProp;
        private SerializedProperty itemCountProp;
        private SerializedProperty interactionRangeProp;
        private SerializedProperty destroyOnPickupProp;

        private void OnEnable()
        {
            itemDataProp = serializedObject.FindProperty("itemData");
            itemCountProp = serializedObject.FindProperty("itemCount");
            interactionRangeProp = serializedObject.FindProperty("interactionRange");
            destroyOnPickupProp = serializedObject.FindProperty("destroyOnPickup");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.LabelField("Item Settings", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            // ItemData field with preview
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(itemDataProp, new GUIContent("Item Data"));
            
            ItemData itemData = itemDataProp.objectReferenceValue as ItemData;
            if (itemData)
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                
                EditorGUILayout.LabelField("Preview", EditorStyles.miniLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (itemData.icon)
                {
                    GUILayout.Label(itemData.icon.texture, GUILayout.Width(64), GUILayout.Height(64));
                }
                
                EditorGUILayout.BeginVertical();
                EditorGUILayout.LabelField("Name:", itemData.displayName, EditorStyles.wordWrappedLabel);
                EditorGUILayout.LabelField("Type:", GetItemType(itemData), EditorStyles.miniLabel);
                EditorGUILayout.LabelField("Max Stack:", itemData.maxStack.ToString(), EditorStyles.miniLabel);
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndHorizontal();
                
                if (!string.IsNullOrEmpty(itemData.description))
                {
                    EditorGUILayout.Space(3);
                    EditorGUILayout.LabelField("Description:", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField(itemData.description, EditorStyles.wordWrappedMiniLabel);
                }
                
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("Assign an ItemData to configure this interactable item.", MessageType.Warning);
            }

            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            EditorGUILayout.Space(5);

            // Item count
            EditorGUILayout.PropertyField(itemCountProp, new GUIContent("Item Count"));
            if (itemData && itemCountProp.intValue > itemData.maxStack)
            {
                EditorGUILayout.HelpBox($"Count exceeds max stack ({itemData.maxStack}). Will be added as multiple stacks.", MessageType.Info);
            }

            EditorGUILayout.Space(5);
            EditorGUILayout.LabelField("Interaction Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(interactionRangeProp, new GUIContent("Interaction Range"));
            EditorGUILayout.PropertyField(destroyOnPickupProp, new GUIContent("Destroy On Pickup"));

            serializedObject.ApplyModifiedProperties();

            // Helper buttons
            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();
            
            if (itemData && GUILayout.Button("Edit Item Data"))
            {
                Selection.activeObject = itemData;
            }
            
            if (GUILayout.Button("Quick Setup Visual"))
            {
                QuickSetupVisual();
            }
            
            EditorGUILayout.EndHorizontal();
        }

        private string GetItemType(ItemData itemData)
        {
            if (itemData.isConsumable && itemData.isEquippable)
                return "Consumable & Equippable";
            if (itemData.isConsumable)
                return "Consumable";
            if (itemData.isEquippable)
                return "Equippable";
            return "Generic";
        }

        private void QuickSetupVisual()
        {
            InteractableItem item = target as InteractableItem;
            if (!item) return;

            ItemData itemData = itemDataProp.objectReferenceValue as ItemData;
            if (!itemData || !itemData.icon) return;

            SpriteRenderer spriteRenderer = item.GetComponent<SpriteRenderer>();
            if (!spriteRenderer)
            {
                spriteRenderer = item.gameObject.AddComponent<SpriteRenderer>();
            }

            spriteRenderer.sprite = itemData.icon;
            
            // Add collider if missing
            if (!item.GetComponent<Collider>())
            {
                BoxCollider collider = item.gameObject.AddComponent<BoxCollider>();
                collider.isTrigger = false;
                collider.size = Vector3.one;
            }

            EditorUtility.SetDirty(item);
            EditorUtility.SetDirty(spriteRenderer);
        }
    }
}
#endif