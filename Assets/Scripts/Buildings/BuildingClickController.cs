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

        // Reused every click to avoid allocating a new array each frame.
        private readonly Collider2D[] _overlapResults = new Collider2D[16];

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

            // OverlapPoint would return just one collider, in no guaranteed
            // order - MapBounds (a map-wide trigger used by the camera
            // confiner) overlaps every building on the map, and either one
            // being a trigger means it can "win" over the actual building
            // underneath the click, silently swallowing it. Check every
            // collider at this point instead and pick the one that's a
            // building.
            int count = Physics2D.OverlapPoint(worldPosition, ContactFilter2D.noFilter, _overlapResults);
            Building building = null;
            for (int i = 0; i < count; i++)
            {
                building = _overlapResults[i].GetComponent<Building>();
                if (building != null)
                {
                    break;
                }
            }

            if (building == null || building.Data == null || string.IsNullOrEmpty(building.Data.UpgradeSaveKey))
            {
                return;
            }

            BuildingInspectorUI.Instance?.Open(building.Data, building);
        }
    }
}
