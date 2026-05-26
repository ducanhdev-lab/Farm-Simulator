namespace IslandHarvest.Game
{
    /// <summary>
    /// Subscribes GameEventBus to IAnalyticsService.
    /// </summary>
    public static class AnalyticsEventBridge
    {
        private static IAnalyticsService analytics;
        private static bool isBound;

        public static void Bind(IAnalyticsService service)
        {
            if (service == null || isBound && analytics == service)
                return;

            Unbind();
            analytics = service;
            GameEventBus.ResourceGathered += OnResourceGathered;
            GameEventBus.SoldAtStall += OnSoldAtStall;
            GameEventBus.IslandUnlocked += OnIslandUnlocked;
            GameEventBus.BuildingUpgraded += OnBuildingUpgraded;
            GameEventBus.ItemProcessed += OnItemProcessed;
            isBound = true;
        }

        public static void Unbind()
        {
            if (!isBound)
                return;

            GameEventBus.ResourceGathered -= OnResourceGathered;
            GameEventBus.SoldAtStall -= OnSoldAtStall;
            GameEventBus.IslandUnlocked -= OnIslandUnlocked;
            GameEventBus.BuildingUpgraded -= OnBuildingUpgraded;
            GameEventBus.ItemProcessed -= OnItemProcessed;
            isBound = false;
            analytics = null;
        }

        private static void OnResourceGathered(int amount) => analytics?.TrackResourceGathered(amount);
        private static void OnSoldAtStall(int coins) => analytics?.TrackSoldAtStall(coins);
        private static void OnIslandUnlocked() => analytics?.TrackIslandUnlocked();
        private static void OnBuildingUpgraded(string id, int level) => analytics?.TrackBuildingUpgraded(id, level);
        private static void OnItemProcessed(string recipeId, int output) => analytics?.TrackItemProcessed(recipeId, output);
    }
}
