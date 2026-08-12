using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject that defines the configuration for a grid.
/// Create via: Assets > Create > BlockJam > Grid Config
///
/// One GridConfig can be shared across multiple levels if they use the same
/// grid dimensions, or each level can reference its own config.
///
/// Void Cell Rules:
///   - All cells default to playable on grid initialization.
///   - Cells listed in voidCells are marked isVoid = true after init.
///   - Void cells have NO floor tile and are always impassable.
///   - Empty cells (not in voidCells) within a block's bounding box remain passable.
/// </summary>
[CreateAssetMenu(fileName = "GridConfig", menuName = "BlockJam/Grid Config")]
public class GridConfig : ScriptableObject
{
    // -------------------------------------------------------------------------
    // Grid Dimensions
    // -------------------------------------------------------------------------

    [Header("Grid Dimensions")]

    [Tooltip("Number of columns on the grid (X axis).")]
    [Min(1)]
    public int width = 6;

    [Tooltip("Number of rows on the grid (Z axis).")]
    [Min(1)]
    public int height = 6;

    // -------------------------------------------------------------------------
    // Cell Size
    // -------------------------------------------------------------------------

    [Header("Cell Size")]

    [Tooltip(
        "World-unit size of each cell. " +
        "A block occupying one cell will be sized to fit within this.")]
    [Min(0.1f)]
    public float cellSize = 1.0f;

    // -------------------------------------------------------------------------
    // Grid Origin
    // -------------------------------------------------------------------------

    [Header("Grid Origin")]

    [Tooltip(
        "World position of the grid's (0, 0) cell (bottom-left corner). " +
        "GridCoordinateUtil uses this as the offset for all conversions.")]
    public Vector3 originWorldPosition = Vector3.zero;

    // -------------------------------------------------------------------------
    // Void Cells
    // -------------------------------------------------------------------------

    [Header("Void Cells")]

    [Tooltip(
        "Explicit list of cells that are non-playable (void). " +
        "Void cells have no floor tile and are always impassable. " +
        "All other cells are playable by default. " +
        "Use this to give the grid an irregular shape.")]
    public List<Vector2Int> voidCells = new List<Vector2Int>();

    // -------------------------------------------------------------------------
    // Validation
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns true if the given grid position is within bounds.
    /// Does NOT check void status — GridManager handles that.
    /// </summary>
    public bool IsInBounds(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width &&
               pos.y >= 0 && pos.y < height;
    }

    /// <summary>
    /// Returns true if the given position is explicitly listed as a void cell.
    /// </summary>
    public bool IsVoidCell(Vector2Int pos)
    {
        return voidCells != null && voidCells.Contains(pos);
    }

#if UNITY_EDITOR
    // -------------------------------------------------------------------------
    // Editor Validation
    // -------------------------------------------------------------------------

    private void OnValidate()
    {
        // Clamp dimensions to safe minimums
        width    = Mathf.Max(1, width);
        height   = Mathf.Max(1, height);
        cellSize = Mathf.Max(0.1f, cellSize);

        // Warn if any void cell is out of bounds
        if (voidCells == null) return;
        foreach (var cell in voidCells)
        {
            if (!IsInBounds(cell))
            {
                Debug.LogWarning(
                    $"[GridConfig] Void cell {cell} is out of bounds " +
                    $"for grid size {width}x{height}. " +
                    $"This will be ignored at runtime.",
                    this);
            }
        }
    }
#endif
}
