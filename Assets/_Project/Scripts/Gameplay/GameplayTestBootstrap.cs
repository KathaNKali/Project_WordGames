using UnityEngine;

namespace StickersOut.Gameplay
{
    /// <summary>
    /// TEMPORARY scene bootstrap for manually validating grid/camera
    /// framing before a real LevelLoader exists. Wires a GridDebugView's
    /// test grid into the CameraRigController on scene start.
    /// </summary>
    public class GameplayTestBootstrap : MonoBehaviour
    {
        [SerializeField] private CameraRigController cameraRig;
        [SerializeField] private GridDebugView gridDebugView;

        private void Start()
        {
            if (cameraRig == null || gridDebugView == null)
            {
                Debug.LogWarning("GameplayTestBootstrap is missing references; camera will not be framed.");
                return;
            }

            cameraRig.LoadLevel(gridDebugView.BuildTestGrid());
        }
    }
}
