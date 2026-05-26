using System.IO;
using UnityEditor;
using UnityEngine;

namespace IslandHarvest.Game
{
    public static class SkinCatalogSync
    {
        const string SkinsFolder = "Assets/_Game/Resources/Skins";

        public static int Sync(SkinCatalogConfig catalog)
        {
            if (catalog == null)
                return 0;

            EnsureFolder("Assets/_Game/Resources");
            EnsureFolder(SkinsFolder);

            int count = 0;
            foreach (var entry in catalog.Entries)
            {
                if (entry == null || string.IsNullOrWhiteSpace(entry.assetFileName))
                    continue;

                if (SyncEntry(entry))
                    count++;
            }

            AssetDatabase.SaveAssets();
            return count;
        }

        public static void ImportFromResources(SkinCatalogConfig catalog)
        {
            if (catalog == null)
                return;

            var skins = Resources.LoadAll<SkinData>("Skins");
            var list = catalog.GetMutableEntries();
            list.Clear();

            foreach (var skin in skins)
            {
                if (skin == null)
                    continue;

                string path = AssetDatabase.GetAssetPath(skin);
                string fileName = Path.GetFileNameWithoutExtension(path);

                list.Add(new SkinCatalogEntry
                {
                    assetFileName = fileName,
                    skinId = skin.SkinId,
                    displayName = skin.DisplayName,
                    skinMaterial = skin.SkinMaterial,
                    transparentMaterial = skin.TransparentMaterial != skin.SkinMaterial
                        ? skin.TransparentMaterial
                        : null,
                    price = skin.Price,
                    unlockedByDefault = skin.UnlockedByDefault,
                    purchaseType = skin.PurchaseType
                });
            }

            EditorUtility.SetDirty(catalog);
        }

        static bool SyncEntry(SkinCatalogEntry entry)
        {
            string fileName = entry.assetFileName.Trim();
            if (!fileName.StartsWith("Skin_"))
                fileName = "Skin_" + fileName;

            string path = $"{SkinsFolder}/{fileName}.asset";

            SkinData skin;
            if (File.Exists(path))
                skin = AssetDatabase.LoadAssetAtPath<SkinData>(path);
            else
            {
                skin = ScriptableObject.CreateInstance<SkinData>();
                skin.SetEditorDefaults(
                    entry.skinId,
                    entry.displayName,
                    entry.skinMaterial,
                    entry.transparentMaterial,
                    entry.price,
                    entry.unlockedByDefault);
                AssetDatabase.CreateAsset(skin, path);
            }

            var so = new SerializedObject(skin);
            so.FindProperty("skinId").stringValue = entry.skinId;
            so.FindProperty("displayName").stringValue = entry.displayName;
            so.FindProperty("skinMaterial").objectReferenceValue = entry.skinMaterial;
            so.FindProperty("transparentMaterial").objectReferenceValue = entry.transparentMaterial;
            so.FindProperty("price").intValue = entry.price;
            so.FindProperty("unlockedByDefault").boolValue = entry.unlockedByDefault;
            so.FindProperty("purchaseType").enumValueIndex = (int)entry.purchaseType;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(skin);
            return true;
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;

            string parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            string name = Path.GetFileName(path);
            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
