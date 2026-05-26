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
        private const string PrefAuthToken = "farm_life_auth_token";
        private const string PrefPlayerId = "farm_life_player_id";

        public static CloudSaveService Instance { get; private set; }

        [SerializeField] private string apiBaseUrl = "http://localhost:3000";
        [SerializeField] private bool syncOnSave = true;
        [SerializeField] private bool authenticateOnStart = true;
        [SerializeField] private bool mergeOnStartup = true;

        private ApiClient apiClient;
        private bool isSyncing;
        private bool startupMergeDone;

        public ApiClient ApiClient => apiClient;
        public string ApiBaseUrl => apiBaseUrl;
        public string PlayerId => PlayerPrefs.GetString(PrefPlayerId, string.Empty);

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
            RestoreSession();

            if (authenticateOnStart)
                StartCoroutine(AuthenticateGuest());

            StartCoroutine(SubscribeToProfileSaves());
        }

        private void RestoreSession()
        {
            string token = PlayerPrefs.GetString(PrefAuthToken, string.Empty);
            if (!string.IsNullOrEmpty(token))
                apiClient.AuthToken = token;
        }

        private void PersistSession(string token, string playerId)
        {
            if (!string.IsNullOrEmpty(token))
            {
                apiClient.AuthToken = token;
                PlayerPrefs.SetString(PrefAuthToken, token);
            }

            if (!string.IsNullOrEmpty(playerId))
                PlayerPrefs.SetString(PrefPlayerId, playerId);

            PlayerPrefs.Save();
        }

        private IEnumerator SubscribeToProfileSaves()
        {
            while (PlayerProfileService.Instance == null)
                yield return null;

            PlayerProfileService.Instance.OnProfileSaved += HandleProfileSaved;

            if (mergeOnStartup && !startupMergeDone)
                yield return MergeFromCloudOnStartup();

            yield return LiveEventsService.RefreshFromServer();
        }

        private void OnDestroy()
        {
            if (PlayerProfileService.Instance != null)
                PlayerProfileService.Instance.OnProfileSaved -= HandleProfileSaved;
        }

        private void HandleProfileSaved(PlayerProfile profile)
        {
            if (syncOnSave && !isSyncing)
                StartCoroutine(SyncProfileAndSnapshot(profile));
        }

        private IEnumerator SyncProfileAndSnapshot(PlayerProfile profile)
        {
            yield return SyncProfileToCloud(profile);
            yield return IslandSnapshotService.UploadSnapshot(profile);
        }

        public IEnumerator AuthenticateGuest()
        {
            if (!string.IsNullOrEmpty(apiClient.AuthToken))
                yield break;

            bool done = false;
            string error = null;

            yield return apiClient.PostJson("/auth/guest", "{}", result =>
            {
                if (result.Success)
                {
                    var response = JsonUtility.FromJson<GuestAuthResponse>(result.Body);
                    if (response != null)
                        PersistSession(response.token, response.playerId);
                }
                else
                    error = result.Error;

                done = true;
            });

            while (!done)
                yield return null;

            if (!string.IsNullOrEmpty(error))
                Debug.LogWarning($"Guest auth failed (offline mode): {error}");
        }

        public IEnumerator MergeFromCloudOnStartup()
        {
            startupMergeDone = true;

            if (string.IsNullOrEmpty(apiClient.AuthToken))
                yield break;

            var profileService = PlayerProfileService.Instance;
            if (profileService?.Profile == null)
                yield break;

            bool done = false;
            PlayerProfile remote = null;

            yield return FetchProfile(p => { remote = p; done = true; });

            while (!done)
                yield return null;

            if (remote == null || remote.homeWorld == null)
                yield break;

            var local = profileService.Profile;
            if (!ProfileMerger.IsRemoteNewer(local, remote))
            {
                yield return SyncProfileToCloud(local);
                yield break;
            }

            var merged = ProfileMerger.Merge(local, remote);
            profileService.ReplaceProfile(merged);
            profileService.SaveNow();
            IslandManager.Instance?.ReloadFromProfile();

            Debug.Log("Applied newer cloud save on startup.");
            yield return SyncProfileToCloud(merged);
        }

        public IEnumerator SyncProfileToCloud(PlayerProfile profile, bool isRetry = false)
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
            ApiResult result = default;

            yield return apiClient.PutJson("/player/save", body, r => { result = r; done = true; });

            while (!done)
                yield return null;

            if (result.StatusCode == 409 && !isRetry)
            {
                yield return ResolveConflictAndRetry(profile);
            }
            else if (!result.Success)
            {
                Debug.LogWarning($"Cloud save failed ({result.StatusCode}): {result.Error}");
            }

            isSyncing = false;
        }

        private IEnumerator ResolveConflictAndRetry(PlayerProfile localProfile)
        {
            bool done = false;
            PlayerProfile remote = null;

            yield return FetchProfile(p => { remote = p; done = true; });

            while (!done)
                yield return null;

            if (remote == null)
                yield break;

            SaveConflictChoice choice = SaveConflictChoice.Merge;
            yield return SaveConflictPrompt.WaitForChoice(localProfile, remote, c => choice = c);

            var profileService = PlayerProfileService.Instance;
            PlayerProfile resolved = localProfile;

            switch (choice)
            {
                case SaveConflictChoice.UseCloud:
                    resolved = remote;
                    break;

                case SaveConflictChoice.UseLocal:
                    resolved = localProfile;
                    resolved.BumpSaveVersionAbove(remote.saveVersion);
                    break;

                default:
                    resolved = ProfileMerger.Merge(localProfile, remote);
                    break;
            }

            profileService?.ReplaceProfile(resolved);
            profileService?.SaveNow();
            IslandManager.Instance?.ReloadFromProfile();

            Debug.Log($"Cloud save conflict resolved: {choice}.");
            yield return SyncProfileToCloud(resolved, isRetry: true);
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

            yield return apiClient.Get("/player/profile", apiResult =>
            {
                if (apiResult.Success && !string.IsNullOrEmpty(apiResult.Body) && apiResult.Body != "{}")
                    result = JsonUtility.FromJson<PlayerProfile>(apiResult.Body);
                done = true;
            });

            while (!done)
                yield return null;

            onComplete?.Invoke(result);
        }
    }
}
