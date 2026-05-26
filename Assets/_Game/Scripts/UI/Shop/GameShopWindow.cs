using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IslandHarvest.Game
{
    public class GameShopWindow : MonoBehaviour
    {
        public enum ShopTab
        {
            Skins,
            IAP
        }

        [SerializeField] private RectTransform mainPanel;
        [SerializeField] private TMP_Text coinBalanceText;
        [SerializeField] private Button tabSkinsButton;
        [SerializeField] private Button tabIAPButton;
        [SerializeField] private GameObject skinsPanel;
        [SerializeField] private GameObject iapPanel;
        [SerializeField] private Transform skinsItemContainer;
        [SerializeField] private Transform iapItemContainer;
        [SerializeField] private SkinShopItemRow itemRowPrefab;
        [SerializeField] private IAPShopItemRow iapItemRowPrefab;
        [SerializeField] private Button closeButton;

        private readonly List<SkinShopItemRow> skinRows = new List<SkinShopItemRow>();
        private readonly List<IAPShopItemRow> iapRows = new List<IAPShopItemRow>();
        private SkinData[] currentCatalog;
        private ShopTab activeTab = ShopTab.Skins;
        private IAPService iapService;

        private void Awake()
        {
            var canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
                if (canvas != null)
                    transform.SetParent(canvas.transform, false);
            }

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (tabSkinsButton != null)
                tabSkinsButton.onClick.AddListener(() => SetTab(ShopTab.Skins));

            if (tabIAPButton != null)
                tabIAPButton.onClick.AddListener(() => SetTab(ShopTab.IAP));

            Hide();
        }

        private void OnEnable()
        {
            iapService = IAPService.EnsureInstance();
            if (iapService != null)
            {
                iapService.OnPurchaseSucceeded += HandleIAPPurchaseFinished;
                iapService.OnPurchaseFailed += HandleIAPPurchaseFailed;
            }
        }

        private void OnDisable()
        {
            if (iapService != null)
            {
                iapService.OnPurchaseSucceeded -= HandleIAPPurchaseFinished;
                iapService.OnPurchaseFailed -= HandleIAPPurchaseFailed;
            }
        }

        /// <summary>Opens the shop on the main menu (only supported context).</summary>
        public void ShowForMainMenu()
        {
            currentCatalog = Resources.LoadAll<SkinData>("Skins");
            activeTab = ShopTab.Skins;

            gameObject.SetActive(true);
            IAPService.EnsureInstance();
            SetTab(ShopTab.Skins);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void SetTab(ShopTab tab)
        {
            activeTab = tab;

            if (skinsPanel != null)
                skinsPanel.SetActive(tab == ShopTab.Skins);
            if (iapPanel != null)
                iapPanel.SetActive(tab == ShopTab.IAP);

            Refresh();
        }

        private void Refresh()
        {
            UpdateCoinBalance();

            if (activeTab == ShopTab.Skins)
                RefreshSkins();
            else
                RefreshIAP();
        }

        private void UpdateCoinBalance()
        {
            if (coinBalanceText == null)
                return;

            int coins = CosmeticsSave.GetLatestSaveCoins();
            coinBalanceText.text = $"Coins: {coins.ToAbbreviatedString()}";
        }

        private void RefreshSkins()
        {
            ClearSkinRows();

            if (currentCatalog == null || itemRowPrefab == null || skinsItemContainer == null)
                return;

            int coins = CosmeticsSave.GetLatestSaveCoins();
            string equippedId = CosmeticsSave.EquippedSkinId;

            foreach (var skin in currentCatalog)
            {
                if (skin == null || !skin.ShowInCoinShop) continue;

                var row = Instantiate(itemRowPrefab, skinsItemContainer);
                row.SetupForMainMenu(skin, equippedId, coins, Refresh);
                skinRows.Add(row);
            }
        }

        private void RefreshIAP()
        {
            ClearIAPRows();

            if (iapItemRowPrefab == null || iapItemContainer == null)
                return;

            var products = iapService != null ? iapService.GetCatalog() : Resources.LoadAll<IAPProductData>("IAP");
            string equippedId = CosmeticsSave.EquippedSkinId;

            foreach (var product in products)
            {
                if (product == null) continue;

                var row = Instantiate(iapItemRowPrefab, iapItemContainer);
                row.Setup(product, equippedId, Refresh);
                iapRows.Add(row);
            }
        }

        private void ClearSkinRows()
        {
            foreach (var row in skinRows)
            {
                if (row != null)
                    Destroy(row.gameObject);
            }
            skinRows.Clear();
        }

        private void ClearIAPRows()
        {
            foreach (var row in iapRows)
            {
                if (row != null)
                    Destroy(row.gameObject);
            }
            iapRows.Clear();
        }

        private void HandleIAPPurchaseFinished(string productId)
        {
            AudioManager.Instance?.PlaySFX(AudioID.Cash);
            Refresh();
        }

        private void HandleIAPPurchaseFailed(string productId, string reason)
        {
            Debug.LogWarning($"IAP failed ({productId}): {reason}");
            AudioManager.Instance?.PlaySFX(AudioID.UI_Decline);
            Refresh();
        }
    }
}
