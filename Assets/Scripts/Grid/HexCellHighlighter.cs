using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

namespace Game.Grid
{
    /// <summary>
    /// Highlights the hex cell under the mouse in Play mode, colored by its
    /// occupied/free/no-tile state. Paints a single tile onto a dedicated
    /// highlight Tilemap that shares the same Grid as the ground tilemap, so
    /// it always aligns exactly with the hex cells Unity itself renders -
    /// there is no separate world-position math for a marker sprite to get
    /// out of sync with the camera.
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class HexCellHighlighter : MonoBehaviour
    {
        [SerializeField] private HexGridManager _hexGridManager;
        [SerializeField] private Camera _camera;
        [SerializeField] private TileBase _highlightTile;
        [SerializeField] private Color _freeColor = new Color(0.51f, 0.76f, 0.29f, 0.6f);
        [SerializeField] private Color _occupiedColor = new Color(0.78f, 0.25f, 0.18f, 0.6f);
        [SerializeField] private Color _noTileColor = new Color(0.35f, 0.35f, 0.35f, 0.35f);

        private Tilemap _tilemap;
        private Vector3Int? _highlightedCell;

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void Update()
        {
            if (_hexGridManager == null || _camera == null || Mouse.current == null)
            {
                return;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            bool cursorInsideView = screenPosition.x >= 0f && screenPosition.x <= Screen.width
                && screenPosition.y >= 0f && screenPosition.y <= Screen.height;
            if (!cursorInsideView)
            {
                ClearHighlight();
                return;
            }

            var cell = _hexGridManager.ScreenToCell(screenPosition, _camera);

            if (_highlightedCell.HasValue && _highlightedCell.Value != cell)
            {
                _tilemap.SetTile(_highlightedCell.Value, null);
            }

            _tilemap.SetTile(cell, _highlightTile);
            _tilemap.SetTileFlags(cell, TileFlags.None);
            _tilemap.SetColor(cell, ResolveColor(cell));
            _highlightedCell = cell;
        }

        private Color ResolveColor(Vector3Int cell)
        {
            if (!_hexGridManager.HasGroundTile(cell))
            {
                return _noTileColor;
            }

            return _hexGridManager.IsOccupied(cell) ? _occupiedColor : _freeColor;
        }

        private void ClearHighlight()
        {
            if (_highlightedCell.HasValue)
            {
                _tilemap.SetTile(_highlightedCell.Value, null);
                _highlightedCell = null;
            }
        }
    }
}
