using UnityEngine;

namespace IslandHarvest.Game
{
    public enum IAPProductKind
    {
        CoinPack,
        PremiumSkin
    }

    [CreateAssetMenu(fileName = "IAPProduct", menuName = "Island Harvest/IAP Product")]
    public class IAPProductData : ScriptableObject
    {
        [SerializeField] private string storeProductId;
        [SerializeField] private IAPProductKind kind;
        [SerializeField] private string displayName;
        [SerializeField] private int coinAmount;
        [SerializeField] private string linkedSkinId;
        [SerializeField] private Sprite icon;

        public string StoreProductId => string.IsNullOrEmpty(storeProductId) ? name.ToLower() : storeProductId;
        public IAPProductKind Kind => kind;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public int CoinAmount => coinAmount;
        public string LinkedSkinId => linkedSkinId;
        public Sprite Icon => icon;

        public void SetEditorDefaults(string productId, IAPProductKind productKind, string title, int coins, string skinId)
        {
            storeProductId = productId;
            kind = productKind;
            displayName = title;
            coinAmount = coins;
            linkedSkinId = skinId;
        }
    }
}
