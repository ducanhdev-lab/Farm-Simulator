namespace IslandHarvest.Game
{
    [System.Serializable]
    public class Item
    {
        public string ItemId;
        public int Amount;

        public ItemData Data => ItemDatabase.GetItemDataById(ItemId);

        public Item(string itemId, int amount)
        {
            ItemId = itemId;
            Amount = amount;
        }
    }
}
