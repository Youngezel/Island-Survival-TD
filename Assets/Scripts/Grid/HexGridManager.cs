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
        [SerializeField] private TileBase[] _grassTiles;
        [SerializeField] private TileBase _coastalTile;
        [SerializeField] private int _tileMaxHealth = 5;

        private readonly HashSet<Vector3Int> _occupiedCells = new HashSet<Vector3Int>();
        private readonly HashSet<Vector3Int> _tileCells = new HashSet<Vector3Int>();
        private readonly Dictionary<Vector3Int, int> _tileHealth = new Dictionary<Vector3Int, int>();

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
            ScanExistingTiles();
            RefreshAllTileVisuals();
        }

        private void ScanExistingTiles()
        {
            BoundsInt bounds = _groundTilemap.cellBounds;
            foreach (Vector3Int cell in bounds.allPositionsWithin)
            {
                if (_groundTilemap.HasTile(cell))
                {
                    _tileCells.Add(cell);
                }
            }
        }

        /// <summary>
        /// Repaints every already-placed tile with its correct variant -
        /// sand-ringed coastal art for cells bordering open water, one of
        /// the pure-grass variants otherwise. Needed once at startup since
        /// tiles authored directly in the scene predate variant selection.
        /// </summary>
        private void RefreshAllTileVisuals()
        {
            var cells = new List<Vector3Int>(_tileCells);
            foreach (Vector3Int cell in cells)
            {
                RefreshTileVisual(cell);
            }
        }

        /// <summary>Repaints a single cell with the art matching its current neighbors: coastal if any neighbor has no ground tile, otherwise a stable grass variant chosen from the cell's coordinates.</summary>
        private void RefreshTileVisual(Vector3Int cell)
        {
            bool coastal = false;
            foreach (Vector3Int neighbor in GetNeighbors(cell))
            {
                if (!HasGroundTile(neighbor))
                {
                    coastal = true;
                    break;
                }
            }

            TileBase tile = coastal ? _coastalTile : _grassTiles[GrassVariantIndex(cell)];
            _groundTilemap.SetTile(cell, tile);
        }

        /// <summary>Deterministic pick of a grass variant from cell coordinates, so a given cell always renders the same variant unless it toggles coastal.</summary>
        private int GrassVariantIndex(Vector3Int cell)
        {
            unchecked
            {
                int hash = cell.x * 73856093 ^ cell.y * 19349663;
                return Mathf.Abs(hash) % _grassTiles.Length;
            }
        }

        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            return _grid.WorldToCell(worldPosition - GridToTilemapOffset);
        }

        public Vector3 CellToWorld(Vector3Int cell)
        {
            return _groundTilemap.GetCellCenterWorld(cell);
        }

        /// <summary>
        /// World-space distance between the centers of two directly adjacent
        /// (straight up/down) hex cells. Used to convert a "range in tiles"
        /// stat into a world-space radius.
        /// </summary>
        public float HexStepWorldDistance => _grid.cellSize.x;

        public bool HasGroundTile(Vector3Int cell)
        {
            return _groundTilemap.HasTile(cell);
        }

        /// <summary>
        /// Paints ground at the given cell (used when a tile is purchased),
        /// picking coastal or grass art based on its neighbors, then
        /// refreshes any already-placed neighbors since this new tile may
        /// turn a previously-coastal one into an interior tile.
        /// </summary>
        public void PlaceGroundTile(Vector3Int cell)
        {
            _tileCells.Add(cell);
            RefreshTileVisual(cell);

            foreach (Vector3Int neighbor in GetNeighbors(cell))
            {
                if (HasGroundTile(neighbor))
                {
                    RefreshTileVisual(neighbor);
                }
            }
        }

        /// <summary>All cells that currently have a ground tile painted on them.</summary>
        public IEnumerable<Vector3Int> GetAllTileCells() => _tileCells;

        /// <summary>Current HP of the tile at this cell (lazily initialized to the max on first query).</summary>
        public int GetTileHealth(Vector3Int cell)
        {
            if (!HasGroundTile(cell))
            {
                return 0;
            }

            if (!_tileHealth.TryGetValue(cell, out int health))
            {
                health = _tileMaxHealth;
                _tileHealth[cell] = health;
            }

            return health;
        }

        /// <summary>Damages the tile at this cell, removing it (back to open water) once its HP is depleted.</summary>
        public void DamageTile(Vector3Int cell, int amount)
        {
            if (!HasGroundTile(cell) || amount <= 0)
            {
                return;
            }

            int health = GetTileHealth(cell) - amount;
            if (health <= 0)
            {
                _groundTilemap.SetTile(cell, null);
                _tileHealth.Remove(cell);
                _tileCells.Remove(cell);
                SetOccupied(cell, false);

                foreach (Vector3Int neighbor in GetNeighbors(cell))
                {
                    if (HasGroundTile(neighbor))
                    {
                        RefreshTileVisual(neighbor);
                    }
                }
            }
            else
            {
                _tileHealth[cell] = health;
            }
        }

        /// <summary>Converts a screen-space point (e.g. a pointer/drag position) to the hex cell under it.</summary>
        public Vector3Int ScreenToCell(Vector2 screenPosition, Camera camera)
        {
            Vector3 worldPosition = camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -camera.transform.position.z));
            worldPosition.z = 0f;
            return WorldToCell(worldPosition);
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
