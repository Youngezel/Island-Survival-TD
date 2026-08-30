using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Game.CameraControl
{
    /// <summary>
    /// Zooms by adjusting the active CinemachineCamera's lens orthographic
    /// size directly, rather than Camera.orthographicSize - CinemachineBrain
    /// overwrites the Camera's own value from the virtual camera's lens
    /// every frame, so changing it there would just get reverted.
    /// </summary>
    public class CameraZoomController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _virtualCamera;
        [SerializeField] private float _zoomStep = 0.5f;
        [SerializeField] private float _minOrthographicSize = 3f;
        [SerializeField] private float _maxOrthographicSize = 16f;

        private void Update()
        {
            if (_virtualCamera == null || Mouse.current == null)
            {
                return;
            }

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            float scroll = Mouse.current.scroll.ReadValue().y;
            if (Mathf.Approximately(scroll, 0f))
            {
                return;
            }

            LensSettings lens = _virtualCamera.Lens;
            lens.OrthographicSize = Mathf.Clamp(lens.OrthographicSize - Mathf.Sign(scroll) * _zoomStep, _minOrthographicSize, _maxOrthographicSize);
            _virtualCamera.Lens = lens;
        }
    }
}
