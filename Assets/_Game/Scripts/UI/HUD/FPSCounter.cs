using UnityEngine;
using TMPro;

namespace IslandHarvest.Game
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class FPSCounter : MonoBehaviour
    {
        [Tooltip("Sets the monospace size for the FPS text.")]
        [SerializeField] private float monoSpace = 1f;

        private TextMeshProUGUI displayText; // Reference to the TextMeshProUGUI component for displaying the FPS.
        private float deltaTime = 0.0f; // Stores the frame time to calculate FPS.

        void Awake()
        {
            // Get and cache the TextMeshProUGUI component.
            displayText = GetComponent<TextMeshProUGUI>();
        }

        void Start()
        {
            // Initialize the FPS display.
            UpdateDisplay();
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        void Update()
        {
            deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
            float fps = 1.0f / deltaTime;
            UpdateDisplay(fps);
        }
#else
        void Start()
        {
            gameObject.SetActive(false);
        }
#endif

        /// <summary>
        /// Updates the displayed FPS value. If no value is provided, it shows default text.
        /// </summary>
        /// <param name="fps">The calculated FPS to display.</param>
        void UpdateDisplay(float fps = 0)
        {
            // Set the text with monospace formatting and display the FPS rounded up to the nearest integer.
            displayText.text = $"<mspace={monoSpace}em>FPS:{Mathf.Ceil(fps)}";
        }
    }
}
