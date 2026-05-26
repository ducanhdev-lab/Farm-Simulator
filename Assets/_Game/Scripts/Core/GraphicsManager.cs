using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace IslandHarvest.Game
{
    public class GraphicsManager : MonoBehaviour
    {
        public static GraphicsManager Instance { get; private set; }

        private const float TargetAspect = 9f / 16f;
        private const float DefaultRenderScale = 0.75f;

        [Header("Pipeline Settings")]
        [SerializeField, Tooltip("Reference to the URP Asset to control render scale.")]
        private UniversalRenderPipelineAsset _URPAsset;

        public bool IsShadowsOn { get; private set; }

        private int lastScreenWidth;
        private int lastScreenHeight;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializeGraphics();
        }

        private void Update()
        {
            if (Screen.width == lastScreenWidth && Screen.height == lastScreenHeight)
                return;

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            ApplyAspectRatio();
        }

        private void InitializeGraphics()
        {
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

            LockPortraitOrientation();

            IsShadowsOn = PlayerPrefs.GetInt("Shadow ON", 1) > 0;
            ApplyShadows(IsShadowsOn);

            ApplyRenderScale(DefaultRenderScale);

            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;
            ApplyAspectRatio();
        }

        private static void LockPortraitOrientation()
        {
#if UNITY_ANDROID || UNITY_IOS
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
#endif
        }

        public void SetShadows(bool isOn)
        {
            IsShadowsOn = isOn;
            PlayerPrefs.SetInt("Shadow ON", isOn ? 1 : 0);
            ApplyShadows(isOn);
        }

        private void ApplyShadows(bool isOn)
        {
            Light sun = RenderSettings.sun;
            if (sun != null)
                sun.shadows = isOn ? LightShadows.Hard : LightShadows.None;
        }

        private void ApplyRenderScale(float scale)
        {
            if (_URPAsset != null)
                _URPAsset.renderScale = scale;
        }

        private void ApplyAspectRatio()
        {
            var cam = Camera.main;
            if (cam == null)
                return;

            float windowAspect = (float)Screen.width / Screen.height;

            if (windowAspect > TargetAspect)
            {
                float scaleWidth = TargetAspect / windowAspect;
                cam.rect = new Rect((1f - scaleWidth) * 0.5f, 0f, scaleWidth, 1f);
            }
            else
            {
                float scaleHeight = windowAspect / TargetAspect;
                cam.rect = new Rect(0f, (1f - scaleHeight) * 0.5f, 1f, scaleHeight);
            }
        }
    }
}
