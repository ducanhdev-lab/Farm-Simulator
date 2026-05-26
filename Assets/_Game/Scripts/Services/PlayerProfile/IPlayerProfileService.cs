using System;

namespace IslandHarvest.Game
{
    public interface IPlayerProfileService
    {
        PlayerProfile Profile { get; }
        GameData ActiveWorldData { get; }
        bool IsHomeWorld { get; }

        event Action<GameData> OnBeforeSave;
        event Action<PlayerProfile> OnProfileSaved;

        void InitializeForScene(string sceneName, int startingCoins, System.Collections.Generic.List<bool> defaultUnlockedIslands);
        void RequestSave();
        void SaveNow();
        EventInstanceData GetOrCreateEventInstance(string eventId, string sceneName);
    }
}
