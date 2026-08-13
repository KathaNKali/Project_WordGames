using UnityEngine;

/// <summary>
/// Draws a Gizmos-based debug visualization of the logical grid in the Scene view.
/// Works in both Edit mode and Play mode.
/// Has zero gameplay logic and never modifies GridManager state.
/// </summary>
public class GridVisualizer : MonoBehaviour
{
    [SerializeField] private GridConfig _config;

    [Header("Debug Gizmos")]
    [SerializeField] public bool showDebugGizmos = true;
    [SerializeField] private Color _emptyColor = Color.green;
    [SerializeField] private Color _occupiedColor = Color.red;

    private void OnDrawGizmos()
    {
        if (!showDebugGizmos || _config == null)
            return;

        for (int x = 0; x < _config.width; x++)
        {
            for (int z = 0; z < _config.height; z++)
            {
                Vector2Int gridPos = new Vector2Int(x, z);

                // Skip void cells — no tile, no gizmo
                if (_config.IsVoidCell(gridPos))
                    continue;

                Vector3 worldPos = GridCoordinateUtil.GridToWorld(gridPos, _config);
                Vector3 cubeSize = new Vector3(_config.cellSize * 0.9f, 0.05f, _config.cellSize * 0.9f);

                // In Play mode — read live cell state from GridManager
                if (Application.isPlaying && GridManager.Instance != null)
                {
                    GridCell cell = GridManager.Instance.GetCell(gridPos);
                    Gizmos.color = (cell != null && cell.isOccupied) ? _occupiedColor : _emptyColor;
                }
                else
                {
                    // Edit mode — all non-void cells drawn as empty
                    Gizmos.color = _emptyColor;
                }

                Gizmos.DrawWireCube(worldPos, cubeSize);
            }
        }
    }
}
