using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    public static class LeaderboardService
    {
        public static IEnumerator FetchTopCoins(int limit, Action<LeaderboardEntryData[]> onComplete)
        {
            var client = CloudSaveService.Instance?.ApiClient;
            if (client == null)
            {
                onComplete?.Invoke(Array.Empty<LeaderboardEntryData>());
                yield break;
            }

            bool done = false;
            LeaderboardEntryData[] result = Array.Empty<LeaderboardEntryData>();

            yield return client.Get($"/leaderboard/coins?limit={limit}", apiResult =>
            {
                if (apiResult.Success && !string.IsNullOrEmpty(apiResult.Body))
                {
                    var response = JsonUtility.FromJson<LeaderboardApiResponse>(apiResult.Body);
                    if (response?.entries != null)
                        result = response.entries;
                }

                done = true;
            });

            while (!done)
                yield return null;

            onComplete?.Invoke(result);
        }

        /// <summary>
        /// Ensures the current player's local coin total appears on the board even if cloud save has not synced yet.
        /// </summary>
        public static LeaderboardEntryData[] MergeWithLocalPlayer(
            LeaderboardEntryData[] serverEntries,
            int limit,
            out bool addedLocalOnly)
        {
            addedLocalOnly = false;
            serverEntries ??= Array.Empty<LeaderboardEntryData>();

            var cloud = CloudSaveService.Instance;
            var profileService = PlayerProfileService.Instance;
            var profile = profileService?.Profile;

            if (cloud == null || profile?.homeWorld == null)
                return serverEntries;

            string playerId = cloud.PlayerId;
            if (string.IsNullOrEmpty(playerId))
                return serverEntries;

            int localCoins = profile.homeWorld.Coin;
            if (localCoins <= 0)
                return serverEntries;

            foreach (var entry in serverEntries)
            {
                if (entry.playerId == playerId)
                    return serverEntries;
            }

            addedLocalOnly = true;
            var list = new List<LeaderboardEntryData>(serverEntries);
            list.Add(new LeaderboardEntryData
            {
                rank = 0,
                playerId = playerId,
                displayName = FormatLocalDisplayName(profile),
                coins = localCoins,
                saveVersion = profile.saveVersion,
                updatedAt = profile.lastSaveUtc
            });

            list.Sort((a, b) => b.coins.CompareTo(a.coins));

            if (list.Count > limit)
                list.RemoveRange(limit, list.Count - limit);

            for (int i = 0; i < list.Count; i++)
                list[i].rank = i + 1;

            return list.ToArray();
        }

        static string FormatLocalDisplayName(PlayerProfile profile)
        {
            if (!string.IsNullOrEmpty(profile.displayName))
                return profile.displayName;

            return "You";
        }
    }
}
