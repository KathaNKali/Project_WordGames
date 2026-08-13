using UnityEngine;

/// <summary>
/// Static utility class for converting between grid coordinates and world positions.
/// All conversions are based on the provided GridConfig — no singleton dependency.
/// Grid lies on the XZ plane; Y axis is always the grid origin's Y value.
/// </summary>
public static class GridCoordinateUtil
{
    /// <summary>
    /// Converts a grid position (column, row) to a world-space position
    /// at the center of that cell on the XZ plane.
    /// </summary>
    /// <param name="gridPos">The grid coordinate (x = column, y = row).</param>
    /// <param name="config">The GridConfig defining cell size and world origin.</param>
    /// <returns>World position at the center of the given cell.</returns>
    public static Vector3 GridToWorld(Vector2Int gridPos, GridConfig config)
    {
        return new Vector3(
            config.originWorldPosition.x + gridPos.x * config.cellSize,
            config.originWorldPosition.y,
            config.originWorldPosition.z + gridPos.y * config.cellSize
        );
    }

    /// <summary>
    /// Converts a world-space position to the nearest grid coordinate.
    /// Uses RoundToInt on the X and Z axes relative to the grid origin.
    /// </summary>
    /// <param name="worldPos">The world position to convert.</param>
    /// <param name="config">The GridConfig defining cell size and world origin.</param>
    /// <returns>The nearest grid coordinate.</returns>
    public static Vector2Int WorldToGrid(Vector3 worldPos, GridConfig config)
    {
        int col = Mathf.RoundToInt((worldPos.x - config.originWorldPosition.x) / config.cellSize);
        int row = Mathf.RoundToInt((worldPos.z - config.originWorldPosition.z) / config.cellSize);
        return new Vector2Int(col, row);
    }

    /// <summary>
    /// Returns true if the given grid position is within the grid bounds.
    /// Does NOT check whether the cell is void — use GridManager.IsCellValid for that.
    /// </summary>
    /// <param name="pos">The grid coordinate to check.</param>
    /// <param name="config">The GridConfig defining grid dimensions.</param>
    /// <returns>True if the position is within bounds.</returns>
    public static bool IsOnGrid(Vector2Int pos, GridConfig config)
    {
        return config.IsInBounds(pos);
    }
}
