using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace IslandHarvest.Game
{
    public static class MainMenuShopSetup
    {
        const string MainMenuScene = "Assets/_Game/Scenes/_MainMenu.unity";

        [MenuItem("Tools/Island Harvest/Setup Main Menu Shop Window")]
        public static void SetupMainMenuShop()
        {
            SkinShopSetup.CreateAllInternal();

            var scene = EditorSceneManager.OpenScene(MainMenuScene, OpenSceneMode.Single);

            var shopWindow = Object.FindFirstObjectByType<GameShopWindow>(FindObjectsInactive.Include);
            if (shopWindow == null)
            {
                Debug.LogError("GameShopWindow not found in _MainMenu scene. Run Create Skin Shop Assets, then add SkinShopWindow prefab under the UI Canvas.");
                return;
            }

            WireMainMenu(shopWindow);
            FixShopButton();

            shopWindow.gameObject.SetActive(false);
            EditorSceneManager.MarkSceneDirty(scene);
            Debug.Log("Main Menu shop wired to ShopButton.");
        }

        static void WireMainMenu(GameShopWindow shopWindow)
        {
            var mainMenu = Object.FindFirstObjectByType<MainMenu>();
            if (mainMenu == null)
            {
                Debug.LogWarning("MainMenu component not found.");
                return;
            }

            var shopButton = GameObject.Find("ShopButton")?.GetComponent<Button>();

            var so = new SerializedObject(mainMenu);
            so.FindProperty("shopButton").objectReferenceValue = shopButton;
            so.FindProperty("shopWindow").objectReferenceValue = shopWindow;
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        static void FixShopButton()
        {
            var shopButton = GameObject.Find("ShopButton")?.GetComponent<Button>();
            if (shopButton != null)
                shopButton.onClick = new Button.ButtonClickedEvent();
        }
    }
}
