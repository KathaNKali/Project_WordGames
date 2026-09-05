using UnityEngine;
using StickersOut.Core;

namespace StickersOut.Gameplay
{
    /// <summary>
    /// TEMPORARY debug visualizer for GridModel bounds and cell lines.
    /// Draws via Gizmos only (Editor Scene view) so we can validate
    /// camera framing before real GridCellView/BlockView art exists.
    /// Not part of the final rendering pipeline - replace with
    /// GridCellView per docs/PROJECT_STRUCTURE.md when art is ready.
    /// </summary>
    public class GridDebugView : MonoBehaviour
    {
        [SerializeField] private GridStageConfig stageConfig;
        [SerializeField] private int columns = 6;
        [SerializeField] private int rows = 10;
        [SerializeField] private Color gridColor = Color.cyan;
        [SerializeField] private Color boundsColor = Color.yellow;

        /// <summary>
        /// Builds a GridModel from the inspector-configured columns/rows
        /// and stage config. Used to feed CameraRigController.LoadLevel
        /// for manual visual testing without a real LevelLoader yet.
        /// </summary>
        public GridModel BuildTestGrid()
        {
            return new GridModel(columns, rows, stageConfig);
        }

        private void OnDrawGizmos()
        {
            if (stageConfig == null || columns <= 0 || rows <= 0)
            {
                return;
            }

            GridModel grid = new GridModel(columns, rows, stageConfig);

            Vector3 origin = transform.position;

            // Overall bounds
            Gizmos.color = boundsColor;
            Gizmos.DrawWireCube(origin, new Vector3(grid.WorldWidth, grid.WorldHeight, 0f));

            // Cell grid lines
            Gizmos.color = gridColor;
            float halfW = grid.WorldWidth * 0.5f;
            float halfH = grid.WorldHeight * 0.5f;

            for (int col = 0; col <= grid.Columns; col++)
            {
                float x = origin.x - halfW + col * grid.CellSize;
                Gizmos.DrawLine(new Vector3(x, origin.y - halfH, 0f), new Vector3(x, origin.y + halfH, 0f));
            }

            for (int row = 0; row <= grid.Rows; row++)
            {
                float y = origin.y - halfH + row * grid.CellSize;
                Gizmos.DrawLine(new Vector3(origin.x - halfW, y, 0f), new Vector3(origin.x + halfW, y, 0f));
            }
        }
    }
}
