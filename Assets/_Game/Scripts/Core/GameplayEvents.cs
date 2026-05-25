using System;

namespace IslandHarvest.Game
{
    /// <summary>
    /// Lightweight gameplay events for quest and analytics hooks.
    /// </summary>
    public static class GameplayEvents
    {
        public static event Action<int> OnCropHarvested;
        public static event Action<int> OnCoinsEarnedAtStall;
        public static event Action OnIslandUnlocked;

        public static void RaiseCropHarvested(int amount = 1) => OnCropHarvested?.Invoke(amount);
        public static void RaiseCoinsEarnedAtStall(int amount) => OnCoinsEarnedAtStall?.Invoke(amount);
        public static void RaiseIslandUnlocked() => OnIslandUnlocked?.Invoke();
    }
}
