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
    /// every frame, so changing it there would just get reverted. Each
    /// scroll tick nudges a target size, and the lens eases toward that
    /// target every frame instead of jumping straight to it, so zooming
    /// reads as one smooth motion rather than a series of steps.
    /// </summary>
    public class CameraZoomController : MonoBehaviour
    {
        [SerializeField] private CinemachineCamera _virtualCamera;
        [SerializeField] private float _zoomStep = 0.5f;
        [SerializeField] private float _minOrthographicSize = 3f;
        [SerializeField] private float _maxOrthographicSize = 16f;
        [SerializeField] private float _smoothTime = 0.15f;

        private float _targetOrthographicSize;
        private float _zoomVelocity;
        private bool _initialized;

        private void Update()
        {
            if (_virtualCamera == null)
            {
                return;
            }

            if (!_initialized)
            {
                _targetOrthographicSize = _virtualCamera.Lens.OrthographicSize;
                _initialized = true;
            }

            bool overUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();
            if (Mouse.current != null && !overUI)
            {
                float scroll = Mouse.current.scroll.ReadValue().y;
                if (!Mathf.Approximately(scroll, 0f))
                {
                    _targetOrthographicSize = Mathf.Clamp(_targetOrthographicSize - Mathf.Sign(scroll) * _zoomStep, _minOrthographicSize, _maxOrthographicSize);
                }
            }

            LensSettings lens = _virtualCamera.Lens;
            lens.OrthographicSize = Mathf.SmoothDamp(lens.OrthographicSize, _targetOrthographicSize, ref _zoomVelocity, _smoothTime);
            _virtualCamera.Lens = lens;
        }
    }
}
