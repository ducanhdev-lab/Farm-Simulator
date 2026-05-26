using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    [System.Serializable]
    public class GameData
    {
        public int saveVersion;
        public int Coin;
        public List<bool> UnlockedIslands;
        public List<PurchaserData> Purchasers;
        public List<InventoryData> InventoryDatabase;
        public List<SiloData> SiloDatabase;
        public int[] AnimalLevels;
        public string lastQuestResetDate;
        public List<QuestProgressData> QuestProgress;
        public List<string> ownedSkinIds;
        public string equippedSkinId;
        public List<string> completedIAPProductIds;

        public GameData(int coin, List<bool> unlockedIslands)
        {
            saveVersion = SaveSystem.CurrentSaveVersion;
            Coin = coin;
            UnlockedIslands = unlockedIslands;
            Purchasers = new List<PurchaserData>();
            InventoryDatabase = new List<InventoryData>();
            SiloDatabase = new List<SiloData>();
            AnimalLevels = new int[System.Enum.GetValues(typeof(AnimalType)).Length];
            lastQuestResetDate = string.Empty;
            QuestProgress = new List<QuestProgressData>();
            ownedSkinIds = new List<string> { PlayerSkinManager.DefaultSkinId };
            equippedSkinId = PlayerSkinManager.DefaultSkinId;
            completedIAPProductIds = new List<string>();
        }
    }

    [System.Serializable]
    public class InventoryData
    {
        public string InventoryId;
        public List<Item> Items;

        public InventoryData(string inventoryId, List<Item> items)
        {
            InventoryId = inventoryId;
            Items = items;
        }
    }

    [System.Serializable]
    public class SiloData
    {
        public string SiloId;
        public int SiloLevel;
        public List<int> FarmerLevels;

        public SiloData(string siloId, int siloLevel, List<int> farmerLevels)
        {
            SiloId = siloId;
            SiloLevel = siloLevel;
            FarmerLevels = farmerLevels;
        }
    }

    [System.Serializable]
    public class PurchaserData
    {
        public int x;
        public int y;
        public int z;
        public Vector3Int Position => new Vector3Int(x, y, z);
        public int Price;

        public PurchaserData(Vector3Int position, int price)
        {
            x = position.x;
            y = position.y;
            z = position.z;
            Price = price;
        }
    }

    [System.Serializable]
    public class QuestProgressData
    {
        public string questId;
        public int currentAmount;
        public bool isCompleted;
        public bool rewardClaimed;
    }
}
