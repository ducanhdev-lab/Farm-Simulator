using System;

namespace IslandHarvest.Game
{
    [Serializable]
    public class LiveEventsApiResponse
    {
        public LiveEventData[] events;
    }

    [Serializable]
    public class LiveEventData
    {
        public string eventId;
        public string sceneName;
        public string displayName;
        public string description;
        public bool alwaysAvailable;
        public int requiredHomeCoins;
        public string startsAt;
        public string endsAt;
    }
}
