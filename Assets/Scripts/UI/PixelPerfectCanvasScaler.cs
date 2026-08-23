using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Keeps the Canvas Scaler on Constant Pixel Size with an integer
    /// multiple of the native 640x360 resolution, per the visual identity
    /// spec: any non-integer scale factor would blur the pixel-art UI.
    /// </summary>
    [RequireComponent(typeof(CanvasScaler))]
    public class PixelPerfectCanvasScaler : MonoBehaviour
    {
        private const float NativeHeight = 360f;

        private CanvasScaler _scaler;
        private int _lastAppliedScreenHeight = -1;

        private void Awake()
        {
            _scaler = GetComponent<CanvasScaler>();
            _scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
        }

        private void Update()
        {
            if (Screen.height == _lastAppliedScreenHeight)
            {
                return;
            }

            _lastAppliedScreenHeight = Screen.height;
            _scaler.scaleFactor = Mathf.Max(1, Mathf.FloorToInt(Screen.height / NativeHeight));
        }
    }
}
