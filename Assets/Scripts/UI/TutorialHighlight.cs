using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// A pulsing gold border box that TutorialController points at whatever
    /// the player needs to interact with next - a hotbar slot, a specific
    /// hex tile, or a placed turret - so a guided step reads as "click/drag
    /// exactly here" instead of just a text description. Tracks its target
    /// every frame (world position converted through the camera, or a UI
    /// RectTransform's own corners) so it stays glued on target even while
    /// the camera pans or a UI layout shifts. Hidden (and untracking)
    /// whenever nothing is set - starts inactive in the scene itself, so
    /// Awake must NOT also call SetActive(false): Unity only invokes Awake
    /// the first time an initially-inactive object is activated, and it
    /// runs synchronously as part of that very activation - so a
    /// SetActive(false) here would immediately undo the first-ever
    /// TrackUI()/TrackWorld() call that turned it on, leaving the highlight
    /// permanently stuck invisible from that point on.
    /// </summary>
    public class TutorialHighlight : MonoBehaviour
    {
        [SerializeField] private RectTransform _box;
        [SerializeField] private Image[] _borderEdges;
        [SerializeField] private Camera _worldCamera;

        private RectTransform _uiTarget;
        private Transform _worldTarget;
        private Vector2 _worldSize = new Vector2(48f, 48f);

        private void Awake()
        {
            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }
        }

        /// <summary>Points the highlight at a UI element (e.g. a hotbar slot) - tracks its live rect every frame.</summary>
        public void TrackUI(RectTransform target)
        {
            _uiTarget = target;
            _worldTarget = null;
            gameObject.SetActive(true);
        }

        /// <summary>Points the highlight at a world-space position (e.g. a hex tile or a placed turret) - sized in screen pixels at the current zoom.</summary>
        public void TrackWorld(Transform target, Vector2 size)
        {
            _worldTarget = target;
            _uiTarget = null;
            _worldSize = size;
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            _uiTarget = null;
            _worldTarget = null;
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_uiTarget != null)
            {
                _box.position = _uiTarget.position;
                _box.sizeDelta = _uiTarget.sizeDelta;
            }
            else if (_worldTarget != null && _worldCamera != null)
            {
                // Converts straight into the box's own parent's local space
                // (rather than requiring a separate Canvas reference) so
                // this stays correct no matter where the box is parented -
                // null camera is correct here since this project's Canvas
                // is Screen Space Overlay.
                Vector3 screenPoint = _worldCamera.WorldToScreenPoint(_worldTarget.position);
                RectTransform parentRect = _box.parent as RectTransform;
                if (parentRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, screenPoint, null, out Vector2 localPoint))
                {
                    _box.anchoredPosition = localPoint;
                }

                _box.sizeDelta = _worldSize;
            }
            else
            {
                return;
            }

            float pulse = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 4.5f);
            Color color = new Color(1f, 0.812f, 0.247f, pulse); // UITheme.Gold at a pulsing alpha
            foreach (Image edge in _borderEdges)
            {
                if (edge != null)
                {
                    edge.color = color;
                }
            }
        }
    }
}
