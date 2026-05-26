using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_PURCHASING
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Extension;
#endif

namespace IslandHarvest.Game
{
    public class IAPService : MonoBehaviour
#if UNITY_PURCHASING
        , IStoreListener
#endif
    {
        public static IAPService Instance { get; private set; }

        public event Action OnInitialized;
        public event Action<string> OnPurchaseSucceeded;
        public event Action<string, string> OnPurchaseFailed;

        public bool IsInitialized { get; private set; }
        public bool IsPurchasePending { get; private set; }

#if UNITY_PURCHASING
        private IStoreController storeController;
        private IExtensionProvider storeExtensionProvider;
#endif

        private readonly Dictionary<string, IAPProductData> productLookup = new Dictionary<string, IAPProductData>();
        private IAPProductData[] catalog = Array.Empty<IAPProductData>();

        public static IAPService EnsureInstance()
        {
            if (Instance != null)
                return Instance;

            var existing = FindFirstObjectByType<IAPService>();
            if (existing != null)
            {
                Instance = existing;
                return Instance;
            }

            var go = new GameObject(nameof(IAPService));
            DontDestroyOnLoad(go);
            return go.AddComponent<IAPService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadCatalog();
            InitializePurchasing();
        }

        private void LoadCatalog()
        {
            catalog = Resources.LoadAll<IAPProductData>("IAP");
            productLookup.Clear();

            foreach (var product in catalog)
            {
                if (product == null) continue;
                productLookup[product.StoreProductId] = product;
            }
        }

        private void InitializePurchasing()
        {
#if UNITY_PURCHASING
            if (IsInitialized || catalog.Length == 0)
                return;

            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            foreach (var product in catalog)
            {
                if (product == null) continue;

                ProductType type = product.Kind == IAPProductKind.CoinPack
                    ? ProductType.Consumable
                    : ProductType.NonConsumable;

                builder.AddProduct(product.StoreProductId, type);
            }

            UnityPurchasing.Initialize(this, builder);
#else
            IsInitialized = catalog.Length > 0;
            OnInitialized?.Invoke();
#endif
        }

        public IAPProductData[] GetCatalog() => catalog;

        public bool TryGetProduct(string storeProductId, out IAPProductData product) =>
            productLookup.TryGetValue(storeProductId, out product);

        public string GetLocalizedPrice(string storeProductId)
        {
#if UNITY_PURCHASING
            if (storeController != null &&
                storeController.products != null &&
                storeController.products.WithID(storeProductId) is { available: true } product)
            {
                return product.metadata.localizedPriceString;
            }
#endif
            return productLookup.ContainsKey(storeProductId) ? "—" : "N/A";
        }

        public void Purchase(string storeProductId)
        {
            if (IsPurchasePending)
                return;

            if (!productLookup.TryGetValue(storeProductId, out IAPProductData productData))
            {
                OnPurchaseFailed?.Invoke(storeProductId, "Unknown product");
                return;
            }

#if UNITY_PURCHASING
            if (!IsInitialized || storeController == null)
            {
                OnPurchaseFailed?.Invoke(storeProductId, "Store not ready");
                return;
            }

            IsPurchasePending = true;
            storeController.InitiatePurchase(storeProductId);
#else
#if UNITY_EDITOR
            SimulatePurchase(productData);
#else
            OnPurchaseFailed?.Invoke(storeProductId, "Purchasing unavailable");
#endif
#endif
        }

        public void RestorePurchases()
        {
#if UNITY_PURCHASING
            if (!IsInitialized || storeExtensionProvider == null)
                return;

            var apple = storeExtensionProvider.GetExtension<IAppleExtensions>();
            if (apple != null)
            {
                apple.RestoreTransactions((success, message) =>
                {
                    ReapplyNonConsumableEntitlements();
                });
                return;
            }

            ReapplyNonConsumableEntitlements();
#else
            ReapplyNonConsumableEntitlements();
#endif
        }

        public void ReapplyNonConsumableEntitlements()
        {
#if UNITY_PURCHASING
            if (storeController == null)
                return;

            foreach (var product in catalog)
            {
                if (product == null || product.Kind != IAPProductKind.PremiumSkin)
                    continue;

                var storeProduct = storeController.products.WithID(product.StoreProductId);
                if (storeProduct != null && storeProduct.hasReceipt)
                    CosmeticsSave.GrantIAPProduct(product, recordProductId: true);
            }
#else
            foreach (var product in catalog)
            {
                if (product != null && product.Kind == IAPProductKind.PremiumSkin &&
                    CosmeticsSave.HasPurchasedIAP(product.StoreProductId))
                {
                    CosmeticsSave.GrantIAPProduct(product, recordProductId: false);
                }
            }
#endif
        }

#if UNITY_PURCHASING
        public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
        {
            storeController = controller;
            storeExtensionProvider = extensions;
            IsInitialized = true;
            ReapplyNonConsumableEntitlements();
            OnInitialized?.Invoke();
        }

        public void OnInitializeFailed(InitializationFailureReason error)
        {
            Debug.LogWarning($"IAP init failed: {error}");
            IsInitialized = false;
        }

        public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
        {
            IsPurchasePending = false;
            string id = args.purchasedProduct.definition.id;

            if (productLookup.TryGetValue(id, out IAPProductData product))
            {
                CosmeticsSave.GrantIAPProduct(product, recordProductId: product.Kind == IAPProductKind.PremiumSkin);
                OnPurchaseSucceeded?.Invoke(id);
            }
            else
            {
                OnPurchaseFailed?.Invoke(id, "Product not in catalog");
            }

            return PurchaseProcessingResult.Complete;
        }

        public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
        {
            IsPurchasePending = false;
            OnPurchaseFailed?.Invoke(product?.definition?.id ?? "unknown", reason.ToString());
        }

#endif

#if UNITY_EDITOR && !UNITY_PURCHASING
        private void SimulatePurchase(IAPProductData product)
        {
            CosmeticsSave.GrantIAPProduct(product, recordProductId: product.Kind == IAPProductKind.PremiumSkin);
            OnPurchaseSucceeded?.Invoke(product.StoreProductId);
        }
#endif
    }
}
