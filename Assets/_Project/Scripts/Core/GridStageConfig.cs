using UnityEngine;

namespace StickersOut.Core
{
    /// <summary>
    /// Design-time configuration for how the grid is framed on screen.
    /// The grid always fits inside a fixed "stage" world-size footprint;
    /// cell size adapts per-level to fill that footprint (see GridModel).
    /// Single source of truth so designers can tune stage size/margins
    /// without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "GridStageConfig", menuName = "StickersOut/Grid Stage Config")]
    public class GridStageConfig : ScriptableObject
    {
        [Header("Stage Footprint (world units)")]
        [Tooltip("Maximum width the grid may occupy, in world units.")]
        [SerializeField] private float designAreaWidth = 10f;

        [Tooltip("Maximum height the grid may occupy, in world units.")]
        [SerializeField] private float designAreaHeight = 16f;

        [Header("Camera Framing")]
        [Tooltip("Fixed padding added around the grid on all sides when framing the camera, in world units.")]
        [SerializeField] private float margin = 1f;

        [Header("HUD Layout (portrait screen fractions)")]
        [Tooltip("Fraction of full screen height reserved for the top HUD (0-1). The game camera's viewport starts below this band.")]
        [SerializeField, Range(0f, 1f)] private float topHudRatio = 0.2f;

        [Tooltip("Fraction of full screen height reserved for the bottom HUD (0-1). The game camera's viewport ends above this band.")]
        [SerializeField, Range(0f, 1f)] private float bottomHudRatio = 0.2f;

        public float DesignAreaWidth => designAreaWidth;
        public float DesignAreaHeight => designAreaHeight;
        public float Margin => margin;

        /// <summary>Fraction of full screen height reserved for the top HUD (0-1).</summary>
        public float TopHudRatio => topHudRatio;

        /// <summary>Fraction of full screen height reserved for the bottom HUD (0-1).</summary>
        public float BottomHudRatio => bottomHudRatio;

        /// <summary>Fraction of full screen height available to the game area (1 - top - bottom), clamped to a small positive minimum.</summary>
        public float GameAreaRatio => Mathf.Max(0.01f, 1f - topHudRatio - bottomHudRatio);

        private void OnValidate()
        {
            if (topHudRatio + bottomHudRatio > 0.98f)
            {
                Debug.LogWarning($"[{nameof(GridStageConfig)}] topHudRatio ({topHudRatio}) + bottomHudRatio ({bottomHudRatio}) leaves little/no room for the game area. Clamping.", this);
                float scale = 0.98f / (topHudRatio + bottomHudRatio);
                topHudRatio *= scale;
                bottomHudRatio *= scale;
            }
        }
    }
}
