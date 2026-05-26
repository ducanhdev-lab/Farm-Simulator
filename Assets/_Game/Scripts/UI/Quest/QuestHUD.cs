using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IslandHarvest.Game
{
    /// <summary>
    /// Runtime daily-quest panel (no prefab wiring required for portfolio build).
    /// </summary>
    public class QuestHUD : MonoBehaviour
    {
        [SerializeField] private Vector2 panelAnchor = new Vector2(1f, 1f);
        [SerializeField] private Vector2 panelPivot = new Vector2(1f, 1f);
        [SerializeField] private Vector2 panelPosition = new Vector2(-24f, -120f);
        [SerializeField] private Vector2 panelSize = new Vector2(340f, 200f);

        private RectTransform panelRoot;
        private readonly List<QuestRowView> rows = new List<QuestRowView>();

        private class QuestRowView
        {
            public string QuestId;
            public TMP_Text Label;
            public Button ClaimButton;
        }

        private void Start()
        {
            BuildPanel();
            if (QuestManager.Instance != null)
            {
                QuestManager.Instance.OnQuestsUpdated += Refresh;
                Refresh();
            }
        }

        private void OnDestroy()
        {
            if (QuestManager.Instance != null)
                QuestManager.Instance.OnQuestsUpdated -= Refresh;
        }

        private void BuildPanel()
        {
            Canvas canvas = null;
            if (UIManager.Instance != null)
                canvas = UIManager.Instance.GetComponentInChildren<Canvas>(true);
            if (canvas == null)
                canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null) return;

            var panelGo = new GameObject("DailyQuestPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelGo.transform.SetParent(canvas.transform, false);

            panelRoot = panelGo.GetComponent<RectTransform>();
            panelRoot.anchorMin = panelAnchor;
            panelRoot.anchorMax = panelAnchor;
            panelRoot.pivot = panelPivot;
            panelRoot.anchoredPosition = panelPosition;
            panelRoot.sizeDelta = panelSize;

            var bg = panelGo.GetComponent<Image>();
            bg.color = new Color(0.08f, 0.12f, 0.18f, 0.88f);

            var titleGo = CreateText(panelGo.transform, "Daily Quests", 22, FontStyles.Bold);
            var titleRect = titleGo.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.anchoredPosition = new Vector2(0f, -8f);
            titleRect.sizeDelta = new Vector2(-16f, 32f);

            float y = -44f;
            for (int i = 0; i < 3; i++)
            {
                rows.Add(CreateQuestRow(panelGo.transform, ref y));
            }
        }

        private QuestRowView CreateQuestRow(Transform parent, ref float y)
        {
            var rowGo = new GameObject("QuestRow", typeof(RectTransform));
            rowGo.transform.SetParent(parent, false);

            var rowRect = rowGo.GetComponent<RectTransform>();
            rowRect.anchorMin = new Vector2(0f, 1f);
            rowRect.anchorMax = new Vector2(1f, 1f);
            rowRect.pivot = new Vector2(0.5f, 1f);
            rowRect.anchoredPosition = new Vector2(0f, y);
            rowRect.sizeDelta = new Vector2(-16f, 44f);
            y -= 48f;

            var labelGo = CreateText(rowGo.transform, "—", 16, FontStyles.Normal);
            var labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = new Vector2(0.72f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            var buttonGo = new GameObject("Claim", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonGo.transform.SetParent(rowGo.transform, false);
            var buttonRect = buttonGo.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.74f, 0.1f);
            buttonRect.anchorMax = new Vector2(1f, 0.9f);
            buttonRect.offsetMin = Vector2.zero;
            buttonRect.offsetMax = Vector2.zero;

            var buttonImage = buttonGo.GetComponent<Image>();
            buttonImage.color = new Color(0.2f, 0.55f, 0.35f, 1f);

            var button = buttonGo.GetComponent<Button>();
            var buttonLabel = CreateText(buttonGo.transform, "Claim", 14, FontStyles.Bold);
            StretchFull(buttonLabel.GetComponent<RectTransform>());

            return new QuestRowView
            {
                Label = labelGo.GetComponent<TMP_Text>(),
                ClaimButton = button
            };
        }

        private static GameObject CreateText(Transform parent, string text, float size, FontStyles style)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            var tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.color = Color.white;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            tmp.enableWordWrapping = true;
            return go;
        }

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private void Refresh()
        {
            if (panelRoot != null)
                panelRoot.gameObject.SetActive(QuestManager.Instance.ShouldShowDailyQuests());

            var quests = QuestManager.Instance.GetActiveQuests();
            for (int i = 0; i < rows.Count; i++)
            {
                var row = rows[i];
                if (i >= quests.Count)
                {
                    row.Label.text = string.Empty;
                    row.ClaimButton.gameObject.SetActive(false);
                    continue;
                }

                var (data, progress) = quests[i];
                row.QuestId = data.QuestId;
                int target = data.TargetAmount;
                int current = Mathf.Min(progress.currentAmount, target);

                if (progress.isCompleted && progress.rewardClaimed)
                    row.Label.text = $"{data.DisplayName} — Done";
                else if (progress.isCompleted)
                    row.Label.text = $"{data.DisplayName} — Claim {data.CoinReward} coins";
                else
                    row.Label.text = $"{data.DisplayName} ({current}/{target})";

                row.ClaimButton.gameObject.SetActive(progress.isCompleted && !progress.rewardClaimed);
                row.ClaimButton.onClick.RemoveAllListeners();
                string questId = data.QuestId;
                row.ClaimButton.onClick.AddListener(() => QuestManager.Instance.ClaimReward(questId));
            }
        }
    }
}
