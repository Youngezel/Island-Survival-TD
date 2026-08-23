using Game.Data;
using Game.Grid;
using Game.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Tracks the hotbar item selected via HotbarSlot and places it on the
    /// next map click - click to select, click to place, rather than a
    /// press-and-drag, since holding the left mouse button also pans the
    /// camera (CameraPanController). A ghost icon follows the cursor while
    /// something is selected; right-click cancels the selection.
    /// </summary>
    public class PlacementCursor : MonoBehaviour
    {
        public static PlacementCursor Instance { get; private set; }

        [SerializeField] private Image _ghostIcon;
        [SerializeField] private Camera _worldCamera;

        public HotbarItemData SelectedItem => _selectedItem;

        private HotbarItemData _selectedItem;
        private bool _selectionIsFree;

        private void Awake()
        {
            Instance = this;
            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }
        }

        public void SelectItem(HotbarItemData item, bool free = false)
        {
            _selectedItem = item;
            _selectionIsFree = free;
            if (_ghostIcon != null)
            {
                _ghostIcon.sprite = item != null ? item.Icon : null;
                _ghostIcon.gameObject.SetActive(item != null);
            }
        }

        private void Update()
        {
            if (_selectedItem == null || Mouse.current == null || HexGridManager.Instance == null || BuildPlacer.Instance == null)
            {
                return;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            if (_ghostIcon != null)
            {
                _ghostIcon.transform.position = screenPosition;
            }

            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                SelectItem(null);
                return;
            }

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (Mouse.current.leftButton.wasPressedThisFrame && !overUI)
            {
                Vector3Int cell = HexGridManager.Instance.ScreenToCell(screenPosition, _worldCamera);
                if (BuildPlacer.Instance.TryPlace(_selectedItem, cell, _selectionIsFree))
                {
                    SelectItem(null);
                }
            }
        }
    }
}
