using System;

namespace IslandHarvest.Game
{
    public interface ISaveCoordinator
    {
        event Action<GameData> OnCollectSaveState;

        void RegisterCollector(Action<GameData> collector);
        void UnregisterCollector(Action<GameData> collector);
        void RequestSave();
        void SaveNow();
    }
}
