namespace IslandHarvest.Game
{
    public interface IEconomyService
    {
        int GetSellPrice(ItemData item, int quantity);
        bool TrySellAtStall(ItemData item, int quantity, out int coinsEarned);
        bool TryPurchaseUpgrade(int cost);
    }
}
