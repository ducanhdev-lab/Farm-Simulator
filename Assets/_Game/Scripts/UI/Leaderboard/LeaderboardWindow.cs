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
        [SerializeField] private float rowHeight = 40f;

        private readonly List<TMP_Text> rows = new List<TMP_Text>();
        private LeaderboardEntryData[] cachedEntries = System.Array.Empty<LeaderboardEntryData>();

        protected override void Awake()
        {
            base.Awake();

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            if (refreshButton != null)
                refreshButton.onClick.AddListener(() => StartCoroutine(LoadLeaderboard()));

            EnsureListLayout();
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
                cachedEntries = LeaderboardService.MergeWithLocalPlayer(
                    entries ?? System.Array.Empty<LeaderboardEntryData>(),
                    entryLimit,
                    out bool addedLocalOnly);

                RenderEntries(cachedEntries);

                if (cachedEntries.Length == 0)
                    SetStatus("No scores yet.");
                else if (addedLocalOnly)
                    SetStatus("Your score is from this device — syncs to cloud after save.");
                else
                    SetStatus(null);
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
                ConfigureRowLayout(row);
                rows.Add(row);
            }

            string localPlayerId = CloudSaveService.Instance?.PlayerId;

            for (int i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                ConfigureRowLayout(rows[i]);
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

            if (listContainer is RectTransform listRect)
                LayoutRebuilder.ForceRebuildLayoutImmediate(listRect);
        }

        private void EnsureListLayout()
        {
            if (listContainer == null)
                return;

            var layout = listContainer.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = listContainer.gameObject.AddComponent<VerticalLayoutGroup>();
                layout.spacing = 4f;
                layout.padding = new RectOffset(0, 0, 4, 4);
                layout.childAlignment = TextAnchor.UpperLeft;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
            }

            var fitter = listContainer.GetComponent<ContentSizeFitter>();
            if (fitter == null)
            {
                fitter = listContainer.gameObject.AddComponent<ContentSizeFitter>();
                fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
                fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }

            if (rowPrefab != null)
                ConfigureRowLayout(rowPrefab);
        }

        private void ConfigureRowLayout(TMP_Text row)
        {
            if (row == null)
                return;

            var rt = row.rectTransform;
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0.5f, 1f);
            rt.sizeDelta = new Vector2(0f, rowHeight);
            rt.anchoredPosition = Vector2.zero;

            var layoutElement = row.GetComponent<LayoutElement>();
            if (layoutElement == null)
                layoutElement = row.gameObject.AddComponent<LayoutElement>();

            layoutElement.minHeight = rowHeight;
            layoutElement.preferredHeight = rowHeight;
            layoutElement.flexibleWidth = 1f;

            row.enableWordWrapping = false;
            row.overflowMode = TextOverflowModes.Ellipsis;
            row.alignment = TextAlignmentOptions.MidlineLeft;
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
