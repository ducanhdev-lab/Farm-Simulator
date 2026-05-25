using System.IO;
using UnityEditor;
using UnityEngine;

namespace IslandHarvest.Game
{
    public static class QuestAssetGenerator
    {
        [MenuItem("Tools/Island Harvest/Create Default Quest Assets")]
        public static void CreateDefaultQuests()
        {
            const string folder = "Assets/_Game/Resources/Quests";
            if (!AssetDatabase.IsValidFolder(folder))
            {
                if (!AssetDatabase.IsValidFolder("Assets/_Game/Resources"))
                    AssetDatabase.CreateFolder("Assets/_Game", "Resources");
                AssetDatabase.CreateFolder("Assets/_Game/Resources", "Quests");
            }

            CreateQuest(folder, "Quest_Harvest", "Green Thumb", QuestType.HarvestCrops, 12, 180);
            CreateQuest(folder, "Quest_Sell", "Market Day", QuestType.SellAtStall, 250, 220);
            CreateQuest(folder, "Quest_Unlock", "New Horizons", QuestType.UnlockIslands, 2, 300);
            CreateQuest(folder, "Quest_Earn", "Treasure Hunt", QuestType.EarnCoins, 600, 350);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("Default quest assets created under Assets/_Game/Resources/Quests");
        }

        private static void CreateQuest(string folder, string fileName, string title, QuestType type, int target, int reward)
        {
            string path = $"{folder}/{fileName}.asset";
            if (File.Exists(path)) return;

            var quest = ScriptableObject.CreateInstance<QuestData>();
            quest.SetRuntimeDefaults(fileName.ToLower(), title, $"{title}: reach {target}", type, target, reward);
            AssetDatabase.CreateAsset(quest, path);
        }
    }
}
