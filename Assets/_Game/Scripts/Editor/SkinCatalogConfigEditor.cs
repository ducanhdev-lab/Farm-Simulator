using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace IslandHarvest.Game
{
    [CustomEditor(typeof(SkinCatalogConfig))]
    public class SkinCatalogConfigEditor : Editor
    {
        ReorderableList list;

        void OnEnable()
        {
            var entriesProp = serializedObject.FindProperty("entries");
            list = new ReorderableList(serializedObject, entriesProp, true, true, true, true);

            list.drawHeaderCallback = rect =>
            {
                EditorGUI.LabelField(rect, "Skins (source list — sync creates Resources/Skins assets)");
            };

            list.elementHeightCallback = index => EditorGUI.GetPropertyHeight(list.serializedProperty.GetArrayElementAtIndex(index), true) + 4f;

            list.drawElementCallback = (rect, index, active, focused) =>
            {
                var element = list.serializedProperty.GetArrayElementAtIndex(index);
                rect.y += 2f;
                rect.height -= 4f;
                EditorGUI.PropertyField(rect, element, new GUIContent($"Skin {index + 1}"), true);
            };

            list.onAddCallback = reorderableList =>
            {
                int index = reorderableList.serializedProperty.arraySize;
                reorderableList.serializedProperty.InsertArrayElementAtIndex(index);
                var element = reorderableList.serializedProperty.GetArrayElementAtIndex(index);

                SetRelative(element, "assetFileName", "Skin_New");
                SetRelative(element, "skinId", "skin_new");
                SetRelative(element, "displayName", "New Skin");
                SetRelative(element, "price", 500);
                SetRelative(element, "unlockedByDefault", false);
                SetRelative(element, "purchaseType", (int)SkinPurchaseType.Coins);
            };
        }

        public override void OnInspectorGUI()
        {
            var catalog = (SkinCatalogConfig)target;
            serializedObject.Update();

            EditorGUILayout.HelpBox(
                "Skin stickman: URP Simple Lit, chỉ Base Color (không texture).\n" +
                "Gán Material → «Đồng bộ vào Shop». Màu mới: Tools → Create Tint Skin Material...",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Đồng bộ vào Shop", GUILayout.Height(28)))
                {
                    serializedObject.ApplyModifiedProperties();
                    int count = SkinCatalogSync.Sync(catalog);
                    AssetDatabase.Refresh();
                    Debug.Log($"Đã đồng bộ {count} skin → Assets/_Game/Resources/Skins");
                }

                if (GUILayout.Button("Import từ Resources/Skins", GUILayout.Height(28)))
                {
                    Undo.RecordObject(catalog, "Import Skins");
                    SkinCatalogSync.ImportFromResources(catalog);
                    serializedObject.Update();
                }
            }

            EditorGUILayout.Space(4f);
            list.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }

        [MenuItem("Tools/Island Harvest/Open Skin Catalog")]
        public static void OpenSkinCatalog()
        {
            var catalog = LoadOrCreateCatalog();
            Selection.activeObject = catalog;
            EditorGUIUtility.PingObject(catalog);
        }

        [MenuItem("Tools/Island Harvest/Create Skin Catalog Asset")]
        public static void CreateSkinCatalogAsset()
        {
            var catalog = LoadOrCreateCatalog(forceCreate: true);
            if (catalog.Entries.Count == 0)
            {
                SkinCatalogSync.ImportFromResources(catalog);
                EditorUtility.SetDirty(catalog);
            }

            Selection.activeObject = catalog;
            Debug.Log($"Skin catalog ready: {SkinCatalogConfig.DefaultAssetPath}");
        }

        public static SkinCatalogConfig LoadOrCreateCatalog(bool forceCreate = false)
        {
            var catalog = AssetDatabase.LoadAssetAtPath<SkinCatalogConfig>(SkinCatalogConfig.DefaultAssetPath);
            if (catalog != null)
                return catalog;

            if (!forceCreate)
                return null;

            EnsureDataFolder();
            catalog = ScriptableObject.CreateInstance<SkinCatalogConfig>();
            AssetDatabase.CreateAsset(catalog, SkinCatalogConfig.DefaultAssetPath);
            AssetDatabase.SaveAssets();
            return catalog;
        }

        static void EnsureDataFolder()
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Game/Data"))
                AssetDatabase.CreateFolder("Assets/_Game", "Data");
        }

        static void SetRelative(SerializedProperty parent, string name, object value)
        {
            var prop = parent.FindPropertyRelative(name);
            if (prop == null)
                return;

            switch (value)
            {
                case string s:
                    prop.stringValue = s;
                    break;
                case int i:
                    prop.intValue = i;
                    break;
                case bool b:
                    prop.boolValue = b;
                    break;
            }
        }
    }
}
