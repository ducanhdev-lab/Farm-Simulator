using System;
using System.Collections;
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
    }
}
