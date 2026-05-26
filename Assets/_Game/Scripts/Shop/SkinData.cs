using UnityEngine;

namespace IslandHarvest.Game
{
    public enum SkinPurchaseType
    {
        Free,
        Coins,
        IapOnly
    }

    [CreateAssetMenu(fileName = "SkinData", menuName = "Island Harvest/Skin Data")]
    public class SkinData : ScriptableObject
    {
        [SerializeField] private string skinId;
        [SerializeField] private string displayName;
        [SerializeField] private Material skinMaterial;
        [SerializeField] private Material transparentMaterial;
        [SerializeField] private int price;
        [SerializeField] private bool unlockedByDefault;
        [SerializeField] private SkinPurchaseType purchaseType = SkinPurchaseType.Coins;

        public string SkinId => string.IsNullOrEmpty(skinId) ? name.ToLower() : skinId;
        public string DisplayName => string.IsNullOrEmpty(displayName) ? name : displayName;
        public Material SkinMaterial => skinMaterial;
        public Material TransparentMaterial => transparentMaterial != null ? transparentMaterial : skinMaterial;
        public int Price => price;
        public bool UnlockedByDefault => unlockedByDefault;
        public SkinPurchaseType PurchaseType => purchaseType;
        public bool ShowInCoinShop => purchaseType != SkinPurchaseType.IapOnly;

        public void SetEditorDefaults(string id, string title, Material opaque, Material transparent, int coinPrice, bool free)
        {
            skinId = id;
            displayName = title;
            skinMaterial = opaque;
            transparentMaterial = transparent;
            price = coinPrice;
            unlockedByDefault = free;
            purchaseType = free ? SkinPurchaseType.Free : SkinPurchaseType.Coins;
        }
    }
}
