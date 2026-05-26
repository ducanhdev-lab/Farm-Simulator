using System;
using System.Collections.Generic;

namespace IslandHarvest.Game
{
    [Serializable]
    public class PlayerProfile
    {
        public string profileId;
        public int saveVersion;
        public string lastSaveUtc;
        public GameData homeWorld;
        public CosmeticsData cosmetics;
        public List<EventInstanceData> eventInstances = new List<EventInstanceData>();

        public PlayerProfile()
        {
            profileId = Guid.NewGuid().ToString("N");
            saveVersion = SaveSystem.CurrentSaveVersion;
            lastSaveUtc = DateTime.UtcNow.ToString("o");
            cosmetics = new CosmeticsData();
            eventInstances = new List<EventInstanceData>();
        }

        public PlayerProfile(int startingCoins, List<bool> unlockedIslands) : this()
        {
            homeWorld = new GameData(startingCoins, unlockedIslands);
        }

        public void TouchSaveTimestamp()
        {
            lastSaveUtc = DateTime.UtcNow.ToString("o");
            saveVersion = SaveSystem.CurrentSaveVersion;
        }
    }

    [Serializable]
    public class EventInstanceData
    {
        public string eventId;
        public string sceneName;
        public int coins;
        public List<InventoryData> inventoryDatabase = new List<InventoryData>();
        public string lastPlayedUtc;

        public EventInstanceData(string eventId, string sceneName)
        {
            this.eventId = eventId;
            this.sceneName = sceneName;
            lastPlayedUtc = DateTime.UtcNow.ToString("o");
        }
    }
}
