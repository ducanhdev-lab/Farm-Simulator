using UnityEngine;

namespace IslandHarvest.Game
{
    [CreateAssetMenu(fileName = "QuestData", menuName = "Island Harvest/Quest Data")]
    public class QuestData : ScriptableObject
    {
        [SerializeField] private string questId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea(2, 4)] private string description;
        [SerializeField] private QuestType questType;
        [SerializeField] private int targetAmount = 5;
        [SerializeField] private int coinReward = 100;

        public string QuestId => string.IsNullOrEmpty(questId) ? name : questId;
        public string DisplayName => displayName;
        public string Description => description;
        public QuestType QuestType => questType;
        public int TargetAmount => targetAmount;
        public int CoinReward => coinReward;

        /// <summary>Runtime bootstrap when Resources assets are missing.</summary>
        public void SetRuntimeDefaults(string id, string title, string desc, QuestType type, int target, int reward)
        {
            questId = id;
            displayName = title;
            description = desc;
            questType = type;
            targetAmount = target;
            coinReward = reward;
        }
    }
}
