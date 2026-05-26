using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    [System.Serializable]
    public class SkinCatalogEntry
    {
        [Tooltip("File name in Resources/Skins (without .asset), e.g. Skin_Green")]
        public string assetFileName = "Skin_New";

        public string skinId = "skin_new";
        public string displayName = "New Skin";
        public Material skinMaterial;
        public Material transparentMaterial;
        public int price = 500;
        public bool unlockedByDefault;
        public SkinPurchaseType purchaseType = SkinPurchaseType.Coins;
    }

    [CreateAssetMenu(fileName = "SkinCatalog", menuName = "Island Harvest/Skin Catalog")]
    public class SkinCatalogConfig : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/_Game/Data/SkinCatalog.asset";

        [SerializeField] private List<SkinCatalogEntry> entries = new List<SkinCatalogEntry>();

        public IReadOnlyList<SkinCatalogEntry> Entries => entries;

        public List<SkinCatalogEntry> GetMutableEntries() => entries;
    }
}
