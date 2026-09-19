using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// A single persistent outline TutorialController points at a specific
    /// world thing - a hex tile or a placed turret - so it lines up exactly
    /// with that thing's real shape instead of a generic square. Draws
    /// either the tile's actual flat-top hexagon (6 vertices, same geometry
    /// HexGridManager uses to place tiles) or a square around a turret.
    /// World-space + LineRenderer, same proven setup as RangeIndicator, so
    /// it renders correctly regardless of any UI canvas/camera math - it
    /// only needs a world position, no screen-space conversion at all.
    /// </summary>
    public class TutorialWorldHighlight : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        private const int HexVertexCount = 6;
        private const int BoxVertexCount = 4;

        /// <summary>Outlines the actual hexagon at this world center - radius is the tile's own center-to-vertex distance (see HexGridManager.HexStepWorldDistance).</summary>
        public void ShowHex(Vector3 worldCenter, float vertexRadius)
        {
            if (_lineRenderer == null)
            {
                return;
            }

            gameObject.SetActive(true);
            _lineRenderer.positionCount = HexVertexCount;

            for (int i = 0; i < HexVertexCount; i++)
            {
                float angle = i * 60f * Mathf.Deg2Rad;
                Vector3 point = worldCenter + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * vertexRadius;
                _lineRenderer.SetPosition(i, point);
            }
        }

        /// <summary>Outlines a square around this world center, sized to the given world-unit width/height.</summary>
        public void ShowBox(Vector3 worldCenter, Vector2 size)
        {
            if (_lineRenderer == null)
            {
                return;
            }

            gameObject.SetActive(true);
            _lineRenderer.positionCount = BoxVertexCount;

            Vector2 half = size * 0.5f;
            _lineRenderer.SetPosition(0, worldCenter + new Vector3(-half.x, -half.y, 0f));
            _lineRenderer.SetPosition(1, worldCenter + new Vector3(half.x, -half.y, 0f));
            _lineRenderer.SetPosition(2, worldCenter + new Vector3(half.x, half.y, 0f));
            _lineRenderer.SetPosition(3, worldCenter + new Vector3(-half.x, half.y, 0f));
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        private void Update()
        {
            if (_lineRenderer == null || !gameObject.activeSelf)
            {
                return;
            }

            float pulse = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 4.5f);
            Color color = new Color(1f, 0.812f, 0.247f, pulse); // UITheme.Gold at a pulsing alpha
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
        }
    }
}
