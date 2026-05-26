using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IslandHarvest.Game
{
    public class LeaderboardWindow : WindowBase
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private Button refreshButton;
        [SerializeField] private Transform listContainer;
        [SerializeField] private TMP_Text rowPrefab;
        [SerializeField] private TMP_Text statusLabel;
        [SerializeField] private IslandVisitWindow visitWindow;
        [SerializeField] private int entryLimit = 20;

        private readonly List<TMP_Text> rows = new List<TMP_Text>();
        private LeaderboardEntryData[] cachedEntries = System.Array.Empty<LeaderboardEntryData>();

        protected override void Awake()
        {
            base.Awake();

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (refreshButton != null)
                refreshButton.onClick.AddListener(() => StartCoroutine(LoadLeaderboard()));

            Hide();
        }

        public override void Show()
        {
            base.Show();
            StartCoroutine(LoadLeaderboard());
        }

        private IEnumerator LoadLeaderboard()
        {
            SetStatus("Loading...");

            yield return LeaderboardService.FetchTopCoins(entryLimit, entries =>
            {
                cachedEntries = entries ?? System.Array.Empty<LeaderboardEntryData>();
                RenderEntries(cachedEntries);
                SetStatus(cachedEntries.Length == 0 ? "No scores yet." : null);
            });
        }

        private void RenderEntries(LeaderboardEntryData[] entries)
        {
            if (rowPrefab == null || listContainer == null)
                return;

            while (rows.Count > entries.Length)
            {
                var row = rows[rows.Count - 1];
                rows.Remove(row);
                Destroy(row.gameObject);
            }

            while (rows.Count < entries.Length)
            {
                var row = Instantiate(rowPrefab, listContainer);
                row.gameObject.SetActive(true);
                rows.Add(row);
            }

            string localPlayerId = CloudSaveService.Instance?.PlayerId;

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                string label = FormatEntryLabel(entry);
                bool isLocal = !string.IsNullOrEmpty(localPlayerId) && entry.playerId == localPlayerId;
                rows[i].text = $"#{entry.rank}  {label}  —  {entry.coins:N0}{(isLocal ? " ★" : "")}";
                rows[i].color = isLocal ? new Color(0.55f, 0.9f, 1f) : Color.white;

                var button = rows[i].GetComponent<Button>();
                if (button == null)
                    button = rows[i].gameObject.AddComponent<Button>();

                string playerId = entry.playerId;
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnEntryClicked(playerId));
            }
        }

        private void OnEntryClicked(string playerId)
        {
            if (visitWindow == null || string.IsNullOrEmpty(playerId))
                return;

            visitWindow.ShowForPlayer(playerId);
        }

        private static string FormatEntryLabel(LeaderboardEntryData entry)
        {
            if (!string.IsNullOrEmpty(entry.displayName))
                return entry.displayName;

            return ShortPlayerId(entry.playerId);
        }

        private static string ShortPlayerId(string playerId)
        {
            if (string.IsNullOrEmpty(playerId))
                return "???";

            return playerId.Length <= 8 ? playerId : playerId.Substring(0, 8);
        }

        private void SetStatus(string message)
        {
            if (statusLabel != null)
            {
                statusLabel.gameObject.SetActive(!string.IsNullOrEmpty(message));
                statusLabel.text = message ?? string.Empty;
            }
        }
    }
}
