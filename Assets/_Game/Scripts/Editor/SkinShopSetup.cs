using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IslandHarvest.Game
{
    public static class SkinShopSetup
    {
        const string SkinsFolder = "Assets/_Game/Resources/Skins";
        const string IAPFolder = "Assets/_Game/Resources/IAP";
        const string PrefabFolder = "Assets/_Game/Prefabs/Shop";
        const string Level01Scene = "Assets/_Game/Scenes/Level01.unity";

        [MenuItem("Tools/Island Harvest/Create Skin Shop Assets")]
        public static void CreateAll()
        {
            CreateAllInternal();
        }

        [MenuItem("Tools/Island Harvest/Upgrade Shop UI Layout")]
        public static void UpgradeShopUILayoutMenu()
        {
            UpgradeShopUILayout();
            AssetDatabase.SaveAssets();
            Debug.Log("Shop UI layout upgraded (item rows + scroll list).");
        }

        // In-level SkinShop removed: purchases are main-menu only (_MainMenu + GameShopWindow).

        public static void CreateAllInternal()
        {
            CreateSkinDataAssets();
            CreateIAPProductAssets();
            var rowPrefab = CreateItemRowPrefab();
            var iapRowPrefab = CreateIAPItemRowPrefab();
            var windowPrefab = CreateOrUpgradeWindowPrefab(rowPrefab, iapRowPrefab);
            CreateShopPrefab(windowPrefab);
            AddPlayerSkinManagerToPlayerPrefab();
            UpgradeShopUILayout();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Shop assets created. Run Setup Main Menu Shop Window on _MainMenu scene.");
        }

        public static void UpgradeShopUILayout()
        {
            UpgradeItemRowPrefab($"{PrefabFolder}/SkinShopItemRow.prefab", iapRow: false);
            UpgradeItemRowPrefab($"{PrefabFolder}/IAPShopItemRow.prefab", iapRow: true);
            UpgradeWindowScrollLayout($"{PrefabFolder}/SkinShopWindow.prefab");
        }

        static void PlaceSkinShopInScene(string scenePath)
        {
            if (string.IsNullOrEmpty(scenePath))
            {
                Debug.LogError("No active scene.");
                return;
            }

            string shopPrefabPath = $"{PrefabFolder}/SkinShop.prefab";
            var shopPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(shopPrefabPath);
            if (shopPrefab == null)
            {
                Debug.LogError("SkinShop.prefab missing. Run Create Skin Shop Assets first.");
                return;
            }

            if (Object.FindFirstObjectByType<SkinShop>() != null)
            {
                Debug.Log("SkinShop already exists in scene.");
                return;
            }

            var instance = PrefabUtility.InstantiatePrefab(shopPrefab) as GameObject;
            instance.transform.position = new Vector3(12f, 0f, 8f);
            Undo.RegisterCreatedObjectUndo(instance, "Place Skin Shop");
            Debug.Log($"Placed SkinShop in {scenePath}");
        }

        static void CreateSkinDataAssets()
        {
            var catalog = AssetDatabase.LoadAssetAtPath<SkinCatalogConfig>(SkinCatalogConfig.DefaultAssetPath);
            if (catalog != null && catalog.Entries.Count > 0)
            {
                SkinCatalogSync.Sync(catalog);
                return;
            }

            EnsureFolder("Assets/_Game/Resources");
            EnsureFolder(SkinsFolder);

            Debug.LogWarning(
                "Chưa có Skin Catalog. Dùng Tools → Island Harvest → Create Skin Catalog Asset, " +
                "hoặc tạo SkinData thủ công trong Resources/Skins.");
        }

        static void CreateIAPProductAssets()
        {
            EnsureFolder(IAPFolder);
            CreateIAP("IAP_Coins_Small", "iap_coins_small", IAPProductKind.CoinPack, "Small bag of Coins", 2500, null);
            CreateIAP("IAP_Coins_Large", "iap_coins_large", IAPProductKind.CoinPack, "Big bag of Coins", 10000, null);
            CreateIAP("IAP_Skin_Gold", "iap_skin_gold", IAPProductKind.PremiumSkin, "Premium Gold Skin", 0, "skin_gold");
        }

        static void CreateIAP(string fileName, string productId, IAPProductKind kind, string title, int coins, string skinId)
        {
            string path = $"{IAPFolder}/{fileName}.asset";
            if (File.Exists(path)) return;

            var product = ScriptableObject.CreateInstance<IAPProductData>();
            product.SetEditorDefaults(productId, kind, title, coins, skinId);
            AssetDatabase.CreateAsset(product, path);
        }

        static GameObject CreateItemRowPrefab()
        {
            EnsureFolder("Assets/_Game/Prefabs");
            EnsureFolder(PrefabFolder);

            string path = $"{PrefabFolder}/SkinShopItemRow.prefab";
            if (File.Exists(path))
            {
                UpgradeItemRowPrefab(path, iapRow: false);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            return BuildSkinRowPrefab(path);
        }

        static GameObject BuildSkinRowPrefab(string path)
        {
            var root = new GameObject("SkinShopItemRow", typeof(RectTransform), typeof(SkinShopItemRow));
            var preview = CreateUIImage(root.transform, "Preview", new Vector2(48f, 48f), new Color(1f, 0.85f, 0.2f));
            var name = CreateTMP(root.transform, "Name", "Skin Name", 160f, 26f, FontStyles.Bold);
            var price = CreateTMP(root.transform, "Price", "500", 130f, 20f, FontStyles.Normal);

            var buttonGo = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(root.transform, false);
            buttonGo.GetComponent<RectTransform>().sizeDelta = new Vector2(118f, 40f);
            buttonGo.GetComponent<Image>().color = new Color(0.22f, 0.72f, 0.38f);

            var label = CreateTMP(buttonGo.transform, "Label", "Buy", 100f, 20f, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            StretchToParent(label.rectTransform);

            ApplyItemRowLayout(root, iapRow: false);

            var row = root.GetComponent<SkinShopItemRow>();
            var so = new SerializedObject(row);
            so.FindProperty("previewImage").objectReferenceValue = preview;
            so.FindProperty("nameText").objectReferenceValue = name;
            so.FindProperty("priceText").objectReferenceValue = price;
            so.FindProperty("actionButton").objectReferenceValue = buttonGo.GetComponent<Button>();
            so.FindProperty("actionButtonLabel").objectReferenceValue = label;
            so.FindProperty("actionButtonImage").objectReferenceValue = buttonGo.GetComponent<Image>();
            so.FindProperty("rowBackground").objectReferenceValue = root.GetComponent<Image>();
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject CreateIAPItemRowPrefab()
        {
            string path = $"{PrefabFolder}/IAPShopItemRow.prefab";
            if (File.Exists(path))
            {
                UpgradeItemRowPrefab(path, iapRow: true);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            var root = new GameObject("IAPShopItemRow", typeof(RectTransform), typeof(IAPShopItemRow));
            var preview = CreateUIImage(root.transform, "Preview", new Vector2(48f, 48f), new Color(0.9f, 0.75f, 0.2f));
            var icon = CreateUIImage(root.transform, "Icon", new Vector2(40f, 40f), Color.white);
            icon.gameObject.SetActive(false);
            var name = CreateTMP(root.transform, "Name", "IAP Item", 160f, 26f, FontStyles.Bold);
            var price = CreateTMP(root.transform, "Price", "$0.99", 130f, 20f, FontStyles.Normal);

            var buttonGo = new GameObject("ActionButton", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(root.transform, false);
            buttonGo.GetComponent<RectTransform>().sizeDelta = new Vector2(118f, 40f);
            buttonGo.GetComponent<Image>().color = new Color(0.25f, 0.55f, 0.9f);

            var label = CreateTMP(buttonGo.transform, "Label", "Buy", 100f, 20f, FontStyles.Bold);
            label.alignment = TextAlignmentOptions.Center;
            StretchToParent(label.rectTransform);

            ApplyItemRowLayout(root, iapRow: true);

            var row = root.GetComponent<IAPShopItemRow>();
            var so = new SerializedObject(row);
            so.FindProperty("previewImage").objectReferenceValue = preview;
            so.FindProperty("iconImage").objectReferenceValue = icon;
            so.FindProperty("nameText").objectReferenceValue = name;
            so.FindProperty("priceText").objectReferenceValue = price;
            so.FindProperty("actionButton").objectReferenceValue = buttonGo.GetComponent<Button>();
            so.FindProperty("actionButtonLabel").objectReferenceValue = label;
            so.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static GameObject CreateOrUpgradeWindowPrefab(GameObject rowPrefab, GameObject iapRowPrefab)
        {
            string path = $"{PrefabFolder}/SkinShopWindow.prefab";
            if (File.Exists(path))
            {
                UpgradeWindowPrefab(path, rowPrefab, iapRowPrefab);
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }

            return BuildWindowPrefab(path, rowPrefab, iapRowPrefab);
        }

        static GameObject BuildWindowPrefab(string path, GameObject rowPrefab, GameObject iapRowPrefab)
        {
            var root = new GameObject("SkinShopWindow", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(SkinShopWindow));
            StretchRect(root.GetComponent<RectTransform>());
            root.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.75f);

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(640f, 520f);
            panel.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

            var title = CreateTMP(panel.transform, "Title", "Shop", 400f, 32f, FontStyles.Bold);
            SetAnchoredTop(title, -28f, new Vector2(520f, 40f));

            var coinText = CreateTMP(panel.transform, "CoinBalance", "Coins: 0", 520f, 22f, FontStyles.Bold);
            SetAnchoredTop(coinText, -64f, new Vector2(520f, 28f));
            coinText.alignment = TextAlignmentOptions.Right;

            var tabSkins = CreateTabButton(panel.transform, "TabSkins", "Skins", new Vector2(-80f, -96f));
            var tabIap = CreateTabButton(panel.transform, "TabIAP", "IAP", new Vector2(80f, -96f));

            var skinsPanel = CreateScrollList(panel.transform, "SkinsPanel", new Vector2(0f, -128f), new Vector2(600f, 300f), out Transform skinsContent);
            var iapPanel = CreateScrollList(panel.transform, "IAPPanel", new Vector2(0f, -128f), new Vector2(600f, 300f), out Transform iapContent);
            iapPanel.SetActive(false);

            var closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(panel.transform, false);
            var closeRect = closeGo.GetComponent<RectTransform>();
            closeRect.anchorMin = closeRect.anchorMax = new Vector2(1f, 1f);
            closeRect.anchoredPosition = new Vector2(-24f, -24f);
            closeRect.sizeDelta = new Vector2(40f, 40f);
            closeGo.GetComponent<Image>().color = new Color(0.85f, 0.25f, 0.25f);
            CreateTMP(closeGo.transform, "X", "X", 36f, 24f, FontStyles.Bold).alignment = TextAlignmentOptions.Center;

            WireGameShopWindow(root.GetComponent<SkinShopWindow>(), panelRect, title, coinText,
                tabSkins.GetComponent<Button>(), tabIap.GetComponent<Button>(),
                skinsPanel, iapPanel, skinsContent, iapContent,
                rowPrefab.GetComponent<SkinShopItemRow>(),
                iapRowPrefab.GetComponent<IAPShopItemRow>(),
                closeGo.GetComponent<Button>());

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void UpgradeWindowPrefab(string path, GameObject rowPrefab, GameObject iapRowPrefab)
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(path);
            var window = prefabRoot.GetComponent<GameShopWindow>();
            if (window == null)
                window = prefabRoot.AddComponent<SkinShopWindow>();

            var panel = prefabRoot.transform.Find("Panel");
            if (panel == null)
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
                return;
            }

            var coinText = panel.Find("CoinBalance")?.GetComponent<TMP_Text>();
            if (coinText == null)
            {
                coinText = CreateTMP(panel, "CoinBalance", "Coins: 0", 520f, 22f, FontStyles.Bold);
                SetAnchoredTop(coinText, -64f, new Vector2(520f, 28f));
                coinText.alignment = TextAlignmentOptions.Right;
            }

            var tabSkins = panel.Find("TabSkins")?.GetComponent<Button>() ?? CreateTabButton(panel, "TabSkins", "Skins", new Vector2(-80f, -96f)).GetComponent<Button>();
            var tabIap = panel.Find("TabIAP")?.GetComponent<Button>() ?? CreateTabButton(panel, "TabIAP", "IAP", new Vector2(80f, -96f)).GetComponent<Button>();

            var skinsPanel = panel.Find("SkinsPanel")?.gameObject;
            Transform skinsContent = null;
            if (skinsPanel == null)
            {
                var scroll = panel.Find("Scroll");
                if (scroll != null)
                {
                    scroll.name = "SkinsPanel";
                    skinsPanel = scroll.gameObject;
                    skinsContent = scroll.Find("Viewport/Content");
                }
                else
                {
                    skinsPanel = CreateScrollList(panel, "SkinsPanel", new Vector2(0f, -130f), new Vector2(560f, 260f), out skinsContent);
                }
            }
            else
            {
                skinsContent = skinsPanel.transform.Find("Viewport/Content");
            }

            var iapPanel = panel.Find("IAPPanel")?.gameObject;
            Transform iapContent = null;
            if (iapPanel == null)
                iapPanel = CreateScrollList(panel, "IAPPanel", new Vector2(0f, -130f), new Vector2(560f, 260f), out iapContent);
            else
                iapContent = iapPanel.transform.Find("Viewport/Content");

            var restoreObj = panel.Find("RestoreButton");
            if (restoreObj != null)
                Object.DestroyImmediate(restoreObj.gameObject);

            var closeBtn = panel.Find("CloseButton")?.GetComponent<Button>();
            var title = panel.Find("Title")?.GetComponent<TMP_Text>();

            WireGameShopWindow(window, panel.GetComponent<RectTransform>(), title, coinText,
                tabSkins, tabIap, skinsPanel, iapPanel, skinsContent, iapContent,
                rowPrefab.GetComponent<SkinShopItemRow>(),
                iapRowPrefab.GetComponent<IAPShopItemRow>(),
                closeBtn);

            ApplyWindowPanelLayout(panel);
            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        static void WireGameShopWindow(
            GameShopWindow window,
            RectTransform panelRect,
            TMP_Text title,
            TMP_Text coinText,
            Button tabSkins,
            Button tabIap,
            GameObject skinsPanel,
            GameObject iapPanel,
            Transform skinsContent,
            Transform iapContent,
            SkinShopItemRow skinRow,
            IAPShopItemRow iapRow,
            Button closeBtn)
        {
            var so = new SerializedObject(window);
            so.FindProperty("mainPanel").objectReferenceValue = panelRect;
            so.FindProperty("titleText").objectReferenceValue = title;
            so.FindProperty("coinBalanceText").objectReferenceValue = coinText;
            so.FindProperty("tabSkinsButton").objectReferenceValue = tabSkins;
            so.FindProperty("tabIAPButton").objectReferenceValue = tabIap;
            so.FindProperty("skinsPanel").objectReferenceValue = skinsPanel;
            so.FindProperty("iapPanel").objectReferenceValue = iapPanel;
            so.FindProperty("skinsItemContainer").objectReferenceValue = skinsContent;
            so.FindProperty("iapItemContainer").objectReferenceValue = iapContent;
            so.FindProperty("itemRowPrefab").objectReferenceValue = skinRow;
            so.FindProperty("iapItemRowPrefab").objectReferenceValue = iapRow;
            so.FindProperty("closeButton").objectReferenceValue = closeBtn;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static GameObject CreateScrollList(Transform parent, string name, Vector2 position, Vector2 size, out Transform content)
        {
            var scrollGo = new GameObject(name, typeof(RectTransform), typeof(ScrollRect), typeof(Image));
            scrollGo.transform.SetParent(parent, false);
            var scrollRect = scrollGo.GetComponent<RectTransform>();
            scrollRect.anchorMin = scrollRect.anchorMax = new Vector2(0.5f, 0.5f);
            scrollRect.sizeDelta = size;
            scrollRect.anchoredPosition = position;
            scrollGo.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.12f, 0.5f);

            var viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewport.transform.SetParent(scrollGo.transform, false);
            StretchRect(viewport.GetComponent<RectTransform>());
            viewport.GetComponent<Image>().color = Color.white;
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            contentGo.transform.SetParent(viewport.transform, false);
            var contentRect = contentGo.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.offsetMin = Vector2.zero;
            contentRect.offsetMax = Vector2.zero;

            var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
            vlg.spacing = 6f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;
            vlg.childForceExpandWidth = true;
            vlg.childForceExpandHeight = false;
            contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var scroll = scrollGo.GetComponent<ScrollRect>();
            scroll.viewport = viewport.GetComponent<RectTransform>();
            scroll.content = contentRect;
            scroll.horizontal = false;

            content = contentGo.transform;
            return scrollGo;
        }

        static GameObject CreateTabButton(Transform parent, string name, string label, Vector2 position)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(140f, 36f);
            go.GetComponent<Image>().color = new Color(0.22f, 0.28f, 0.38f);
            var tmp = CreateTMP(go.transform, "Label", label, 140f, 18f, FontStyles.Bold);
            StretchToParent(tmp.rectTransform);
            return go;
        }

        static void CreateShopPrefab(GameObject windowPrefab)
        {
            string path = $"{PrefabFolder}/SkinShop.prefab";
            if (File.Exists(path)) return;

            var stall = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Game/Prefabs/Buildings/Stall01.prefab");
            GameObject shopRoot;

            if (stall != null)
            {
                shopRoot = PrefabUtility.InstantiatePrefab(stall) as GameObject;
                shopRoot.name = "SkinShop";
            }
            else
            {
                shopRoot = GameObject.CreatePrimitive(PrimitiveType.Cube);
                shopRoot.name = "SkinShop";
                shopRoot.transform.localScale = new Vector3(2f, 2f, 2f);
            }

            var trigger = shopRoot.GetComponent<BoxCollider>() ?? shopRoot.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(6f, 4f, 6f);
            trigger.center = new Vector3(0f, 2f, 0f);

            var shop = shopRoot.AddComponent<SkinShop>();
            var skinList = new System.Collections.Generic.List<SkinData>();
            foreach (var asset in AssetDatabase.LoadAllAssetsAtPath(SkinsFolder))
            {
                if (asset is SkinData skin)
                    skinList.Add(skin);
            }

            var so = new SerializedObject(shop);
            so.FindProperty("skinsForSale").arraySize = skinList.Count;
            for (int i = 0; i < skinList.Count; i++)
                so.FindProperty("skinsForSale").GetArrayElementAtIndex(i).objectReferenceValue = skinList[i];
            so.FindProperty("shopWindowPrefab").objectReferenceValue = windowPrefab.GetComponent<GameShopWindow>();
            so.ApplyModifiedPropertiesWithoutUndo();

            PrefabUtility.SaveAsPrefabAsset(shopRoot, path);
            Object.DestroyImmediate(shopRoot);
        }

        static Image CreateUIImage(Transform parent, string name, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = size;
            go.GetComponent<Image>().color = color;
            return go.GetComponent<Image>();
        }

        static TMP_Text CreateTMP(Transform parent, string name, string text, float width, float fontSize, FontStyles style)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            go.GetComponent<RectTransform>().sizeDelta = new Vector2(width, fontSize + 12f);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            return tmp;
        }

        static void SetAnchoredTop(TMP_Text tmp, float y, Vector2 size)
        {
            var rect = tmp.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, y);
            rect.sizeDelta = size;
        }

        static void StretchRect(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        static void AddPlayerSkinManagerToPlayerPrefab()
        {
            string path = "Assets/_Game/Prefabs/Characters/Player.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning("Player.prefab not found; add PlayerSkinManager manually.");
                return;
            }

            var root = PrefabUtility.LoadPrefabContents(path);
            if (root.GetComponent<PlayerSkinManager>() == null)
                root.AddComponent<PlayerSkinManager>();
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            var parent = Path.GetDirectoryName(path).Replace('\\', '/');
            var folderName = Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, folderName);
        }

        static void UpgradeItemRowPrefab(string path, bool iapRow)
        {
            if (!File.Exists(path)) return;

            var root = PrefabUtility.LoadPrefabContents(path);
            ApplyItemRowLayout(root, iapRow);
            WireSkinRowReferences(root, iapRow);
            PrefabUtility.SaveAsPrefabAsset(root, path);
            PrefabUtility.UnloadPrefabContents(root);
        }

        static void WireSkinRowReferences(GameObject root, bool iapRow)
        {
            if (iapRow) return;

            var row = root.GetComponent<SkinShopItemRow>();
            if (row == null) return;

            var button = root.transform.Find("ActionButton")?.GetComponent<Button>();
            var so = new SerializedObject(row);
            so.FindProperty("actionButtonImage").objectReferenceValue = button?.GetComponent<Image>();
            so.FindProperty("rowBackground").objectReferenceValue = root.GetComponent<Image>();
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ApplyItemRowLayout(GameObject root, bool iapRow)
        {
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0f, 80f);
            rect.anchoredPosition = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);

            var bg = root.GetComponent<Image>() ?? root.AddComponent<Image>();
            bg.color = new Color(1f, 1f, 1f, 0.06f);
            bg.raycastTarget = false;

            var hlg = root.GetComponent<HorizontalLayoutGroup>() ?? root.AddComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(16, 16, 10, 10);
            hlg.spacing = 14f;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;

            SetLayoutElement(root.transform, "Preview", 52f, 52f, 0f, useHeight: true);
            SetLayoutElement(root.transform, "Name", 120f, 0f, 1f, useHeight: false);
            SetLayoutElement(root.transform, "Price", 128f, 28f, 0f, useHeight: true);
            SetLayoutElement(root.transform, "ActionButton", 118f, 40f, 0f, useHeight: true);

            var name = root.transform.Find("Name")?.GetComponent<TMP_Text>();
            if (name != null)
            {
                name.alignment = TextAlignmentOptions.MidlineLeft;
                name.fontSize = 26f;
                name.color = Color.white;
            }

            var price = root.transform.Find("Price")?.GetComponent<TMP_Text>();
            if (price != null)
            {
                price.alignment = TextAlignmentOptions.Center;
                price.fontSize = 20f;
                price.color = new Color(0.72f, 0.78f, 0.84f);
            }

            var preview = root.transform.Find("Preview")?.GetComponent<Image>();
            if (preview != null)
            {
                preview.preserveAspect = true;
                preview.GetComponent<RectTransform>().sizeDelta = new Vector2(48f, 48f);
            }

            var button = root.transform.Find("ActionButton");
            if (button != null)
            {
                button.GetComponent<RectTransform>().sizeDelta = new Vector2(118f, 40f);
                var btnImage = button.GetComponent<Image>();
                if (btnImage != null && !iapRow)
                    btnImage.color = new Color(0.22f, 0.72f, 0.38f);
                else if (btnImage != null)
                    btnImage.color = new Color(0.25f, 0.55f, 0.9f);
            }
        }

        static void SetLayoutElement(Transform root, string childName, float width, float height, float flexWidth, bool useHeight)
        {
            var child = root.Find(childName);
            if (child == null) return;

            var le = child.GetComponent<LayoutElement>() ?? child.gameObject.AddComponent<LayoutElement>();
            le.minWidth = width > 0f ? width : -1f;
            le.preferredWidth = width;
            le.flexibleWidth = flexWidth;
            le.flexibleHeight = 0f;

            if (useHeight && height > 0f)
            {
                le.minHeight = height;
                le.preferredHeight = height;
            }
            else
            {
                le.minHeight = -1f;
                le.preferredHeight = -1f;
            }
        }

        static void UpgradeWindowScrollLayout(string path)
        {
            if (!File.Exists(path)) return;

            var prefabRoot = PrefabUtility.LoadPrefabContents(path);
            var panel = prefabRoot.transform.Find("Panel");
            if (panel != null)
                ApplyWindowPanelLayout(panel);

            ApplyScrollContentLayout(panel?.Find("SkinsPanel/Viewport/Content"));
            ApplyScrollContentLayout(panel?.Find("IAPPanel/Viewport/Content"));

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, path);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
        }

        static void ApplyWindowPanelLayout(Transform panel)
        {
            if (panel == null) return;

            var panelRect = panel.GetComponent<RectTransform>();
            if (panelRect != null)
                panelRect.sizeDelta = new Vector2(640f, 520f);

            var skinsScroll = panel.Find("SkinsPanel")?.GetComponent<RectTransform>();
            if (skinsScroll != null)
            {
                skinsScroll.sizeDelta = new Vector2(600f, 300f);
                skinsScroll.anchoredPosition = new Vector2(0f, -128f);
            }

            var iapScroll = panel.Find("IAPPanel")?.GetComponent<RectTransform>();
            if (iapScroll != null)
            {
                iapScroll.sizeDelta = new Vector2(600f, 300f);
                iapScroll.anchoredPosition = new Vector2(0f, -128f);
            }
        }

        static void ApplyScrollContentLayout(Transform content)
        {
            if (content == null) return;

            var vlg = content.GetComponent<VerticalLayoutGroup>();
            if (vlg == null) return;

            vlg.spacing = 6f;
            vlg.padding = new RectOffset(10, 10, 10, 10);
            vlg.childForceExpandHeight = false;
        }
    }
}
