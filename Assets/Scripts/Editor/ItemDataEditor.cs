using UnityEngine;
using UnityEditor;
using InventoryDir.Items;
using System.Collections.Generic;
using System.Linq;

namespace EditorDir
{
    public class ItemDataEditor : EditorWindow
    {
        private enum EditorMode
        {
            Create,
            Modify
        }

        private EditorMode mode = EditorMode.Create;
        private Vector2 scrollPosition;
        private Vector2 itemListScroll;

        // Create mode fields
        private string newDisplayName = "";
        private string newDescription = "";
        private Sprite newIcon;
        private bool newIsConsumable;
        private bool newIsEquippable;
        private int newMaxStack = 99;
        private List<ItemEffect> newEffects = new();
        private string savePath = "Assets/ScriptableObjects/Items";

        // Modify mode fields
        private List<ItemData> allItems = new();
        private ItemData selectedItem;
        private string searchFilter = "";
        private bool showConsumableOnly;
        private bool showEquippableOnly;

        // Effect editing
        private bool showEffectFoldout = true;

        [MenuItem("Tools/Item Data Editor")]
        public static void ShowWindow()
        {
            ItemDataEditor window = GetWindow<ItemDataEditor>("Item Data Editor");
            window.minSize = new Vector2(600, 400);
            window.Show();
        }

        private void OnEnable()
        {
            RefreshItemList();
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            if (GUILayout.Toggle(mode == EditorMode.Create, "Create New", EditorStyles.toolbarButton))
                mode = EditorMode.Create;
            if (GUILayout.Toggle(mode == EditorMode.Modify, "Modify Existing", EditorStyles.toolbarButton))
                mode = EditorMode.Modify;
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(60)))
                RefreshItemList();
            EditorGUILayout.EndHorizontal();

            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            if (mode == EditorMode.Create)
            {
                DrawCreateMode();
            }
            else
            {
                DrawModifyMode();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawCreateMode()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Create New Item", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            DrawBasicFields(ref newDisplayName, ref newDescription, ref newIcon, 
                ref newIsConsumable, ref newIsEquippable, ref newMaxStack);

            EditorGUILayout.Space(10);
            DrawEffectsList(newEffects);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Save Settings", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            savePath = EditorGUILayout.TextField("Save Path", savePath);
            if (GUILayout.Button("Browse", GUILayout.Width(60)))
            {
                string path = EditorUtility.OpenFolderPanel("Select Save Folder", "Assets", "");
                if (!string.IsNullOrEmpty(path))
                {
                    savePath = "Assets" + path.Substring(Application.dataPath.Length);
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            GUI.enabled = !string.IsNullOrWhiteSpace(newDisplayName);
            if (GUILayout.Button("Create Item", GUILayout.Height(30)))
            {
                CreateNewItem();
            }
            GUI.enabled = true;

            if (GUILayout.Button("Clear All Fields", GUILayout.Height(25)))
            {
                ClearCreateFields();
            }
        }

        private void DrawModifyMode()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField($"All Items ({allItems.Count})", EditorStyles.boldLabel);

            // Search and filters
            EditorGUILayout.BeginHorizontal();
            searchFilter = EditorGUILayout.TextField("Search", searchFilter);
            if (GUILayout.Button("Clear", GUILayout.Width(50)))
                searchFilter = "";
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            showConsumableOnly = EditorGUILayout.ToggleLeft("Consumables Only", showConsumableOnly, GUILayout.Width(150));
            showEquippableOnly = EditorGUILayout.ToggleLeft("Equippables Only", showEquippableOnly, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            // Item list
            EditorGUILayout.BeginHorizontal();
            
            // Left panel - item list
            EditorGUILayout.BeginVertical(GUILayout.Width(250));
            itemListScroll = EditorGUILayout.BeginScrollView(itemListScroll, GUI.skin.box, GUILayout.Height(400));
            
            List<ItemData> filteredItems = GetFilteredItems();
            foreach (ItemData item in filteredItems)
            {
                bool isSelected = selectedItem == item;
                GUI.backgroundColor = isSelected ? Color.cyan : Color.white;

                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                
                if (item.icon)
                {
                    GUILayout.Label(item.icon.texture, GUILayout.Width(30), GUILayout.Height(30));
                }
                else
                {
                    GUILayout.Box("", GUILayout.Width(30), GUILayout.Height(30));
                }

                if (GUILayout.Button(item.displayName, EditorStyles.label))
                {
                    selectedItem = item;
                    Selection.activeObject = item;
                }

                EditorGUILayout.EndHorizontal();
                GUI.backgroundColor = Color.white;
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            // Right panel - item details
            EditorGUILayout.BeginVertical();
            if (selectedItem != null)
            {
                DrawItemDetails();
            }
            else
            {
                EditorGUILayout.HelpBox("Select an item to edit", MessageType.Info);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndHorizontal();
        }

        private void DrawItemDetails()
        {
            EditorGUILayout.LabelField("Edit Item", EditorStyles.boldLabel);
            EditorGUILayout.Space(5);

            EditorGUI.BeginChangeCheck();

            string displayName = selectedItem.displayName;
            string description = selectedItem.description;
            Sprite icon = selectedItem.icon;
            bool isConsumable = selectedItem.isConsumable;
            bool isEquippable = selectedItem.isEquippable;
            int maxStack = selectedItem.maxStack;

            DrawBasicFields(ref displayName, ref description, ref icon, 
                ref isConsumable, ref isEquippable, ref maxStack);

            EditorGUILayout.Space(10);

            // Convert array to list for editing
            List<ItemEffect> effectsList = selectedItem.effects != null 
                ? new List<ItemEffect>(selectedItem.effects) 
                : new List<ItemEffect>();

            DrawEffectsList(effectsList);

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(selectedItem, "Modify Item Data");
                
                selectedItem.displayName = displayName;
                selectedItem.description = description;
                selectedItem.icon = icon;
                selectedItem.isConsumable = isConsumable;
                selectedItem.isEquippable = isEquippable;
                selectedItem.maxStack = maxStack;
                selectedItem.effects = effectsList.ToArray();

                EditorUtility.SetDirty(selectedItem);
            }

            EditorGUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Ping in Project", GUILayout.Height(25)))
            {
                EditorGUIUtility.PingObject(selectedItem);
            }
            if (GUILayout.Button("Duplicate Item", GUILayout.Height(25)))
            {
                DuplicateItem(selectedItem);
            }
            EditorGUILayout.EndHorizontal();

            GUI.backgroundColor = Color.red;
            if (GUILayout.Button("Delete Item", GUILayout.Height(25)))
            {
                if (EditorUtility.DisplayDialog("Delete Item", 
                    $"Are you sure you want to delete '{selectedItem.displayName}'?", 
                    "Delete", "Cancel"))
                {
                    DeleteItem(selectedItem);
                }
            }
            GUI.backgroundColor = Color.white;
        }

        private void DrawBasicFields(ref string displayName, ref string description, ref Sprite icon,
            ref bool isConsumable, ref bool isEquippable, ref int maxStack)
        {
            displayName = EditorGUILayout.TextField("Display Name", displayName);
            
            EditorGUILayout.LabelField("Description");
            description = EditorGUILayout.TextArea(description, GUILayout.Height(60));

            icon = (Sprite)EditorGUILayout.ObjectField("Icon", icon, typeof(Sprite), false);

            EditorGUILayout.Space(5);
            isConsumable = EditorGUILayout.Toggle("Is Consumable", isConsumable);
            isEquippable = EditorGUILayout.Toggle("Is Equippable", isEquippable);
            maxStack = EditorGUILayout.IntSlider("Max Stack", maxStack, 1, 999);
        }

        private void DrawEffectsList(List<ItemEffect> effects)
        {
            showEffectFoldout = EditorGUILayout.Foldout(showEffectFoldout, "Effects", true);
            
            if (!showEffectFoldout) return;

            EditorGUI.indentLevel++;

            for (int i = 0; i < effects.Count; i++)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"Effect {i + 1}", EditorStyles.boldLabel);
                
                GUI.backgroundColor = Color.red;
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    effects.RemoveAt(i);
                    EditorGUI.indentLevel--;
                    return;
                }
                GUI.backgroundColor = Color.white;
                EditorGUILayout.EndHorizontal();

                ItemEffect effect = effects[i];
                effect.type = (EffectType)EditorGUILayout.EnumPopup("Type", effect.type);
                effect.value = EditorGUILayout.FloatField("Value", effect.value);
                effect.duration = EditorGUILayout.FloatField("Duration (0 = instant)", effect.duration);
                effects[i] = effect;

                // Show helpful hints
                string hint = GetEffectHint(effect.type);
                if (!string.IsNullOrEmpty(hint))
                {
                    EditorGUILayout.HelpBox(hint, MessageType.Info);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(3);
            }

            if (GUILayout.Button("+ Add Effect", GUILayout.Height(25)))
            {
                effects.Add(new ItemEffect());
            }

            EditorGUI.indentLevel--;
        }

        private string GetEffectHint(EffectType type)
        {
            return type switch
            {
                EffectType.HealthRestore => "Restores health immediately. Value = amount to restore.",
                EffectType.HealthBoost => "Temporarily increases max health. Value = amount to add.",
                EffectType.SpeedBoost => "Temporarily increases movement speed. Value = speed multiplier.",
                EffectType.JumpBoost => "Temporarily increases jump height. Value = jump force to add.",
                EffectType.StaminaRestore => "Restores stamina immediately. Value = amount to restore.",
                EffectType.StaminaBoost => "Temporarily increases max stamina. Value = amount to add.",
                EffectType.ScaleIncrease => "Temporarily increases player scale. Value = scale multiplier (0.5 = 150% size).",
                EffectType.ScaleDecrease => "Temporarily decreases player scale. Value = scale reduction (0.5 = 50% size).",
                EffectType.GravityReduction => "Temporarily reduces gravity effect. Value = gravity reduction amount.",
                _ => ""
            };
        }

        private void CreateNewItem()
        {
            if (!AssetDatabase.IsValidFolder(savePath))
            {
                string[] folders = savePath.Split('/');
                string currentPath = folders[0];
                for (int i = 1; i < folders.Length; i++)
                {
                    string newPath = $"{currentPath}/{folders[i]}";
                    if (!AssetDatabase.IsValidFolder(newPath))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = newPath;
                }
            }

            ItemData newItem = CreateInstance<ItemData>();
            newItem.displayName = newDisplayName;
            newItem.description = newDescription;
            newItem.icon = newIcon;
            newItem.isConsumable = newIsConsumable;
            newItem.isEquippable = newIsEquippable;
            newItem.maxStack = newMaxStack;
            newItem.effects = newEffects.ToArray();

            string sanitizedName = newDisplayName.Replace(" ", "_");
            string assetPath = $"{savePath}/{sanitizedName}.asset";
            assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);

            AssetDatabase.CreateAsset(newItem, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newItem;

            RefreshItemList();
            ClearCreateFields();

            Debug.Log($"Created new item: {newDisplayName} at {assetPath}");
        }

        private void DuplicateItem(ItemData original)
        {
            ItemData duplicate = Instantiate(original);
            duplicate.displayName = $"{original.displayName} (Copy)";

            string path = AssetDatabase.GetAssetPath(original);
            string directory = System.IO.Path.GetDirectoryName(path);
            string sanitizedName = duplicate.displayName.Replace(" ", "_");
            string newPath = $"{directory}/{sanitizedName}.asset";
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

            AssetDatabase.CreateAsset(duplicate, newPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RefreshItemList();
            selectedItem = duplicate;
            Selection.activeObject = duplicate;
        }

        private void DeleteItem(ItemData item)
        {
            string path = AssetDatabase.GetAssetPath(item);
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            RefreshItemList();
            selectedItem = null;
        }

        private void ClearCreateFields()
        {
            newDisplayName = "";
            newDescription = "";
            newIcon = null;
            newIsConsumable = false;
            newIsEquippable = false;
            newMaxStack = 99;
            newEffects.Clear();
        }

        private void RefreshItemList()
        {
            allItems.Clear();
            string[] guids = AssetDatabase.FindAssets("t:ItemData");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                ItemData item = AssetDatabase.LoadAssetAtPath<ItemData>(path);
                if (item) allItems.Add(item);
            }

            allItems = allItems.OrderBy(item => item.displayName).ToList();
        }

        private List<ItemData> GetFilteredItems()
        {
            return allItems.Where(item =>
            {
                if (!string.IsNullOrEmpty(searchFilter) && 
                    !item.displayName.ToLower().Contains(searchFilter.ToLower()))
                    return false;

                if (showConsumableOnly && !item.isConsumable)
                    return false;

                if (showEquippableOnly && !item.isEquippable)
                    return false;

                return true;
            }).ToList();
        }
    }
}