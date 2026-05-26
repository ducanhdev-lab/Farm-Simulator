namespace IslandHarvest.Game
{
    public interface IAnalyticsService
    {
        void TrackResourceGathered(int amount);
        void TrackSoldAtStall(int coins);
        void TrackIslandUnlocked();
        void TrackBuildingUpgraded(string buildingId, int level);
        void TrackItemProcessed(string recipeId, int outputAmount);
    }
}
