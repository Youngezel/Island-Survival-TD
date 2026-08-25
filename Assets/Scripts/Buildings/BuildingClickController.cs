using Game.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game.Buildings
{
    /// <summary>
    /// Detects clicks on placed buildings and opens the inspector for them.
    /// Uses a manual Physics2D query driven by the new Input System rather
    /// than Unity's legacy OnMouseDown message - this project's Active Input
    /// Handling is set to "Input System Package (New)" only, and OnMouseDown
    /// is implemented on top of the old Input Manager under the hood, so it
    /// silently never fires with that setting.
    /// </summary>
    public class BuildingClickController : MonoBehaviour
    {
        [SerializeField] private Camera _worldCamera;

        private void Awake()
        {
            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }
        }

        private void Update()
        {
            if (Mouse.current == null || _worldCamera == null || !Mouse.current.leftButton.wasPressedThisFrame)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (PlacementCursor.Instance != null && PlacementCursor.Instance.SelectedItem != null)
            {
                return;
            }

            Vector2 screenPosition = Mouse.current.position.ReadValue();
            Vector3 worldPosition = _worldCamera.ScreenToWorldPoint(new Vector3(screenPosition.x, screenPosition.y, -_worldCamera.transform.position.z));
            worldPosition.z = 0f;

            Collider2D hit = Physics2D.OverlapPoint(worldPosition);
            if (hit == null)
            {
                return;
            }

            Building building = hit.GetComponent<Building>();
            if (building == null || building.Data == null || string.IsNullOrEmpty(building.Data.UpgradeSaveKey))
            {
                return;
            }

            BuildingInspectorUI.Instance?.Open(building.Data, building);
        }
    }
}
