using UnityEngine;

namespace IslandHarvest.Game
{
    /// <summary>
    /// Development analytics sink; replace with SDK adapter in production.
    /// </summary>
    public class DebugAnalyticsService : IAnalyticsService
    {
        private readonly bool logEvents;

        public DebugAnalyticsService(bool logEvents = true) => this.logEvents = logEvents;

        public void TrackResourceGathered(int amount) => Log("resource_gathered", amount);
        public void TrackSoldAtStall(int coins) => Log("sold_at_stall", coins);
        public void TrackIslandUnlocked() => Log("island_unlocked", 1);
        public void TrackBuildingUpgraded(string buildingId, int level) => Log($"building_upgraded:{buildingId}", level);
        public void TrackItemProcessed(string recipeId, int outputAmount) => Log($"item_processed:{recipeId}", outputAmount);

        private void Log(string name, int value)
        {
            if (logEvents)
                Debug.Log($"[Analytics] {name} = {value}");
        }
    }
}
