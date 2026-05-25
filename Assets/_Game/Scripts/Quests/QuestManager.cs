using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace IslandHarvest.Game
{
    public class QuestManager : MonoBehaviour
    {
        public static QuestManager Instance { get; private set; }

        public event Action OnQuestsUpdated;

        private const int DailyQuestCount = 3;
        private readonly List<QuestData> questPool = new List<QuestData>();
        private readonly List<ActiveQuest> activeQuests = new List<ActiveQuest>();

        private GameData gameData;
        private int lastCoinSnapshot;

        private class ActiveQuest
        {
            public QuestData Data;
            public QuestProgressData Progress;
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            LoadQuestPool();
        }

        private void OnDestroy()
        {
            UnsubscribeEvents();
        }

        public void Initialize(GameData data)
        {
            UnsubscribeEvents();
            gameData = data;
            lastCoinSnapshot = data.Coin;
            EnsureQuestFields();
            RefreshDailyQuestsIfNeeded();
            SubscribeEvents();
            OnQuestsUpdated?.Invoke();
        }

        private void SubscribeEvents()
        {
            GameplayEvents.OnCropHarvested += HandleCropHarvested;
            GameplayEvents.OnCoinsEarnedAtStall += HandleCoinsEarned;
            GameplayEvents.OnIslandUnlocked += HandleIslandUnlocked;

            if (IslandManager.Instance != null)
                IslandManager.Instance.OnCoinChanged += HandleCoinChanged;
        }

        private void UnsubscribeEvents()
        {
            GameplayEvents.OnCropHarvested -= HandleCropHarvested;
            GameplayEvents.OnCoinsEarnedAtStall -= HandleCoinsEarned;
            GameplayEvents.OnIslandUnlocked -= HandleIslandUnlocked;

            if (IslandManager.Instance != null)
                IslandManager.Instance.OnCoinChanged -= HandleCoinChanged;
        }

        public IReadOnlyList<(QuestData data, QuestProgressData progress)> GetActiveQuests()
        {
            return activeQuests.Select(q => (q.Data, q.Progress)).ToList();
        }

        public void ClaimReward(string questId)
        {
            var quest = activeQuests.FirstOrDefault(q => q.Data.QuestId == questId);
            if (quest == null || !quest.Progress.isCompleted || quest.Progress.rewardClaimed)
                return;

            quest.Progress.rewardClaimed = true;
            IslandManager.Instance.Coin += quest.Data.CoinReward;
            AudioManager.Instance.PlaySFX(AudioID.Cash);
            OnQuestsUpdated?.Invoke();
        }

        private void LoadQuestPool()
        {
            questPool.Clear();
            var assets = Resources.LoadAll<QuestData>("Quests");
            if (assets.Length > 0)
            {
                questPool.AddRange(assets);
                return;
            }

            // Runtime fallback when ScriptableObject assets are not yet created in Resources.
            CreateRuntimeFallbackQuests();
        }

        private void CreateRuntimeFallbackQuests()
        {
            questPool.Add(CreateRuntimeQuest("harvest_10", "Harvest Crops", QuestType.HarvestCrops, 10, 150));
            questPool.Add(CreateRuntimeQuest("sell_200", "Market Seller", QuestType.SellAtStall, 200, 200));
            questPool.Add(CreateRuntimeQuest("unlock_2", "Expand the Farm", QuestType.UnlockIslands, 2, 250));
            questPool.Add(CreateRuntimeQuest("earn_500", "Coin Collector", QuestType.EarnCoins, 500, 300));
        }

        private static QuestData CreateRuntimeQuest(string id, string title, QuestType type, int target, int reward)
        {
            var quest = ScriptableObject.CreateInstance<QuestData>();
            quest.SetRuntimeDefaults(id, title, $"{title} — reach {target}", type, target, reward);
            return quest;
        }

        private void EnsureQuestFields()
        {
            if (gameData.QuestProgress == null)
                gameData.QuestProgress = new List<QuestProgressData>();

            if (string.IsNullOrEmpty(gameData.lastQuestResetDate))
                gameData.lastQuestResetDate = string.Empty;
        }

        private void RefreshDailyQuestsIfNeeded()
        {
            string today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            if (gameData.lastQuestResetDate == today && gameData.QuestProgress.Count > 0)
            {
                RebuildActiveQuestsFromSave();
                return;
            }

            gameData.lastQuestResetDate = today;
            gameData.QuestProgress.Clear();
            activeQuests.Clear();

            var picks = questPool.OrderBy(_ => UnityEngine.Random.value).Take(DailyQuestCount).ToList();
            foreach (var quest in picks)
            {
                gameData.QuestProgress.Add(new QuestProgressData
                {
                    questId = quest.QuestId,
                    currentAmount = 0,
                    isCompleted = false,
                    rewardClaimed = false
                });
            }

            RebuildActiveQuestsFromSave();
        }

        private void RebuildActiveQuestsFromSave()
        {
            activeQuests.Clear();
            foreach (var progress in gameData.QuestProgress)
            {
                var data = questPool.FirstOrDefault(q => q.QuestId == progress.questId);
                if (data == null) continue;
                activeQuests.Add(new ActiveQuest { Data = data, Progress = progress });
            }
        }

        private void HandleCropHarvested(int amount) => AddProgress(QuestType.HarvestCrops, amount);
        private void HandleCoinsEarned(int amount) => AddProgress(QuestType.SellAtStall, amount);
        private void HandleIslandUnlocked() => AddProgress(QuestType.UnlockIslands, 1);

        private void HandleCoinChanged(int totalCoins)
        {
            int delta = totalCoins - lastCoinSnapshot;
            lastCoinSnapshot = totalCoins;
            if (delta > 0)
                AddProgress(QuestType.EarnCoins, delta);
        }

        private void AddProgress(QuestType type, int amount)
        {
            if (amount <= 0) return;

            bool changed = false;
            foreach (var quest in activeQuests.Where(q => q.Data.QuestType == type && !q.Progress.isCompleted))
            {
                quest.Progress.currentAmount += amount;
                changed |= TryComplete(quest);
            }

            if (changed)
                OnQuestsUpdated?.Invoke();
        }

        private bool TryComplete(ActiveQuest quest)
        {
            if (quest.Progress.isCompleted) return false;

            int target = quest.Data.TargetAmount;
            if (quest.Data.QuestType == QuestType.EarnCoins)
                quest.Progress.currentAmount = Mathf.Min(quest.Progress.currentAmount, target);

            if (quest.Progress.currentAmount < target) return false;

            quest.Progress.currentAmount = target;
            quest.Progress.isCompleted = true;
            AudioManager.Instance.PlaySFX(AudioID.UI_Accept);
            return true;
        }
    }
}
