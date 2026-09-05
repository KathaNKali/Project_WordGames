           
using System;
using UnityEngine;

namespace StickersOut.Core
{
    /// <summary>
    /// Pure-logic grid data for a level's board. Owns dimensions and
    /// static layout (wall/void cells) and derives world-space sizing
    /// from a GridStageConfig so the grid always fits inside a fixed
    /// design-area footprint regardless of row/column count.
    ///
    /// No MonoBehaviour/UnityEngine.Object dependency beyond value
    /// types (Vector2) so this is constructible/testable in EditMode
    /// without a scene.
    /// </summary>
    public class GridModel
    {
        public int Columns { get; }
        public int Rows { get; }

        /// <summary>Uniform cell size (world units) computed to fit the grid inside the stage design area.</summary>
        public float CellSize { get; }

        /// <summary>Total grid width in world units (Columns * CellSize).</summary>
        public float WorldWidth => Columns * CellSize;

        /// <summary>Total grid height in world units (Rows * CellSize).</summary>
        public float WorldHeight => Rows * CellSize;

        private readonly bool[] wallCells;
        private readonly bool[] voidCells;

        public GridModel(int columns, int rows, float designAreaWidth, float designAreaHeight)
        {
            if (columns <= 0) throw new ArgumentOutOfRangeException(nameof(columns));
            if (rows <= 0) throw new ArgumentOutOfRangeException(nameof(rows));
            if (designAreaWidth <= 0) throw new ArgumentOutOfRangeException(nameof(designAreaWidth));
            if (designAreaHeight <= 0) throw new ArgumentOutOfRangeException(nameof(designAreaHeight));

            Columns = columns;
            Rows = rows;

            // Cells stay square; the whole grid fits inside the design stage
            // by taking the more constraining of the two axes.
            CellSize = Mathf.Min(designAreaWidth / columns, designAreaHeight / rows);

            wallCells = new bool[columns * rows];
            voidCells = new bool[columns * rows];
        }

        public GridModel(int columns, int rows, GridStageConfig stageConfig)
            : this(columns, rows, GetWidth(stageConfig), GetHeight(stageConfig))
        {
        }

        private static float GetWidth(GridStageConfig stageConfig)
        {
            if (stageConfig == null) throw new ArgumentNullException(nameof(stageConfig));
            return stageConfig.DesignAreaWidth;
        }

        private static float GetHeight(GridStageConfig stageConfig)
        {
            if (stageConfig == null) throw new ArgumentNullException(nameof(stageConfig));
            return stageConfig.DesignAreaHeight;
        }

        public bool IsInBounds(int col, int row)
        {
            return col >= 0 && col < Columns && row >= 0 && row < Rows;
        }

        public bool IsWall(int col, int row)
        {
            CheckBounds(col, row);
            return wallCells[Index(col, row)];
        }

        public void SetWall(int col, int row, bool isWall)
        {
            CheckBounds(col, row);
            wallCells[Index(col, row)] = isWall;
        }

        public bool IsVoid(int col, int row)
        {
            CheckBounds(col, row);
            return voidCells[Index(col, row)];
        }

        public void SetVoid(int col, int row, bool isVoid)
        {
            CheckBounds(col, row);
            voidCells[Index(col, row)] = isVoid;
        }

        /// <summary>
        /// Converts a grid cell coordinate to a world-space position at
        /// the cell's center. Grid origin (0,0) maps to the bottom-left
        /// corner of the board; the board is centered on world origin.
        /// </summary>
        public Vector2 GridToWorld(int col, int row)
        {
            float halfWidth = WorldWidth * 0.5f;
            float halfHeight = WorldHeight * 0.5f;

            float x = -halfWidth + (col + 0.5f) * CellSize;
            float y = -halfHeight + (row + 0.5f) * CellSize;
            return new Vector2(x, y);
        }

        /// <summary>
        /// Converts a world-space position to the grid cell coordinate
        /// containing it. Does not clamp to bounds - check IsInBounds
        /// on the result if needed.
        /// </summary>
        public Vector2Int WorldToGrid(Vector2 worldPos)
        {
            float halfWidth = WorldWidth * 0.5f;
            float halfHeight = WorldHeight * 0.5f;

            int col = Mathf.FloorToInt((worldPos.x + halfWidth) / CellSize);
            int row = Mathf.FloorToInt((worldPos.y + halfHeight) / CellSize);
            return new Vector2Int(col, row);
        }

        private void CheckBounds(int col, int row)
        {
            if (!IsInBounds(col, row))
            {
                throw new ArgumentOutOfRangeException($"Cell ({col},{row}) is out of bounds for a {Columns}x{Rows} grid.");
            }
        }

        private int Index(int col, int row) => row * Columns + col;
    }
}
