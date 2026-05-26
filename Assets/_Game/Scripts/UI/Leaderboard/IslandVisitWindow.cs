using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace IslandHarvest.Game
{
    public class IslandVisitWindow : WindowBase
    {
        [SerializeField] private Button closeButton;
        [SerializeField] private TMP_Text titleLabel;
        [SerializeField] private TMP_Text detailsLabel;
        [SerializeField] private TMP_Text statusLabel;

        protected override void Awake()
        {
            base.Awake();

            if (closeButton != null)
                closeButton.onClick.AddListener(Hide);

            Hide();
        }

        public void ShowForPlayer(string playerId)
        {
            Show();
            StartCoroutine(LoadSnapshot(playerId));
        }

        private IEnumerator LoadSnapshot(string playerId)
        {
            SetStatus("Loading island...");
            if (detailsLabel != null)
                detailsLabel.text = string.Empty;

            if (titleLabel != null)
                titleLabel.text = $"Island — {playerId.Substring(0, Mathf.Min(8, playerId.Length))}";

            IslandSnapshotData snapshot = null;
            yield return IslandSnapshotService.FetchSnapshot(playerId, s => snapshot = s);

            if (snapshot == null)
            {
                SetStatus("No snapshot available.");
                yield break;
            }

            SetStatus(null);
            if (titleLabel != null && !string.IsNullOrEmpty(snapshot.displayName))
                titleLabel.text = snapshot.displayName;

            if (detailsLabel != null)
            {
                detailsLabel.text =
                    $"Coins: {snapshot.coins:N0}\n" +
                    $"Unlocked islands: {snapshot.unlockedIslandCount}\n" +
                    $"Skin: {snapshot.equippedSkinId}\n" +
                    $"Captured: {snapshot.capturedUtc}";
            }
        }

        private void SetStatus(string message)
        {
            if (statusLabel == null)
                return;

            statusLabel.gameObject.SetActive(!string.IsNullOrEmpty(message));
            statusLabel.text = message ?? string.Empty;
        }
    }
}
