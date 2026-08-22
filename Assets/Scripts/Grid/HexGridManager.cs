using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Game.Grid
{
    /// <summary>
    /// Wraps the ground Tilemap configured for flat-top hexagons. Provides
    /// world&lt;-&gt;cell conversion, neighbor lookup and per-cell occupied/free
    /// state for building placement.
    ///
    /// For this hexagon cellSwizzle, Unity's Grid.GetCellCenterWorld and
    /// Tilemap.GetCellCenterWorld disagree by a constant world-space offset
    /// (confirmed visually: Grid's reported "center" for a cell actually sits
    /// on a corner shared by three tiles, not inside any one of them). Since
    /// Grid.WorldToCell partitions space relative to ITS OWN (shifted) notion
    /// of cell centers, its cell boundaries are offset from where the tiles
    /// are actually rendered - which made the highlighted cell change before
    /// the cursor visually reached the next hex. WorldToCell corrects for this
    /// by shifting the query point into the Grid's frame before classifying it,
    /// using the offset measured once at startup against the true (Tilemap)
    /// center of cell (0,0,0).
    /// </summary>
    public class HexGridManager : MonoBehaviour
    {
        public static HexGridManager Instance { get; private set; }

        [SerializeField] private UnityEngine.Grid _grid;
        [SerializeField] private Tilemap _groundTilemap;

        private readonly HashSet<Vector3Int> _occupiedCells = new HashSet<Vector3Int>();

        private Vector3? _gridToTilemapOffset;

        private Vector3 GridToTilemapOffset
        {
            get
            {
                if (!_gridToTilemapOffset.HasValue)
                {
                    _gridToTilemapOffset = _groundTilemap.GetCellCenterWorld(Vector3Int.zero) - _grid.GetCellCenterWorld(Vector3Int.zero);
                }
                return _gridToTilemapOffset.Value;
            }
        }

        // Flat-top hex on a Unity Tilemap (cellLayout=Hexagon, cellSwizzle=YXZ):
        // neighbor deltas depend on the parity of the cell's x coordinate.
        // Verified empirically against Tilemap.GetCellCenterWorld (equal distance to all 6).
        private static readonly Vector3Int[] EvenXNeighbors =
        {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
            new Vector3Int(-1, 1, 0), new Vector3Int(-1, -1, 0),
        };

        private static readonly Vector3Int[] OddXNeighbors =
        {
            new Vector3Int(1, 0, 0), new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0), new Vector3Int(0, -1, 0),
            new Vector3Int(1, 1, 0), new Vector3Int(1, -1, 0),
        };

        private void Awake()
        {
            Instance = this;
        }

        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            return _grid.WorldToCell(worldPosition - GridToTilemapOffset);
        }

        public Vector3 CellToWorld(Vector3Int cell)
        {
            return _groundTilemap.GetCellCenterWorld(cell);
        }

        public bool HasGroundTile(Vector3Int cell)
        {
            return _groundTilemap.HasTile(cell);
        }

        public bool IsOccupied(Vector3Int cell)
        {
            return _occupiedCells.Contains(cell);
        }

        public bool IsFree(Vector3Int cell)
        {
            return HasGroundTile(cell) && !IsOccupied(cell);
        }

        public void SetOccupied(Vector3Int cell, bool occupied)
        {
            if (occupied)
            {
                _occupiedCells.Add(cell);
            }
            else
            {
                _occupiedCells.Remove(cell);
            }
        }

        public IEnumerable<Vector3Int> GetNeighbors(Vector3Int cell)
        {
            var offsets = (cell.x & 1) == 0 ? EvenXNeighbors : OddXNeighbors;
            foreach (var offset in offsets)
            {
                yield return cell + offset;
            }
        }
    }
}
