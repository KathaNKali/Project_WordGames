using UnityEngine;

namespace StickersOut.Core
{
    /// <summary>
    /// Result of a camera-framing calculation: what an orthographic
    /// camera's size and world-space center position should be to
    /// fit a target bounds + margin, for a given viewport aspect ratio.
    /// </summary>
    public readonly struct CameraFraming
    {
        public readonly float OrthographicSize;
        public readonly Vector2 Center;

        public CameraFraming(float orthographicSize, Vector2 center)
        {
            OrthographicSize = orthographicSize;
            Center = center;
        }
    }

    /// <summary>
    /// Pure math for fitting a fixed-size rectangle (the grid + margin)
    /// inside an orthographic camera's viewport, regardless of the
    /// camera's aspect ratio. No Camera/Screen API calls - aspect ratio
    /// is passed in - so this is EditMode-testable without a scene.
    /// </summary>
    public static class CameraFramingCalculator
    {
        /// <summary>
        /// Computes the orthographic size and center position required
        /// to fit a grid of the given world width/height, padded by a
        /// uniform margin on all sides, inside a viewport of the given
        /// aspect ratio (width / height).
        /// </summary>
        /// <param name="gridWorldWidth">Grid width in world units.</param>
        /// <param name="gridWorldHeight">Grid height in world units.</param>
        /// <param name="margin">Uniform padding added to all sides, in world units.</param>
        /// <param name="cameraAspect">Camera viewport aspect ratio (width / height).</param>
        /// <param name="gridCenter">World-space center of the grid bounds (defaults to origin).</param>
        public static CameraFraming Calculate(
            float gridWorldWidth,
            float gridWorldHeight,
            float margin,
            float cameraAspect,
            Vector2 gridCenter = default)
        {
            if (gridWorldWidth <= 0f) throw new System.ArgumentOutOfRangeException(nameof(gridWorldWidth));
            if (gridWorldHeight <= 0f) throw new System.ArgumentOutOfRangeException(nameof(gridWorldHeight));
            if (margin < 0f) throw new System.ArgumentOutOfRangeException(nameof(margin));
            if (cameraAspect <= 0f) throw new System.ArgumentOutOfRangeException(nameof(cameraAspect));

            float targetWidth = gridWorldWidth + margin * 2f;
            float targetHeight = gridWorldHeight + margin * 2f;

            // Orthographic size is half the viewport's vertical extent.
            // Fit-by-height gives size = targetHeight/2.
            // Fit-by-width requires size = (targetWidth / aspect) / 2,
            // since viewport width = 2*size*aspect.
            float sizeToFitHeight = targetHeight * 0.5f;
            float sizeToFitWidth = (targetWidth / cameraAspect) * 0.5f;

            float orthographicSize = Mathf.Max(sizeToFitHeight, sizeToFitWidth);

            return new CameraFraming(orthographicSize, gridCenter);
        }
    }
}
