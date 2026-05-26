using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace IslandHarvest.Game
{
    /// <summary>
    /// Composition root for runtime services. Place on IslandManager prefab.
    /// </summary>
    public class GameServices : MonoBehaviour
    {
        public static GameServices Instance { get; private set; }

        public IPlayerProfileService Profile { get; private set; }
        public IWalletService Wallet { get; private set; }
        public ISaveCoordinator Save { get; private set; }
        public IEconomyService Economy { get; private set; }
        public IWorldExpansionService World { get; private set; }
        public IRemoteConfigService RemoteConfig { get; private set; }
        public IAnalyticsService Analytics { get; private set; }

        [SerializeField] private int startingMoney = 500;
        [SerializeField] private int baseUnlockPrice = 100;
        [SerializeField] private float unlockGrowthFactor = 1.1f;
        [SerializeField] private bool fetchRemoteConfig = true;

        private WalletService walletImpl;
        private ApiRemoteConfigService apiRemoteConfig;
        private LocalRemoteConfigService localRemoteConfig;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
            Bootstrap();
        }

        public void BootstrapForScene(List<bool> defaultUnlockedIslands)
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            Profile.InitializeForScene(sceneName, startingMoney, defaultUnlockedIslands);
            walletImpl.Bind(Profile.ActiveWorldData);

            EnsureBiomeManager();
        }

        private void Bootstrap()
        {
            PlayerProfileService.EnsureInitialized();
            Profile = PlayerProfileService.Instance ?? new PlayerProfileService();

            walletImpl = new WalletService();
            Wallet = walletImpl;

            Save = new SaveCoordinator(Profile);
            localRemoteConfig = new LocalRemoteConfigService();
            apiRemoteConfig = new ApiRemoteConfigService(localRemoteConfig, baseUnlockPrice);
            RemoteConfig = apiRemoteConfig;
            World = new WorldExpansionService(baseUnlockPrice, unlockGrowthFactor);
            Economy = new EconomyService(Wallet, RemoteConfig);
            Analytics = new DebugAnalyticsService();
            AnalyticsEventBridge.Bind(Analytics);

            CloudSaveService.EnsureInstance();

            if (fetchRemoteConfig)
                StartCoroutine(InitializeRemoteConfig());
        }

        private IEnumerator InitializeRemoteConfig()
        {
            while (CloudSaveService.Instance == null)
                yield return null;

            yield return CloudSaveService.Instance.StartCoroutine(CloudSaveService.Instance.AuthenticateGuest());

            var client = CloudSaveService.Instance.ApiClient;
            if (client == null)
                yield break;

            yield return apiRemoteConfig.FetchFromApi(client);

            World = new WorldExpansionService(RemoteConfig.GetIslandBasePrice(baseUnlockPrice), unlockGrowthFactor);
            Economy = new EconomyService(Wallet, RemoteConfig);
        }

        private static void EnsureBiomeManager()
        {
            if (Object.FindFirstObjectByType<BiomeManager>() != null)
                return;

            var islandManager = IslandManager.Instance;
            if (islandManager == null)
                return;

            islandManager.gameObject.AddComponent<BiomeManager>();
        }

        private void OnDestroy() => AnalyticsEventBridge.Unbind();

        private void OnApplicationPause(bool pause)
        {
            if (pause)
                Save?.SaveNow();
        }

        private void OnApplicationQuit() => Save?.SaveNow();
    }
}
