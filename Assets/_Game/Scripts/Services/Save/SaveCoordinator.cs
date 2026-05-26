using System;
using System.Collections.Generic;

namespace IslandHarvest.Game
{
    public class SaveCoordinator : ISaveCoordinator
    {
        private readonly List<Action<GameData>> collectors = new List<Action<GameData>>();
        private readonly IPlayerProfileService profileService;

        public event Action<GameData> OnCollectSaveState;

        public SaveCoordinator(IPlayerProfileService profileService)
        {
            this.profileService = profileService;
        }

        public void RegisterCollector(Action<GameData> collector)
        {
            if (collector != null && !collectors.Contains(collector))
                collectors.Add(collector);
        }

        public void UnregisterCollector(Action<GameData> collector) => collectors.Remove(collector);

        public void RequestSave() => profileService?.RequestSave();

        public void SaveNow()
        {
            var data = profileService?.ActiveWorldData;
            if (data == null)
                return;

            foreach (var collector in collectors)
                collector?.Invoke(data);

            OnCollectSaveState?.Invoke(data);
            profileService.SaveNow();
        }
    }
}
