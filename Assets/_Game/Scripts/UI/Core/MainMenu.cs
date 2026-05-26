using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;

namespace IslandHarvest.Game
{
    public class MainMenu : MonoBehaviour
    {
        [Tooltip("The Start button that initiates the game.")]
        [SerializeField] private Button startButton;

        [Tooltip("The image used for screen fading effects.")]
        [SerializeField] private Image screenFader;

        [Tooltip("Audio clip to play when the title screen is displayed upon first opening the game.")]
        [SerializeField] private AudioClip titleSong;

        [Header("Shop")]
        [SerializeField] private Button shopButton;
        [SerializeField] private GameShopWindow shopWindow;

        private void Awake()
        {
            // Hide the start button until the fade effect is complete
            startButton.gameObject.SetActive(false);

            // Ensure the screen fader is visible at the start
            screenFader.gameObject.SetActive(true);
        }

        private void Start()
        {
            // Fade out the screen and then hide the fader while showing the start button
            screenFader.DOFade(0f, 1f)
                .OnComplete(() =>
                {
                    screenFader.gameObject.SetActive(false); // Disable the screen fader
                    startButton.gameObject.SetActive(true); // Show the start button
                    InitializeStartButton(); // Set up the start button behavior
                });

            // Play the title screen background music when the game is first opened.
            AudioManager.Instance.PlayBGM(titleSong);

            IAPService.EnsureInstance();
            InitializeShopButton();
        }

        private void InitializeShopButton()
        {
            if (shopButton == null)
                shopButton = GameObject.Find("ShopButton")?.GetComponent<Button>();

            if (shopWindow == null)
                shopWindow = FindFirstObjectByType<GameShopWindow>(FindObjectsInactive.Include);

            if (shopWindow != null)
                shopWindow.Hide();

            if (shopButton == null || shopWindow == null)
                return;

            shopButton.onClick.RemoveAllListeners();
            shopButton.onClick.AddListener(() =>
            {
                shopWindow.ShowForMainMenu();
                AudioManager.Instance.PlaySFX(AudioID.UI_Accept);
            });
        }

        private void InitializeStartButton()
        {
            string sceneToLoad = ResolveStartupSceneName();

            // Add a listener to start loading the scene when the button is clicked
            startButton.onClick.AddListener(() =>
            {
                StartCoroutine(LoadSceneAsync(sceneToLoad));
                AudioManager.Instance.PlaySFX(AudioID.UI_Accept); // Play 'UI Accept' sound
                DOTween.Kill(startButton.transform); // Stop any ongoing tweening on the button (if any)
            });

            // Apply a pulsing animation to the start button for visual effect
            startButton.transform.DOScale(Vector3.one * 1.2f, 0.5f).SetLoops(-1, LoopType.Yoyo);
        }

        private static string ResolveStartupSceneName()
        {
            return WorldSceneIds.HomeIsland;
        }

        private static string GetDefaultLevelSceneName()
        {
            const string fallbackScene = WorldSceneIds.HomeIsland;
            string defaultScenePath = SceneUtility.GetScenePathByBuildIndex(1);

            if (string.IsNullOrEmpty(defaultScenePath))
            {
                Debug.LogWarning($"Build index 1 is missing; falling back to '{fallbackScene}'.");
                return fallbackScene;
            }

            return Path.GetFileNameWithoutExtension(defaultScenePath);
        }

        private IEnumerator LoadSceneAsync(string sceneName)
        {
            // Enable the screen fader and start fading in
            screenFader.gameObject.SetActive(true);
            yield return screenFader.DOFade(1f, 1f).WaitForCompletion();

            // Start loading the scene asynchronously but prevent automatic activation
            AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(sceneName);
            if (asyncOperation == null)
            {
                string fallbackScene = GetDefaultLevelSceneName();
                Debug.LogError($"Scene '{sceneName}' is not in Build Settings. Loading '{fallbackScene}' instead.");
                asyncOperation = SceneManager.LoadSceneAsync(fallbackScene);

                if (asyncOperation == null)
                    yield break;
            }

            asyncOperation.allowSceneActivation = false;

            // Wait until the scene loading is complete (but not yet activated)
            while (!asyncOperation.isDone)
            {
                // Once loading reaches 90% (0.9f), consider it ready for activation
                if (asyncOperation.progress >= 0.9f)
                {
                    yield return new WaitForSeconds(1f); // Optional delay before activating the scene
                    asyncOperation.allowSceneActivation = true; // Activate the scene
                }

                yield return null; // Continue waiting in the loop
            }
        }
    }
}
