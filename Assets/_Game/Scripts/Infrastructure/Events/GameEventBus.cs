using System;

namespace IslandHarvest.Game
{
    public static class GameEventBus
    {
        public static event Action<int> ResourceGathered;
        public static event Action<string, int> ItemProcessed;
        public static event Action<int> SoldAtStall;
        public static event Action IslandUnlocked;
        public static event Action<string, int> BuildingUpgraded;
        public static event Action<string> QuestProgressed;

        public static void RaiseResourceGathered(int amount = 1)
        {
            ResourceGathered?.Invoke(amount);
            GameplayEvents.RaiseCropHarvested(amount);
        }

        public static void RaiseItemProcessed(string recipeId, int outputAmount) =>
            ItemProcessed?.Invoke(recipeId, outputAmount);

        public static void RaiseSoldAtStall(int coinAmount)
        {
            SoldAtStall?.Invoke(coinAmount);
            GameplayEvents.RaiseCoinsEarnedAtStall(coinAmount);
        }

        public static void RaiseIslandUnlocked()
        {
            IslandUnlocked?.Invoke();
            GameplayEvents.RaiseIslandUnlocked();
        }

        public static void RaiseBuildingUpgraded(string buildingId, int newLevel) =>
            BuildingUpgraded?.Invoke(buildingId, newLevel);

        public static void RaiseQuestProgressed(string questId) =>
            QuestProgressed?.Invoke(questId);
    }
}
