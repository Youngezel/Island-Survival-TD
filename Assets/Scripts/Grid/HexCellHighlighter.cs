using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.Grid
{
    /// <summary>
    /// Follows the mouse in Play mode and highlights the hex cell currently
    /// under the cursor, colored by its occupied/free/no-tile state.
    /// Used to validate that HexGridManager correctly resolves world -> cell.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class HexCellHighlighter : MonoBehaviour
    {
        [SerializeField] private HexGridManager _hexGridManager;
        [SerializeField] private Camera _camera;
        [SerializeField] private Color _freeColor = new Color(0.51f, 0.76f, 0.29f, 0.6f);
        [SerializeField] private Color _occupiedColor = new Color(0.78f, 0.25f, 0.18f, 0.6f);
        [SerializeField] private Color _noTileColor = new Color(0.35f, 0.35f, 0.35f, 0.35f);

        private SpriteRenderer _spriteRenderer;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
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
            var mouseWorld = _camera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_camera.transform.position.z));
            mouseWorld.z = 0f;
            var cell = _hexGridManager.WorldToCell(mouseWorld);

            transform.position = _hexGridManager.CellToWorld(cell);

            if (!_hexGridManager.HasGroundTile(cell))
            {
                _spriteRenderer.color = _noTileColor;
            }
            else if (_hexGridManager.IsOccupied(cell))
            {
                _spriteRenderer.color = _occupiedColor;
            }
            else
            {
                _spriteRenderer.color = _freeColor;
            }
        }
    }
}
