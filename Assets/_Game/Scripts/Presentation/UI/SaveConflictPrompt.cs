using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace IslandHarvest.Game
{
    public enum SaveConflictChoice
    {
        Merge,
        UseLocal,
        UseCloud
    }

    /// <summary>
    /// Runtime overlay when cloud save returns 409. No prefab / Editor wiring required.
    /// </summary>
    public class SaveConflictPrompt : MonoBehaviour
    {
        static SaveConflictPrompt instance;

        SaveConflictChoice? pendingChoice;
        GameObject overlayRoot;

        public static void EnsureInstance()
        {
            if (instance != null)
                return;

            var go = new GameObject(nameof(SaveConflictPrompt));
            DontDestroyOnLoad(go);
            instance = go.AddComponent<SaveConflictPrompt>();
        }

        public static IEnumerator WaitForChoice(
            PlayerProfile local,
            PlayerProfile remote,
            Action<SaveConflictChoice> onComplete)
        {
            EnsureInstance();
            yield return instance.PromptCoroutine(local, remote, onComplete);
        }

        IEnumerator PromptCoroutine(
            PlayerProfile local,
            PlayerProfile remote,
            Action<SaveConflictChoice> onComplete)
        {
            pendingChoice = null;
            BuildOverlay(local, remote);

            while (!pendingChoice.HasValue)
                yield return null;

            var choice = pendingChoice.Value;
            DestroyOverlay();
            onComplete?.Invoke(choice);
        }

        void BuildOverlay(PlayerProfile local, PlayerProfile remote)
        {
            DestroyOverlay();

            overlayRoot = new GameObject("SaveConflictOverlay");
            var canvasGo = overlayRoot;
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 5000;
            canvasGo.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasGo.AddComponent<GraphicRaycaster>();

            var dimmer = CreatePanel(canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero,
                new Color(0f, 0f, 0f, 0.65f));

            var panel = CreatePanel(dimmer.transform,
                new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-280f, -200f), new Vector2(280f, 200f),
                new Color(0.12f, 0.14f, 0.18f, 0.98f));

            int localCoins = local?.homeWorld?.Coin ?? 0;
            int remoteCoins = remote?.homeWorld?.Coin ?? 0;
            int localVer = local?.saveVersion ?? 0;
            int remoteVer = remote?.saveVersion ?? 0;

            CreateLabel(panel.transform, "Cloud Save Conflict",
                new Vector2(0f, 150f), 28, FontStyle.Bold);
            CreateLabel(panel.transform,
                $"Device: {localCoins:N0} coins · v{localVer}\nCloud: {remoteCoins:N0} coins · v{remoteVer}",
                new Vector2(0f, 70f), 20, FontStyle.Normal);

            CreateButton(panel.transform, "Merge (recommended)", new Vector2(0f, -10f),
                new Color(0.25f, 0.55f, 0.85f), () => pendingChoice = SaveConflictChoice.Merge);
            CreateButton(panel.transform, "Keep this device", new Vector2(0f, -70f),
                new Color(0.35f, 0.65f, 0.4f), () => pendingChoice = SaveConflictChoice.UseLocal);
            CreateButton(panel.transform, "Use cloud save", new Vector2(0f, -130f),
                new Color(0.75f, 0.35f, 0.3f), () => pendingChoice = SaveConflictChoice.UseCloud);
        }

        static GameObject CreatePanel(
            Transform parent,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 offsetMin,
            Vector2 offsetMax,
            Color color)
        {
            var go = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            return go;
        }

        static void CreateLabel(Transform parent, string text, Vector2 pos, int fontSize, FontStyle style)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(520f, 80f);
            rt.anchoredPosition = pos;

            var label = go.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.text = text;
        }

        static void CreateButton(Transform parent, string label, Vector2 pos, Color color, Action onClick)
        {
            var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(360f, 44f);
            rt.anchoredPosition = pos;
            go.GetComponent<Image>().color = color;

            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var textRt = textGo.GetComponent<RectTransform>();
            textRt.anchorMin = Vector2.zero;
            textRt.anchorMax = Vector2.one;
            textRt.offsetMin = Vector2.zero;
            textRt.offsetMax = Vector2.zero;

            var text = textGo.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.white;
            text.text = label;

            go.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
        }

        void DestroyOverlay()
        {
            if (overlayRoot != null)
            {
                Destroy(overlayRoot);
                overlayRoot = null;
            }
        }

        void OnDestroy()
        {
            if (instance == this)
                instance = null;
        }
    }
}
