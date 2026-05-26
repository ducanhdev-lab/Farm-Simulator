using System;
using System.Collections;
using UnityEngine;

namespace IslandHarvest.Game
{
    public static class IslandSnapshotService
    {
        public static IslandSnapshotData CaptureFromProfile(PlayerProfile profile)
        {
            if (profile?.homeWorld == null)
                return null;

            var home = profile.homeWorld;
            int unlocked = 0;
            if (home.UnlockedIslands != null)
            {
                foreach (bool flag in home.UnlockedIslands)
                {
                    if (flag)
                        unlocked++;
                }
            }

            string ownerId = CloudSaveService.Instance?.PlayerId;
            if (string.IsNullOrEmpty(ownerId))
                ownerId = profile.profileId;

            return new IslandSnapshotData
            {
                ownerPlayerId = ownerId,
                displayName = profile.displayName,
                coins = home.Coin,
                unlockedIslandCount = unlocked,
                equippedSkinId = home.equippedSkinId,
                capturedUtc = DateTime.UtcNow.ToString("o")
            };
        }

        public static IEnumerator UploadSnapshot(PlayerProfile profile)
        {
            var client = CloudSaveService.Instance?.ApiClient;
            if (client == null || string.IsNullOrEmpty(client.AuthToken))
                yield break;

            var snapshot = CaptureFromProfile(profile);
            if (snapshot == null)
                yield break;

            string body = JsonUtility.ToJson(new IslandSnapshotPayload
            {
                snapshotJson = JsonUtility.ToJson(snapshot)
            });

            bool done = false;
            yield return client.PutJson("/island/snapshot", body, _ => done = true);

            while (!done)
                yield return null;
        }

        public static IEnumerator FetchSnapshot(string playerId, Action<IslandSnapshotData> onComplete)
        {
            var client = CloudSaveService.Instance?.ApiClient;
            if (client == null || string.IsNullOrEmpty(playerId))
            {
                onComplete?.Invoke(null);
                yield break;
            }

            bool done = false;
            IslandSnapshotData result = null;

            yield return client.Get($"/island/snapshot/{playerId}", apiResult =>
            {
                if (apiResult.Success && !string.IsNullOrEmpty(apiResult.Body))
                {
                    var response = JsonUtility.FromJson<IslandSnapshotApiResponse>(apiResult.Body);
                    result = response?.snapshot;
                }

                done = true;
            });

            while (!done)
                yield return null;

            onComplete?.Invoke(result);
        }
    }
}
