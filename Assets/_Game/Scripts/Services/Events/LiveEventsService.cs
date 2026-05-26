using System;
using System.Collections;
using UnityEngine;

namespace IslandHarvest.Game
{
    public static class LiveEventsService
    {
        public static bool HasRefreshed { get; private set; }

        public static IEnumerator RefreshFromServer()
        {
            var client = CloudSaveService.Instance?.ApiClient;
            if (client == null)
                yield break;

            bool done = false;
            LiveEventData[] events = null;

            yield return client.Get("/events/active", apiResult =>
            {
                if (apiResult.Success && !string.IsNullOrEmpty(apiResult.Body))
                {
                    var response = JsonUtility.FromJson<LiveEventsApiResponse>(apiResult.Body);
                    events = response?.events;
                }

                done = true;
            });

            while (!done)
                yield return null;

            if (events != null && events.Length > 0)
            {
                EventWorldRegistry.ApplyLiveEvents(events);
                HasRefreshed = true;
                Debug.Log($"Live events loaded: {events.Length}");
            }
        }
    }
}
