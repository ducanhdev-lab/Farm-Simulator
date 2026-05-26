namespace IslandHarvest.Game
{
    public interface IRemoteConfigService
    {
        bool IsReady { get; }
        float GetStallPriceMultiplier(string itemId, float defaultMultiplier);
        int GetIslandBasePrice(int defaultPrice);
    }
}
