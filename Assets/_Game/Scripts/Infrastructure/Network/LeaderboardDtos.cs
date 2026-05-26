using System;

namespace IslandHarvest.Game
{
    [Serializable]
    public class LeaderboardApiResponse
    {
        public LeaderboardEntryData[] entries;
    }

    [Serializable]
    public class LeaderboardEntryData
    {
        public int rank;
        public string playerId;
        public string displayName;
        public int coins;
        public int saveVersion;
        public string updatedAt;
    }
}
