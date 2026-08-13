using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Singleton MonoBehaviour that owns and manages the logical grid.
/// Reads configuration from GridConfig on Awake.
/// Does NOT handle any visuals — that is GridVisualizer's responsibility.
/// </summary>
public class GridManager : MonoBehaviour
{
    public static GridManager Instance { get; private set; }

    [SerializeField] private GridConfig _config;

    private GridCell[,] _grid;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[GridManager] Duplicate instance destroyed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        _grid = new GridCell[_config.width, _config.height];

        for (int x = 0; x < _config.width; x++)
        {
            for (int z = 0; z < _config.height; z++)
            {
                _grid[x, z] = new GridCell(new Vector2Int(x, z));
            }
        }

        foreach (Vector2Int voidPos in _config.voidCells)
        {
            if (_config.IsInBounds(voidPos))
            {
                _grid[voidPos.x, voidPos.y].isVoid = true;
            }
            else
            {
                Debug.LogWarning($"[GridManager] Void cell {voidPos} is out of bounds and will be ignored.");
            }
        }

        Debug.Log($"[GridManager] Grid initialized: {_config.width}x{_config.height}, {_config.voidCells.Count} void cell(s).");
    }

    // ─── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns true if the position is within grid bounds and is not a void cell.
    /// </summary>
    public bool IsCellValid(Vector2Int pos)
    {
        return _config.IsInBounds(pos) && !_grid[pos.x, pos.y].isVoid;
    }

    /// <summary>
    /// Returns true if the cell is valid and currently unoccupied.
    /// </summary>
    public bool IsCellEmpty(Vector2Int pos)
    {
        return IsCellValid(pos) && !_grid[pos.x, pos.y].isOccupied;
    }

    /// <summary>
    /// Returns true if a block can move to this position —
    /// the cell must be valid, not void, and not occupied.
    /// </summary>
    public bool CanMoveTo(Vector2Int pos)
    {
        return IsCellEmpty(pos);
    }

    /// <summary>
    /// Registers a Block on the cell at the given grid position.
    /// Rejects and logs an error if the cell is void, out of bounds, or already occupied.
    /// Must be called per cell for multi-cell block groups.
    /// </summary>
    public void RegisterBlock(Block b, Vector2Int pos)
    {
        if (!_config.IsInBounds(pos))
        {
            Debug.LogError($"[GridManager] RegisterBlock failed — position {pos} is out of bounds.");
            return;
        }

        GridCell cell = _grid[pos.x, pos.y];

        if (cell.isVoid)
        {
            Debug.LogError($"[GridManager] RegisterBlock failed — position {pos} is a void cell.");
            return;
        }

        if (cell.isOccupied)
        {
            Debug.LogError($"[GridManager] RegisterBlock failed — position {pos} is already occupied by {cell.occupant}.");
            return;
        }

        cell.SetOccupant(b);
    }

    /// <summary>
    /// Removes the block registration from the cell at the given grid position.
    /// Must be called per cell for multi-cell block groups.
    /// </summary>
    public void UnregisterBlock(Vector2Int pos)
    {
        if (!IsCellValid(pos))
        {
            Debug.LogWarning($"[GridManager] UnregisterBlock skipped — position {pos} is invalid.");
            return;
        }

        _grid[pos.x, pos.y].ClearOccupant();
    }

    /// <summary>
    /// Returns the Block occupying the given grid position, or null if empty or invalid.
    /// </summary>
    public Block GetBlockAt(Vector2Int pos)
    {
        if (!IsCellValid(pos))
            return null;

        return _grid[pos.x, pos.y].occupant;
    }

    /// <summary>
    /// Returns the GridCell at the given position, or null if out of bounds.
    /// </summary>
    public GridCell GetCell(Vector2Int pos)
    {
        if (!_config.IsInBounds(pos))
            return null;

        return _grid[pos.x, pos.y];
    }

    /// <summary>
    /// Logs an ASCII representation of the current grid state to the console.
    /// V = void cell, X = occupied, . = empty.
    /// </summary>
    public void PrintGridState()
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.AppendLine($"[GridManager] Grid State ({_config.width}x{_config.height}):");

        for (int z = _config.height - 1; z >= 0; z--)
        {
            System.Text.StringBuilder row = new System.Text.StringBuilder();
            for (int x = 0; x < _config.width; x++)
            {
                GridCell cell = _grid[x, z];
                if (cell.isVoid)         row.Append("V ");
                else if (cell.isOccupied) row.Append("X ");
                else                     row.Append(". ");
            }
            sb.AppendLine(row.ToString());
        }

        Debug.Log(sb.ToString());
    }
}
