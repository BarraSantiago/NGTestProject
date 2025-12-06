#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using InventoryDir.Items;

namespace EditorDir
{
    [CustomEditor(typeof(ItemData))]
    public class ItemDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            ItemData item = (ItemData)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Item ID (Auto-Generated)", EditorStyles.boldLabel);
            EditorGUILayout.SelectableLabel(item.id, EditorStyles.textField, GUILayout.Height(18));
            
            if (GUILayout.Button("Copy ID to Clipboard"))
            {
                EditorGUIUtility.systemCopyBuffer = item.id;
                Debug.Log($"Copied ID: {item.id}");
            }

            EditorGUILayout.Space();
            DrawDefaultInspector();
        }
    }
}
#endif