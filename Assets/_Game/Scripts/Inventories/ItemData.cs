using UnityEngine;

namespace IslandHarvest.Game
{
    public enum ResourceCategory
    {
        Crop,
        Animal,
        Fish,
        Wood,
        Mineral,
        Processed,
        Other
    }

    public enum ItemRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic
    }

    [CreateAssetMenu()]
    public class ItemData : ScriptableObject
    {
        [SerializeField] private string itemId;
        [SerializeField] private int price;
        [SerializeField] private int maxStack = 99;
        [SerializeField] private ResourceCategory category = ResourceCategory.Other;
        [SerializeField] private ItemRarity rarity = ItemRarity.Common;
        [SerializeField] private Sprite icon;
        [SerializeField] private GameObject drop;

        public string ItemId => itemId;
        public int Price => price;
        public int MaxStack => maxStack;
        public ResourceCategory Category => category;
        public ItemRarity Rarity => rarity;
        public Sprite Icon => icon;
        public GameObject Drop => drop;
    }
}
