using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IslandHarvest.Game
{
    public static class FarmLifeEditorSetup
    {
        const string ProductionFolder = "Assets/_Game/Data/Production";
        const string BuildingsPrefabFolder = "Assets/_Game/Prefabs/Buildings";
        const string UIManagerPrefabPath = "Assets/_Game/Prefabs/UI/UIManager.prefab";
        const string Level01Scene = "Assets/_Game/Scenes/Level01.unity";
        const string MainMenuScene = "Assets/_Game/Scenes/_MainMenu.unity";
        const string ProcessingPrefabPath = BuildingsPrefabFolder + "/ProcessingStation_Home.prefab";

        [MenuItem("Tools/Island Harvest/Setup Farm Life (All)")]
        public static void SetupAll()
        {
            CreateProcessingContent();
            SetupBiomeProgression();
            SetupModularUI();
            SetupInGameLeaderboard();
            PolishInventoryPrefab();
            SetupMainMenuLeaderboard();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Farm Life setup complete: processing, biomes, UI, leaderboard, inventory.");
        }

        [MenuItem("Tools/Island Harvest/Setup Main Menu Leaderboard")]
        public static void SetupMainMenuLeaderboard()
        {
            var scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);
            var canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas not found in _MainMenu.");
                return;
            }

            var leaderboardWindow = Object.FindFirstObjectByType<LeaderboardWindow>(FindObjectsInactive.Include);
            if (leaderboardWindow == null)
                leaderboardWindow = CreateLeaderboardUi(canvas.transform);

            var visitWindow = Object.FindFirstObjectByType<IslandVisitWindow>(FindObjectsInactive.Include);
            if (visitWindow == null)
                visitWindow = CreateVisitWindow(canvas.transform);

            var lbSo = new SerializedObject(leaderboardWindow);
            lbSo.FindProperty("visitWindow").objectReferenceValue = visitWindow;
            lbSo.ApplyModifiedPropertiesWithoutUndo();

            var leaderboardButton = GameObject.Find("LeaderboardButton")?.GetComponent<Button>();
            if (leaderboardButton == null)
                leaderboardButton = CreateLeaderboardButton(canvas.transform);

            var mainMenu = Object.FindFirstObjectByType<MainMenu>();
            if (mainMenu != null)
            {
                var menuSo = new SerializedObject(mainMenu);
                menuSo.FindProperty("leaderboardButton").objectReferenceValue = leaderboardButton;
                menuSo.FindProperty("leaderboardWindow").objectReferenceValue = leaderboardWindow;
                menuSo.ApplyModifiedPropertiesWithoutUndo();
            }

            leaderboardWindow.Hide();
            visitWindow.Hide();
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Main Menu leaderboard wired.");
        }

        static LeaderboardWindow CreateLeaderboardUi(Transform canvas)
        {
            var root = new GameObject("LeaderboardWindow", typeof(RectTransform), typeof(LeaderboardWindow));
            root.transform.SetParent(canvas, false);
            var rootRt = root.GetComponent<RectTransform>();
            rootRt.anchorMin = Vector2.zero;
            rootRt.anchorMax = Vector2.one;
            rootRt.offsetMin = Vector2.zero;
            rootRt.offsetMax = Vector2.zero;

            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(560f, 720f);
            panel.GetComponent<Image>().color = new Color(0.1f, 0.12f, 0.16f, 0.96f);

            var title = EnsureTmpChild(panel.transform, "Title", new Vector2(0f, -16f), 32);
            title.text = "Leaderboard";

            var status = EnsureTmpChild(panel.transform, "Status", new Vector2(0f, -56f), 20);
            status.text = "";

            var scrollGo = new GameObject("List", typeof(RectTransform));
            scrollGo.transform.SetParent(panel.transform, false);
            var listRt = scrollGo.GetComponent<RectTransform>();
            listRt.anchorMin = new Vector2(0f, 0f);
            listRt.anchorMax = new Vector2(1f, 1f);
            listRt.offsetMin = new Vector2(20f, 80f);
            listRt.offsetMax = new Vector2(-20f, -100f);

            var rowPrefab = EnsureTmpChild(scrollGo.transform, "RowPrefab", Vector2.zero, 22);
            rowPrefab.gameObject.SetActive(false);

            var closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(panel.transform, false);
            var closeRt = closeGo.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.sizeDelta = new Vector2(48f, 48f);
            closeRt.anchoredPosition = new Vector2(-12f, -12f);

            var window = root.GetComponent<LeaderboardWindow>();
            var so = new SerializedObject(window);
            so.FindProperty("root").objectReferenceValue = panel;
            so.FindProperty("closeButton").objectReferenceValue = closeGo.GetComponent<Button>();
            so.FindProperty("listContainer").objectReferenceValue = scrollGo.transform;
            so.FindProperty("rowPrefab").objectReferenceValue = rowPrefab;
            so.FindProperty("statusLabel").objectReferenceValue = status;
            so.ApplyModifiedPropertiesWithoutUndo();
            return window;
        }

        static IslandVisitWindow CreateVisitWindow(Transform canvas)
        {
            var root = new GameObject("IslandVisitWindow", typeof(RectTransform), typeof(IslandVisitWindow));
            root.transform.SetParent(canvas, false);
            var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var panelRt = panel.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(420f, 320f);
            panel.GetComponent<Image>().color = new Color(0.08f, 0.1f, 0.14f, 0.98f);

            var title = EnsureTmpChild(panel.transform, "Title", new Vector2(0f, -20f), 28);
            var details = EnsureTmpChild(panel.transform, "Details", new Vector2(0f, -80f), 22);
            details.alignment = TextAlignmentOptions.TopLeft;
            var status = EnsureTmpChild(panel.transform, "Status", new Vector2(0f, -260f), 18);

            var closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
            closeGo.transform.SetParent(panel.transform, false);
            var closeRt = closeGo.GetComponent<RectTransform>();
            closeRt.anchorMin = new Vector2(1f, 1f);
            closeRt.anchorMax = new Vector2(1f, 1f);
            closeRt.pivot = new Vector2(1f, 1f);
            closeRt.sizeDelta = new Vector2(40f, 40f);
            closeRt.anchoredPosition = new Vector2(-8f, -8f);

            var visit = root.GetComponent<IslandVisitWindow>();
            var so = new SerializedObject(visit);
            so.FindProperty("root").objectReferenceValue = panel;
            so.FindProperty("closeButton").objectReferenceValue = closeGo.GetComponent<Button>();
            so.FindProperty("titleLabel").objectReferenceValue = title;
            so.FindProperty("detailsLabel").objectReferenceValue = details;
            so.FindProperty("statusLabel").objectReferenceValue = status;
            so.ApplyModifiedPropertiesWithoutUndo();
            return visit;
        }

        static Button CreateLeaderboardButton(Transform canvas)
        {
            var shop = GameObject.Find("ShopButton");
            var go = new GameObject("LeaderboardButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas, false);
            var rt = go.GetComponent<RectTransform>();
            if (shop != null)
            {
                var shopRt = shop.GetComponent<RectTransform>();
                rt.anchorMin = shopRt.anchorMin;
                rt.anchorMax = shopRt.anchorMax;
                rt.pivot = shopRt.pivot;
                rt.sizeDelta = shopRt.sizeDelta;
                rt.anchoredPosition = shopRt.anchoredPosition + new Vector2(120f, 0f);
            }
            else
                rt.anchoredPosition = new Vector2(170f, -350f);

            go.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.75f, 1f);
            var label = EnsureTmpChild(go.transform, "Label", Vector2.zero, 24);
            label.text = "RANK";
            label.alignment = TextAlignmentOptions.Center;
            return go.GetComponent<Button>();
        }

        [MenuItem("Tools/Island Harvest/Setup Biome Zones (Level01)")]
        public static void SetupBiomeProgression()
        {
            EnsureFolder("Assets/_Game/Data");
            var config = GetOrCreateBiomeProgressionConfig();
            var scene = EditorSceneManager.OpenScene(Level01Scene, OpenSceneMode.Single);

            var manager = Object.FindFirstObjectByType<BiomeManager>();
            if (manager == null && IslandManager.Instance != null)
                manager = IslandManager.Instance.gameObject.AddComponent<BiomeManager>();

            if (manager != null)
            {
                var so = new SerializedObject(manager);
                so.FindProperty("progressionConfig").objectReferenceValue = config;
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            EnsureBiomeZone("Biome_Starter", BiomeId.Starter, Object.FindFirstObjectByType<ProcessingStation>()?.gameObject);
            var dock = Object.FindFirstObjectByType<Dock>();
            if (dock != null)
                EnsureBiomeZone("Biome_FishingCoast", BiomeId.FishingCoast, dock.gameObject);

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            Debug.Log("Biome progression config + zones applied to Level01.");
        }

        [MenuItem("Tools/Island Harvest/Polish Inventory Window (UIManager)")]
        public static void PolishInventoryPrefab()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(UIManagerPrefabPath);
            if (prefabRoot == null)
                return;

            try
            {
                var inventory = prefabRoot.GetComponentInChildren<InventoryWindow>(true);
                if (inventory == null)
                {
                    Debug.LogWarning("InventoryWindow not found. Run Setup Modular UI first.");
                    return;
                }

                var panel = inventory.transform.Find("Panel");
                if (panel == null)
                    return;

                var summary = EnsureTmpChild(panel, "SummaryLabel", new Vector2(0f, -8f), 22);
                summary.text = "0 types · 0 items";
                summary.alignment = TextAlignmentOptions.Center;

                var empty = EnsureTmpChild(panel, "EmptyLabel", Vector2.zero, 26);
                empty.text = "Inventory empty";
                empty.alignment = TextAlignmentOptions.Center;
                empty.color = new Color(1f, 1f, 1f, 0.6f);

                var closeGo = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button));
                closeGo.transform.SetParent(panel, false);
                var closeRt = closeGo.GetComponent<RectTransform>();
                closeRt.anchorMin = new Vector2(1f, 1f);
                closeRt.anchorMax = new Vector2(1f, 1f);
                closeRt.pivot = new Vector2(1f, 1f);
                closeRt.sizeDelta = new Vector2(48f, 48f);
                closeRt.anchoredPosition = new Vector2(-12f, -12f);
                closeGo.GetComponent<Image>().color = new Color(0.8f, 0.25f, 0.25f, 1f);

                var invSo = new SerializedObject(inventory);
                invSo.FindProperty("summaryLabel").objectReferenceValue = summary;
                invSo.FindProperty("emptyStateLabel").objectReferenceValue = empty;
                invSo.FindProperty("closeButton").objectReferenceValue = closeGo.GetComponent<Button>();
                invSo.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, UIManagerPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        [MenuItem("Tools/Island Harvest/Setup Processing (Recipe + Home Station)")]
        public static void CreateProcessingContent()
        {
            EnsureFolder("Assets/_Game/Data");
            EnsureFolder(ProductionFolder);
            EnsureFolder(BuildingsPrefabFolder);

            var tomato = LoadItem("Tomato");
            var carrot = LoadItem("Carrot");
            if (tomato == null || carrot == null)
            {
                Debug.LogError("Missing Tomato or Carrot ItemData in Resources/Items.");
                return;
            }

            var recipe = GetOrCreateRecipe(
                $"{ProductionFolder}/Recipe_CarrotCrate.asset",
                "recipe_carrot_crate",
                new[] { (carrot, 3) },
                (tomato, 1),
                2.5f);

            var building = GetOrCreateBuilding(
                $"{ProductionFolder}/Building_StarterProcessor.asset",
                "building_starter_processor",
                "Starter Processor",
                recipe);

            var corn = LoadItem("Corn");
            var grouper = LoadItem("Grouper");
            if (corn != null && grouper != null)
            {
                var fishRecipe = GetOrCreateRecipe(
                    $"{ProductionFolder}/Recipe_FishPack.asset",
                    "recipe_fish_pack",
                    new[] { (grouper, 2), (corn, 1) },
                    (grouper, 3),
                    3f);

                GetOrCreateBuilding(
                    $"{ProductionFolder}/Building_FishingProcessor.asset",
                    "building_fishing_processor",
                    "Fishing Processor",
                    fishRecipe,
                    BiomeId.FishingCoast);
            }

            var milk = LoadItem("Milk_Cow");
            var tomatoOut = LoadItem("Tomato");
            if (milk != null && carrot != null && tomatoOut != null)
            {
                var dairyRecipe = GetOrCreateRecipe(
                    $"{ProductionFolder}/Recipe_DairyBox.asset",
                    "recipe_dairy_box",
                    new[] { (milk, 2), (carrot, 1) },
                    (tomatoOut, 2),
                    4f);

                GetOrCreateBuilding(
                    $"{ProductionFolder}/Building_DairyProcessor.asset",
                    "building_dairy_processor",
                    "Dairy Processor",
                    dairyRecipe);
            }

            var stationPrefab = GetOrCreateProcessingPrefab(building, recipe);
            PlaceProcessingInLevel01(stationPrefab);

            AssetDatabase.SaveAssets();
            Debug.Log("Processing: recipe, building, prefab, and Level01 station are ready.");
        }

        [MenuItem("Tools/Island Harvest/Setup In-Game Leaderboard (UIManager)")]
        public static void SetupInGameLeaderboard()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(UIManagerPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"Could not load {UIManagerPrefabPath}");
                return;
            }

            try
            {
                var uiManager = prefabRoot.GetComponent<UIManager>();
                if (uiManager == null)
                {
                    Debug.LogError("UIManager component missing on prefab root.");
                    return;
                }

                var canvas = prefabRoot.GetComponentInChildren<Canvas>(true);
                if (canvas == null)
                {
                    Debug.LogError("Canvas not found on UIManager prefab.");
                    return;
                }

                var leaderboardWindow = prefabRoot.GetComponentInChildren<LeaderboardWindow>(true);
                if (leaderboardWindow == null)
                    leaderboardWindow = CreateLeaderboardUi(canvas.transform);

                var visitWindow = prefabRoot.GetComponentInChildren<IslandVisitWindow>(true);
                if (visitWindow == null)
                    visitWindow = CreateVisitWindow(canvas.transform);

                var lbSo = new SerializedObject(leaderboardWindow);
                lbSo.FindProperty("visitWindow").objectReferenceValue = visitWindow;
                lbSo.ApplyModifiedPropertiesWithoutUndo();

                var rankButton = canvas.transform.Find("LeaderboardButton")?.GetComponent<Button>();
                if (rankButton == null)
                    rankButton = CreateInGameLeaderboardButton(canvas.transform);

                var stack = GetOrAddComponent<PanelStack>(prefabRoot);
                var inventory = prefabRoot.GetComponentInChildren<InventoryWindow>(true);
                if (inventory != null)
                    RegisterPanel(stack, inventory);
                RegisterPanel(stack, leaderboardWindow);
                RegisterPanel(stack, visitWindow);

                var uiSo = new SerializedObject(uiManager);
                uiSo.FindProperty("leaderboardWindow").objectReferenceValue = leaderboardWindow;
                uiSo.FindProperty("islandVisitWindow").objectReferenceValue = visitWindow;
                uiSo.FindProperty("leaderboardButton").objectReferenceValue = rankButton;
                uiSo.FindProperty("panelStack").objectReferenceValue = stack;
                uiSo.ApplyModifiedPropertiesWithoutUndo();

                leaderboardWindow.Hide();
                visitWindow.Hide();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, UIManagerPrefabPath);
                Debug.Log("In-game leaderboard wired on UIManager prefab.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        static void RegisterPanel(PanelStack stack, WindowBase panel)
        {
            if (stack == null || panel == null)
                return;

            var so = new SerializedObject(stack);
            var panels = so.FindProperty("panels");
            for (int i = 0; i < panels.arraySize; i++)
            {
                if (panels.GetArrayElementAtIndex(i).objectReferenceValue == panel)
                    return;
            }

            panels.InsertArrayElementAtIndex(panels.arraySize);
            panels.GetArrayElementAtIndex(panels.arraySize - 1).objectReferenceValue = panel;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static Button CreateInGameLeaderboardButton(Transform canvas)
        {
            Transform invTransform = null;
            foreach (var btn in canvas.GetComponentsInChildren<Button>(true))
            {
                if (btn.gameObject.name == "InventoryButton")
                {
                    invTransform = btn.transform;
                    break;
                }
            }

            var go = new GameObject("LeaderboardButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(canvas, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.sizeDelta = new Vector2(100f, 100f);

            if (invTransform != null)
            {
                var invRt = invTransform.GetComponent<RectTransform>();
                rt.anchoredPosition = invRt.anchoredPosition + new Vector2(110f, 0f);
            }
            else
                rt.anchoredPosition = new Vector2(160f, -460f);

            go.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.75f, 1f);
            var label = EnsureTmpChild(go.transform, "Label", Vector2.zero, 24);
            label.text = "RANK";
            label.alignment = TextAlignmentOptions.Center;
            return go.GetComponent<Button>();
        }

        [MenuItem("Tools/Island Harvest/Setup Modular UI (UIManager Prefab)")]
        public static void SetupModularUI()
        {
            var prefabRoot = PrefabUtility.LoadPrefabContents(UIManagerPrefabPath);
            if (prefabRoot == null)
            {
                Debug.LogError($"Could not load {UIManagerPrefabPath}");
                return;
            }

            try
            {
                var uiManager = prefabRoot.GetComponent<UIManager>();
                if (uiManager == null)
                {
                    Debug.LogError("UIManager component missing on prefab root.");
                    return;
                }

                var hud = GetOrAddComponent<HudController>(prefabRoot);
                WireHudController(hud, uiManager);

                var stack = GetOrAddComponent<PanelStack>(prefabRoot);
                var canvas = prefabRoot.GetComponentInChildren<Canvas>(true);
                var inventory = EnsureInventoryWindow(canvas != null ? canvas.transform : prefabRoot.transform, hud);

                var uiSo = new SerializedObject(uiManager);
                uiSo.FindProperty("hudController").objectReferenceValue = hud;
                uiSo.FindProperty("panelStack").objectReferenceValue = stack;
                uiSo.FindProperty("inventoryWindow").objectReferenceValue = inventory;
                uiSo.ApplyModifiedPropertiesWithoutUndo();

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, UIManagerPrefabPath);
                Debug.Log("Modular UI wired on UIManager prefab (HudController, PanelStack, InventoryWindow).");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        [MenuItem("Tools/Island Harvest/Start Cloud Server (Docker + npm dev)")]
        public static void StartCloudServerMenu()
        {
            string serverDir = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "server"));
            string docker = $"docker compose -f \"{Path.Combine(serverDir, "docker-compose.yml")}\" up -d postgres";
            string npm = $"cd /d \"{serverDir}\" && npm run dev";
            Debug.Log(
                "Run in terminal:\n" +
                $"  {docker}\n" +
                $"  {npm}\n" +
                "API: http://localhost:3000");
            EditorUtility.RevealInFinder(serverDir);
        }

        public static void RunAllBatchMode()
        {
            CreateProcessingContent();
            SetupModularUI();
            AssetDatabase.SaveAssets();
        }

        static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
                var leaf = Path.GetFileName(path);
                if (!string.IsNullOrEmpty(parent))
                    AssetDatabase.CreateFolder(parent, leaf);
            }
        }

        static ItemData LoadItem(string id) =>
            AssetDatabase.LoadAssetAtPath<ItemData>($"Assets/_Game/Resources/Items/{id}.asset");

        static RecipeData GetOrCreateRecipe(
            string path,
            string recipeId,
            (ItemData item, int amount)[] inputs,
            (ItemData outputItem, int outputAmount) output,
            float duration)
        {
            var recipe = AssetDatabase.LoadAssetAtPath<RecipeData>(path);
            if (recipe == null)
            {
                recipe = ScriptableObject.CreateInstance<RecipeData>();
                AssetDatabase.CreateAsset(recipe, path);
            }

            var so = new SerializedObject(recipe);
            so.FindProperty("recipeId").stringValue = recipeId;
            so.FindProperty("processDuration").floatValue = duration;

            var inputsProp = so.FindProperty("inputs");
            inputsProp.ClearArray();
            for (int i = 0; i < inputs.Length; i++)
            {
                inputsProp.InsertArrayElementAtIndex(i);
                var el = inputsProp.GetArrayElementAtIndex(i);
                el.FindPropertyRelative("item").objectReferenceValue = inputs[i].item;
                el.FindPropertyRelative("amount").intValue = inputs[i].amount;
            }

            var outputProp = so.FindProperty("output");
            outputProp.FindPropertyRelative("item").objectReferenceValue = output.outputItem;
            outputProp.FindPropertyRelative("amount").intValue = output.outputAmount;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(recipe);
            return recipe;
        }

        static BiomeProgressionConfig GetOrCreateBiomeProgressionConfig()
        {
            var config = AssetDatabase.LoadAssetAtPath<BiomeProgressionConfig>(BiomeProgressionConfig.DefaultAssetPath);
            if (config != null)
                return config;

            config = ScriptableObject.CreateInstance<BiomeProgressionConfig>();
            AssetDatabase.CreateAsset(config, BiomeProgressionConfig.DefaultAssetPath);
            return config;
        }

        static void EnsureBiomeZone(string objectName, BiomeId biome, GameObject gatedRoot)
        {
            if (gatedRoot == null)
                return;

            var zoneGo = GameObject.Find(objectName) ?? new GameObject(objectName);
            zoneGo.transform.SetParent(gatedRoot.transform.parent, false);
            zoneGo.transform.position = gatedRoot.transform.position;

            var zone = zoneGo.GetComponent<BiomeZone>() ?? zoneGo.AddComponent<BiomeZone>();
            var so = new SerializedObject(zone);
            so.FindProperty("biome").enumValueIndex = (int)biome;
            so.FindProperty("gatedRoots").arraySize = 1;
            so.FindProperty("gatedRoots").GetArrayElementAtIndex(0).objectReferenceValue = gatedRoot;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static TMP_Text EnsureTmpChild(Transform parent, string name, Vector2 anchoredPos, int fontSize)
        {
            var existing = parent.Find(name);
            if (existing != null)
                return existing.GetComponent<TMP_Text>();

            var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(-32f, 36f);
            rt.anchoredPosition = anchoredPos;
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.fontSize = fontSize;
            return tmp;
        }

        static BuildingData GetOrCreateBuilding(
            string path,
            string id,
            string displayName,
            RecipeData recipe,
            BiomeId biome = BiomeId.Starter)
        {
            var building = AssetDatabase.LoadAssetAtPath<BuildingData>(path);
            if (building == null)
            {
                building = ScriptableObject.CreateInstance<BuildingData>();
                AssetDatabase.CreateAsset(building, path);
            }

            var so = new SerializedObject(building);
            so.FindProperty("buildingId").stringValue = id;
            so.FindProperty("displayName").stringValue = displayName;
            so.FindProperty("requiredBiome").enumValueIndex = (int)biome;
            so.FindProperty("unlockCost").intValue = 0;
            so.FindProperty("defaultRecipe").objectReferenceValue = recipe;
            so.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(building);
            return building;
        }

        static GameObject GetOrCreateProcessingPrefab(BuildingData building, RecipeData recipe)
        {
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(ProcessingPrefabPath);
            if (existing != null)
            {
                var station = existing.GetComponent<ProcessingStation>();
                if (station != null)
                {
                    var so = new SerializedObject(station);
                    so.FindProperty("building").objectReferenceValue = building;
                    so.FindProperty("recipe").objectReferenceValue = recipe;
                    so.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(existing);
                }

                return existing;
            }

            var root = new GameObject("ProcessingStation_Home");
            root.transform.position = Vector3.zero;

            var visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "Visual";
            visual.transform.SetParent(root.transform, false);
            visual.transform.localScale = new Vector3(2f, 1.5f, 2f);
            visual.transform.localPosition = new Vector3(0f, 0.75f, 0f);
            Object.DestroyImmediate(visual.GetComponent<Collider>());

            var indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            indicator.name = "ProcessIndicator";
            indicator.transform.SetParent(root.transform, false);
            indicator.transform.localPosition = new Vector3(0f, 2.2f, 0f);
            indicator.transform.localScale = Vector3.one * 0.5f;
            Object.DestroyImmediate(indicator.GetComponent<Collider>());
            indicator.SetActive(false);

            var trigger = root.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = new Vector3(4f, 3f, 4f);
            trigger.center = new Vector3(0f, 1.5f, 0f);

            var processing = root.AddComponent<ProcessingStation>();
            var procSo = new SerializedObject(processing);
            procSo.FindProperty("building").objectReferenceValue = building;
            procSo.FindProperty("recipe").objectReferenceValue = recipe;
            procSo.FindProperty("processIndicator").objectReferenceValue = indicator.transform;
            procSo.ApplyModifiedPropertiesWithoutUndo();

            var prefab = PrefabUtility.SaveAsPrefabAsset(root, ProcessingPrefabPath);
            Object.DestroyImmediate(root);
            return prefab;
        }

        static void PlaceProcessingInLevel01(GameObject stationPrefab)
        {
            if (stationPrefab == null)
                return;

            var scene = EditorSceneManager.OpenScene(Level01Scene, OpenSceneMode.Single);

            var existing = Object.FindFirstObjectByType<ProcessingStation>();
            if (existing != null)
            {
                Debug.Log("ProcessingStation already exists in Level01.");
                EditorSceneManager.MarkSceneDirty(scene);
                return;
            }

            Vector3 position = new Vector3(12f, 0f, 12f);
            var stall = Object.FindFirstObjectByType<Stall>();
            if (stall != null)
                position = stall.transform.position + new Vector3(4f, 0f, 0f);

            var islandManager = Object.FindFirstObjectByType<IslandManager>();
            Transform parent = islandManager != null ? islandManager.transform : null;

            var instance = PrefabUtility.InstantiatePrefab(stationPrefab, scene) as GameObject;
            if (instance == null)
                return;

            instance.transform.SetParent(parent, true);
            instance.transform.position = position;
            instance.name = "StarterProcessor";

            Undo.RegisterCreatedObjectUndo(instance, "Place Processing Station");
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        static T GetOrAddComponent<T>(GameObject go) where T : Component
        {
            var c = go.GetComponent<T>();
            return c != null ? c : go.AddComponent<T>();
        }

        static void WireHudController(HudController hud, UIManager uiManager)
        {
            var uiSo = new SerializedObject(uiManager);
            var hudSo = new SerializedObject(hud);

            CopyRef(uiSo, hudSo, "joystick");
            CopyRef(uiSo, hudSo, "activityButton");
            CopyRef(uiSo, hudSo, "farmingButton");
            CopyRef(uiSo, hudSo, "itemInfoPrefab");
            CopyRef(uiSo, hudSo, "itemInfoContainer");

            var farmingIcons = uiSo.FindProperty("farmingIcons");
            var hudIcons = hudSo.FindProperty("farmingIcons");
            hudIcons.arraySize = farmingIcons.arraySize;
            for (int i = 0; i < farmingIcons.arraySize; i++)
                hudIcons.GetArrayElementAtIndex(i).objectReferenceValue =
                    farmingIcons.GetArrayElementAtIndex(i).objectReferenceValue;

            var huds = uiSo.FindProperty("huds");
            var hudElements = hudSo.FindProperty("hudElements");
            hudElements.arraySize = huds.arraySize;
            for (int i = 0; i < huds.arraySize; i++)
                hudElements.GetArrayElementAtIndex(i).objectReferenceValue =
                    huds.GetArrayElementAtIndex(i).objectReferenceValue;

            hudSo.ApplyModifiedPropertiesWithoutUndo();
        }

        static void CopyRef(SerializedObject from, SerializedObject to, string prop)
        {
            to.FindProperty(prop).objectReferenceValue = from.FindProperty(prop).objectReferenceValue;
        }

        static InventoryWindow EnsureInventoryWindow(Transform canvasRoot, HudController hud)
        {
            var existing = canvasRoot.GetComponentInChildren<InventoryWindow>(true);
            if (existing != null)
                return WireInventory(existing, hud);

            var settingsBtn = canvasRoot.GetComponentInChildren<Button>(true);
            // Find SettingsButton by name
            Transform settingsTransform = null;
            foreach (var btn in canvasRoot.GetComponentsInChildren<Button>(true))
            {
                if (btn.gameObject.name == "SettingsButton")
                {
                    settingsTransform = btn.transform;
                    break;
                }
            }

            var windowGo = new GameObject("InventoryWindow", typeof(RectTransform));
            windowGo.transform.SetParent(canvasRoot, false);
            var windowRt = windowGo.GetComponent<RectTransform>();
            windowRt.anchorMin = Vector2.zero;
            windowRt.anchorMax = Vector2.one;
            windowRt.offsetMin = Vector2.zero;
            windowRt.offsetMax = Vector2.zero;

            var panelGo = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panelGo.transform.SetParent(windowGo.transform, false);
            var panelRt = panelGo.GetComponent<RectTransform>();
            panelRt.anchorMin = new Vector2(0.5f, 0.5f);
            panelRt.anchorMax = new Vector2(0.5f, 0.5f);
            panelRt.sizeDelta = new Vector2(520f, 640f);
            panelRt.anchoredPosition = Vector2.zero;
            panelGo.GetComponent<Image>().color = new Color(0.12f, 0.14f, 0.18f, 0.95f);

            var gridGo = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
            gridGo.transform.SetParent(panelGo.transform, false);
            var gridRt = gridGo.GetComponent<RectTransform>();
            gridRt.anchorMin = new Vector2(0f, 0f);
            gridRt.anchorMax = new Vector2(1f, 1f);
            gridRt.offsetMin = new Vector2(16f, 56f);
            gridRt.offsetMax = new Vector2(-16f, -72f);
            var grid = gridGo.GetComponent<GridLayoutGroup>();
            grid.cellSize = new Vector2(96f, 96f);
            grid.spacing = new Vector2(8f, 8f);
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 4;

            var filterGo = new GameObject("CategoryFilter", typeof(RectTransform), typeof(TMP_Dropdown));
            filterGo.transform.SetParent(panelGo.transform, false);
            var filterRt = filterGo.GetComponent<RectTransform>();
            filterRt.anchorMin = new Vector2(0f, 1f);
            filterRt.anchorMax = new Vector2(1f, 1f);
            filterRt.pivot = new Vector2(0.5f, 1f);
            filterRt.sizeDelta = new Vector2(-32f, 40f);
            filterRt.anchoredPosition = new Vector2(0f, -20f);

            var hudSo = new SerializedObject(hud);
            var itemInfoPrefab = hudSo.FindProperty("itemInfoPrefab").objectReferenceValue as ItemInfo;

            var toggleGo = new GameObject("InventoryButton", typeof(RectTransform), typeof(Image), typeof(Button));
            toggleGo.transform.SetParent(canvasRoot, false);
            var toggleRt = toggleGo.GetComponent<RectTransform>();
            toggleRt.anchorMin = new Vector2(0f, 1f);
            toggleRt.anchorMax = new Vector2(0f, 1f);
            toggleRt.pivot = new Vector2(0f, 1f);
            toggleRt.sizeDelta = new Vector2(100f, 100f);
            if (settingsTransform != null)
            {
                toggleRt.anchoredPosition = settingsTransform.GetComponent<RectTransform>().anchoredPosition + new Vector2(0f, -110f);
            }
            else
                toggleRt.anchoredPosition = new Vector2(50f, -460f);

            toggleGo.GetComponent<Image>().color = new Color(0.25f, 0.55f, 0.85f, 1f);
            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGo.transform.SetParent(toggleGo.transform, false);
            var label = labelGo.GetComponent<TextMeshProUGUI>();
            label.text = "INV";
            label.fontSize = 28;
            label.alignment = TextAlignmentOptions.Center;
            var labelRt = labelGo.GetComponent<RectTransform>();
            labelRt.anchorMin = Vector2.zero;
            labelRt.anchorMax = Vector2.one;
            labelRt.offsetMin = Vector2.zero;
            labelRt.offsetMax = Vector2.zero;

            var inventory = windowGo.AddComponent<InventoryWindow>();
            var invSo = new SerializedObject(inventory);
            invSo.FindProperty("root").objectReferenceValue = panelGo;
            invSo.FindProperty("toggleButton").objectReferenceValue = toggleGo.GetComponent<Button>();
            invSo.FindProperty("gridContainer").objectReferenceValue = gridGo.transform;
            invSo.FindProperty("itemSlotPrefab").objectReferenceValue = itemInfoPrefab;
            invSo.FindProperty("categoryFilter").objectReferenceValue = filterGo.GetComponent<TMP_Dropdown>();
            invSo.ApplyModifiedPropertiesWithoutUndo();

            windowGo.SetActive(false);
            panelGo.SetActive(false);
            return inventory;
        }

        static InventoryWindow WireInventory(InventoryWindow inventory, HudController hud)
        {
            var hudSo = new SerializedObject(hud);
            var itemInfoPrefab = hudSo.FindProperty("itemInfoPrefab").objectReferenceValue;

            var invSo = new SerializedObject(inventory);
            if (invSo.FindProperty("itemSlotPrefab").objectReferenceValue == null)
                invSo.FindProperty("itemSlotPrefab").objectReferenceValue = itemInfoPrefab;
            invSo.ApplyModifiedPropertiesWithoutUndo();
            return inventory;
        }
    }
}
