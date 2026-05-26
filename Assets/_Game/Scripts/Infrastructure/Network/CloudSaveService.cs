using System;
using System.Collections;
using UnityEngine;

namespace IslandHarvest.Game
{
    [Serializable]
    public class GuestAuthResponse
    {
        public string token;
        public string playerId;
    }

    [Serializable]
    public class CloudSavePayload
    {
        public int saveVersion;
        public string profileJson;
    }

    public class CloudSaveService : MonoBehaviour
    {
        public static CloudSaveService Instance { get; private set; }

        [SerializeField] private string apiBaseUrl = "http://localhost:3000";
        [SerializeField] private bool syncOnSave = true;
        [SerializeField] private bool authenticateOnStart = true;

        private ApiClient apiClient;
        private string playerId;
        private bool isSyncing;

        public static void EnsureInstance()
        {
            if (Instance != null)
                return;

            var go = new GameObject(nameof(CloudSaveService));
            go.AddComponent<CloudSaveService>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            apiClient = new ApiClient { BaseUrl = apiBaseUrl };

            if (authenticateOnStart)
                StartCoroutine(AuthenticateGuest());

            StartCoroutine(SubscribeToProfileSaves());
        }

        private IEnumerator SubscribeToProfileSaves()
        {
            while (PlayerProfileService.Instance == null)
                yield return null;

            PlayerProfileService.Instance.OnProfileSaved += HandleProfileSaved;
        }

        private void OnDestroy()
        {
            if (PlayerProfileService.Instance != null)
                PlayerProfileService.Instance.OnProfileSaved -= HandleProfileSaved;
        }

        private void HandleProfileSaved(PlayerProfile profile)
        {
            if (syncOnSave && !isSyncing)
                StartCoroutine(SyncProfileToCloud(profile));
        }

        public IEnumerator AuthenticateGuest()
        {
            bool done = false;
            string error = null;

            yield return apiClient.PostJson("/auth/guest", "{}", json =>
            {
                var response = JsonUtility.FromJson<GuestAuthResponse>(json);
                if (response != null)
                {
                    apiClient.AuthToken = response.token;
                    playerId = response.playerId;
                }

                done = true;
            }, err =>
            {
                error = err;
                done = true;
            });

            while (!done)
                yield return null;

            if (!string.IsNullOrEmpty(error))
                Debug.LogWarning($"Guest auth failed (offline mode): {error}");
        }

        public IEnumerator SyncProfileToCloud(PlayerProfile profile)
        {
            if (profile == null || string.IsNullOrEmpty(apiClient.AuthToken))
                yield break;

            isSyncing = true;
            string json = JsonUtility.ToJson(profile);
            var payload = new CloudSavePayload
            {
                saveVersion = profile.saveVersion,
                profileJson = json
            };
            string body = JsonUtility.ToJson(payload);

            bool done = false;
            yield return apiClient.PutJson("/player/save", body, _ => done = true, err =>
            {
                Debug.LogWarning($"Cloud save failed: {err}");
                done = true;
            });

            while (!done)
                yield return null;

            isSyncing = false;
        }

        public IEnumerator FetchProfile(Action<PlayerProfile> onComplete)
        {
            if (string.IsNullOrEmpty(apiClient.AuthToken))
            {
                onComplete?.Invoke(null);
                yield break;
            }

            bool done = false;
            PlayerProfile result = null;

            yield return apiClient.Get("/player/profile", json =>
            {
                if (!string.IsNullOrEmpty(json))
                    result = JsonUtility.FromJson<PlayerProfile>(json);
                done = true;
            }, _ => done = true);

            while (!done)
                yield return null;

            onComplete?.Invoke(result);
        }
    }
}
