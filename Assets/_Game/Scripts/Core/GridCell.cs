using UnityEngine;

/// <summary>
/// Pure data class representing a single cell on the grid.
/// Not a MonoBehaviour — owned and managed entirely by GridManager.
/// </summary>
[System.Serializable]
public class GridCell
{
    // -------------------------------------------------------------------------
    // Fields
    // -------------------------------------------------------------------------

    /// <summary>
    /// This cell's coordinate on the logical grid (column, row).
    /// X = column, Y = row.
    /// </summary>
    public Vector2Int gridPosition;

    /// <summary>
    /// True if a Block is currently occupying this cell.
    /// Always check this before calling GridManager.CanMoveTo().
    /// </summary>
    public bool isOccupied;

    /// <summary>
    /// True if this cell is a void (non-playable) cell.
    /// Void cells:
    ///   - Have NO floor tile rendered by GridVisualizer.
    ///   - Are ALWAYS impassable regardless of isOccupied.
    ///   - Are defined explicitly in GridConfig.voidCells.
    /// All cells default to isVoid = false on grid initialization.
    /// </summary>
    public bool isVoid;

    /// <summary>
    /// Reference to the Block occupying this cell.
    /// Null if the cell is empty.
    /// NOTE: Block is currently a stub class. Will be fully implemented in Task 5.
    /// </summary>
    public Block occupant;

    // -------------------------------------------------------------------------
    // Constructor
    // -------------------------------------------------------------------------

    /// <summary>
    /// Creates a new GridCell at the given grid coordinate.
    /// Defaults to playable (isVoid = false) and empty (isOccupied = false).
    /// </summary>
    public GridCell(Vector2Int position)
    {
        gridPosition = position;
        isOccupied   = false;
        isVoid       = false;
        occupant     = null;
    }

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    /// <summary>
    /// Returns true if this cell is playable AND not occupied.
    /// Void cells always return false.
    /// </summary>
    public bool IsAvailable()
    {
        return !isVoid && !isOccupied;
    }

    /// <summary>
    /// Marks the cell as occupied by the given Block.
    /// </summary>
    public void SetOccupant(Block block)
    {
        occupant   = block;
        isOccupied = block != null;
    }

    /// <summary>
    /// Clears the occupant and marks the cell as empty.
    /// </summary>
    public void ClearOccupant()
    {
        occupant   = null;
        isOccupied = false;
    }

    /// <summary>
    /// Debug-friendly string representation.
    /// </summary>
    public override string ToString()
    {
        if (isVoid)     return "[V]";
        if (isOccupied) return "[B]";
        return "[ ]";
    }
}
