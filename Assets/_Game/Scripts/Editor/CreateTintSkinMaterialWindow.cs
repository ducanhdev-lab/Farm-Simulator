using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace IslandHarvest.Game
{
    public class CreateTintSkinMaterialWindow : EditorWindow
    {
        const string SkinsMaterialFolder = "Assets/_Game/Materials/Skins";
        const string SimpleLitShaderName = "Universal Render Pipeline/Simple Lit";

        string materialName = "Skin_Orange";
        Color baseColor = new Color(0.95f, 0.55f, 0.15f, 1f);

        [MenuItem("Tools/Island Harvest/Create Tint Skin Material...")]
        public static void ShowWindow()
        {
            var window = GetWindow<CreateTintSkinMaterialWindow>(true, "Tint Skin", true);
            window.minSize = new Vector2(320f, 140f);
        }

        void OnGUI()
        {
            EditorGUILayout.LabelField("Skin solid màu (URP Simple Lit)", EditorStyles.boldLabel);
            EditorGUILayout.Space(4f);

            materialName = EditorGUILayout.TextField("Tên material", materialName);
            baseColor = EditorGUILayout.ColorField("Base Color", baseColor);

            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "Không gán texture. Chỉ đổi Base Color giống Skin_Yellow / Skin_Blue.",
                MessageType.Info);

            EditorGUILayout.Space(8f);
            if (GUILayout.Button("Tạo material", GUILayout.Height(32f)))
                Create();
        }

        void Create()
        {
            if (string.IsNullOrWhiteSpace(materialName))
            {
                EditorUtility.DisplayDialog("Tint Skin", "Nhập tên material.", "OK");
                return;
            }

            Material material = CreateTintMaterial(materialName.Trim(), baseColor);
            if (material == null)
            {
                EditorUtility.DisplayDialog("Tint Skin", "Không tạo được material (thiếu URP Simple Lit?).", "OK");
                return;
            }

            Selection.activeObject = material;
            EditorGUIUtility.PingObject(material);
            Debug.Log($"Đã tạo tint skin: {material.name}");
            Close();
        }

        static Material CreateTintMaterial(string name, Color color)
        {
            EnsureSkinsFolder();

            if (!name.StartsWith("Skin_"))
                name = "Skin_" + name;

            var shader = Shader.Find(SimpleLitShaderName);
            if (shader == null)
                return null;

            string path = $"{SkinsMaterialFolder}/{name}.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader) { name = name };
                AssetDatabase.CreateAsset(material, path);
            }
            else
            {
                material.shader = shader;
            }

            material.SetTexture("_BaseMap", null);
            material.SetTexture("_MainTex", null);

            color.a = 0f;
            material.SetColor("_BaseColor", color);
            material.SetColor("_Color", color);
            material.SetFloat("_Metallic", 0f);
            material.SetFloat("_Smoothness", 0f);
            material.DisableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", Color.black);
            material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.EmissiveIsBlack;
            CoreUtils.SetKeyword(material, "_GLOSSINESS_FROM_BASE_ALPHA", true);
            CoreUtils.SetKeyword(material, "_SPECULAR_COLOR", true);

            EditorUtility.SetDirty(material);
            AssetDatabase.SaveAssets();
            return material;
        }

        static void EnsureSkinsFolder()
        {
            if (AssetDatabase.IsValidFolder(SkinsMaterialFolder))
                return;

            if (!AssetDatabase.IsValidFolder("Assets/_Game/Materials"))
                AssetDatabase.CreateFolder("Assets/_Game", "Materials");
            AssetDatabase.CreateFolder("Assets/_Game/Materials", "Skins");
        }
    }
}
