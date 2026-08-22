using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.CameraControl
{
    /// <summary>
    /// Pans a target transform (followed by a Cinemachine camera) while the
    /// left mouse button is held and dragged. Uses raw screen-pixel mouse
    /// delta converted to world units via the camera's orthographic size,
    /// so panning stays stable even though moving the target also moves the
    /// camera that the conversion would otherwise depend on.
    /// Map bounds are enforced separately by a CinemachineConfiner2D on the
    /// camera, not by this script.
    /// </summary>
    public class CameraPanController : MonoBehaviour
    {
        [SerializeField] private Transform _panTarget;
        [SerializeField] private Camera _camera;

        private bool _isDragging;

        private void Awake()
        {
            if (_camera == null)
            {
                _camera = Camera.main;
            }
        }

        private void Update()
        {
            var mouse = Mouse.current;
            if (mouse == null || _panTarget == null || _camera == null)
            {
                return;
            }

            if (mouse.leftButton.wasPressedThisFrame)
            {
                _isDragging = true;
            }
            else if (mouse.leftButton.wasReleasedThisFrame)
            {
                _isDragging = false;
            }

            if (!_isDragging)
            {
                return;
            }

            Vector2 screenDelta = mouse.delta.ReadValue();
            if (screenDelta == Vector2.zero || Screen.height == 0)
            {
                return;
            }

            float worldUnitsPerPixel = (_camera.orthographicSize * 2f) / Screen.height;
            Vector3 worldDelta = new Vector3(screenDelta.x, screenDelta.y, 0f) * worldUnitsPerPixel;
            _panTarget.position -= worldDelta;
        }
    }
}
