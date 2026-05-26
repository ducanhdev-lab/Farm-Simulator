using System;

namespace IslandHarvest.Game
{
    [Serializable]
    public class IslandSnapshotData
    {
        public string ownerPlayerId;
        public string displayName;
        public int coins;
        public int unlockedIslandCount;
        public string equippedSkinId;
        public string capturedUtc;
    }

    [Serializable]
    public class IslandSnapshotPayload
    {
        public string snapshotJson;
    }

    [Serializable]
    public class IslandSnapshotApiResponse
    {
        public IslandSnapshotData snapshot;
    }
}
