namespace IslandHarvest.Game
{
    /// <summary>
    /// Offline defaults until Phase 3 live remote config is wired.
    /// </summary>
    public class LocalRemoteConfigService : IRemoteConfigService
    {
        public bool IsReady => true;

        public float GetStallPriceMultiplier(string itemId, float defaultMultiplier) => defaultMultiplier;

        public int GetIslandBasePrice(int defaultPrice) => defaultPrice;
    }
}
