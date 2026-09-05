using UnityEngine;
using StickersOut.Core;

namespace StickersOut.Gameplay
{
    /// <summary>
    /// Controller-layer component that owns the gameplay camera. On
    /// level load it reads the current viewport aspect ratio and the
    /// loaded GridModel's world bounds, computes the required
    /// orthographic size/position via CameraFramingCalculator, and
    /// applies it to the Camera component.
    ///
    /// Framing is recomputed on load (and can be manually re-triggered
    /// via Refit), not per-frame, since grid dimensions are fixed for
    /// the duration of a level.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class CameraRigController : MonoBehaviour
    {
        [SerializeField] private GridStageConfig stageConfig;
        [SerializeField] private Camera targetCamera;

        private GridModel currentGrid;

        private void Reset()
        {
            targetCamera = GetComponent<Camera>();
        }

        private void Awake()
        {
            if (targetCamera == null)
            {
                targetCamera = GetComponent<Camera>();
            }

            targetCamera.orthographic = true;
        }

        /// <summary>
        /// Frames the camera to fit the given grid's world bounds plus
        /// the configured margin, using the camera's current aspect ratio.
        /// </summary>
        public void LoadLevel(GridModel grid)
        {
            currentGrid = grid;
            Refit();
        }

        /// <summary>
        /// Recomputes and applies camera framing for the currently
        /// loaded grid. Call this after an aspect ratio change
        /// (e.g. device rotation) if live re-framing is needed.
        /// </summary>
        public void Refit()
        {
            ApplyHudViewportRect();

            if (currentGrid == null)
            {
                return;
            }

            float margin = stageConfig != null ? stageConfig.Margin : 0f;

            CameraFraming framing = CameraFramingCalculator.Calculate(
                currentGrid.WorldWidth,
                currentGrid.WorldHeight,
                margin,
                targetCamera.aspect);

            targetCamera.orthographicSize = framing.OrthographicSize;

            Vector3 pos = targetCamera.transform.position;
            pos.x = framing.Center.x;
            pos.y = framing.Center.y;
            targetCamera.transform.position = pos;
        }

        /// <summary>
        /// Sets the camera's normalized viewport rect to the center
        /// "game area" band, reserving the top/bottom fractions of the
        /// full screen for HUD (see GridStageConfig). After this,
        /// targetCamera.aspect reflects the game area's own aspect
        /// ratio (not the full screen's), so CameraFramingCalculator
        /// fits the grid correctly within that band.
        /// </summary>
        private void ApplyHudViewportRect()
        {
            if (stageConfig == null)
            {
                return;
            }

            float top = stageConfig.TopHudRatio;
            float bottom = stageConfig.BottomHudRatio;
            float gameAreaHeight = stageConfig.GameAreaRatio;

            targetCamera.rect = new Rect(0f, bottom, 1f, gameAreaHeight);
        }

        private void Start()
        {
            if (currentGrid != null)
            {
                Refit();
            }
        }
    }
}
