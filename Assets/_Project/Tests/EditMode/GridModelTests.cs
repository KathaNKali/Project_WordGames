using NUnit.Framework;
using UnityEngine;
using StickersOut.Core;

namespace StickersOut.Tests.EditMode
{
    public class GridModelTests
    {
        [Test]
        public void CellSize_SquareGrid_FitsDesignArea()
        {
            var grid = new GridModel(columns: 10, rows: 10, designAreaWidth: 10f, designAreaHeight: 16f);

            // Constrained by width (10/10=1) vs height (16/10=1.6) -> min is 1
            Assert.AreEqual(1f, grid.CellSize, 0.0001f);
            Assert.AreEqual(10f, grid.WorldWidth, 0.0001f);
            Assert.AreEqual(10f, grid.WorldHeight, 0.0001f);
        }

        [Test]
        public void CellSize_TallGrid_ConstrainedByHeight()
        {
            var grid = new GridModel(columns: 6, rows: 20, designAreaWidth: 10f, designAreaHeight: 16f);

            // width/cols = 10/6 = 1.667, height/rows = 16/20 = 0.8 -> min is 0.8
            Assert.AreEqual(0.8f, grid.CellSize, 0.0001f);
        }

        [Test]
        public void CellSize_WideGrid_ConstrainedByWidth()
        {
            var grid = new GridModel(columns: 20, rows: 6, designAreaWidth: 10f, designAreaHeight: 16f);

            // width/cols = 10/20 = 0.5, height/rows = 16/6 = 2.667 -> min is 0.5
            Assert.AreEqual(0.5f, grid.CellSize, 0.0001f);
        }

        [Test]
        public void GridToWorld_OriginCell_IsBottomLeftOfCenteredBoard()
        {
            var grid = new GridModel(columns: 4, rows: 4, designAreaWidth: 8f, designAreaHeight: 8f);
            // cellSize = 2, worldWidth/Height = 8, half = 4

            Vector2 cell00 = grid.GridToWorld(0, 0);
            Assert.AreEqual(new Vector2(-3f, -3f), cell00);

            Vector2 lastCell = grid.GridToWorld(3, 3);
            Assert.AreEqual(new Vector2(3f, 3f), lastCell);
        }

        [Test]
        public void WorldToGrid_IsInverseOfGridToWorld_ForCellCenters()
        {
            var grid = new GridModel(columns: 5, rows: 7, designAreaWidth: 10f, designAreaHeight: 14f);

            for (int col = 0; col < grid.Columns; col++)
            {
                for (int row = 0; row < grid.Rows; row++)
                {
                    Vector2 world = grid.GridToWorld(col, row);
                    Vector2Int back = grid.WorldToGrid(world);
                    Assert.AreEqual(col, back.x, $"col mismatch at ({col},{row})");
                    Assert.AreEqual(row, back.y, $"row mismatch at ({col},{row})");
                }
            }
        }

        [Test]
        public void WallAndVoid_DefaultFalse_AndSettable()
        {
            var grid = new GridModel(columns: 3, rows: 3, designAreaWidth: 6f, designAreaHeight: 6f);

            Assert.IsFalse(grid.IsWall(1, 1));
            Assert.IsFalse(grid.IsVoid(1, 1));

            grid.SetWall(1, 1, true);
            grid.SetVoid(2, 0, true);

            Assert.IsTrue(grid.IsWall(1, 1));
            Assert.IsTrue(grid.IsVoid(2, 0));
            Assert.IsFalse(grid.IsWall(0, 0));
        }

        [Test]
        public void OutOfBoundsAccess_Throws()
        {
            var grid = new GridModel(columns: 3, rows: 3, designAreaWidth: 6f, designAreaHeight: 6f);

            Assert.Throws<System.ArgumentOutOfRangeException>(() => grid.IsWall(3, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => grid.IsWall(0, -1));
        }
    }
}
