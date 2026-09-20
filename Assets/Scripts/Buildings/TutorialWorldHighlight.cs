using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// A single persistent outline TutorialController points at a specific
    /// world thing - a hex tile or a placed turret - so it lines up exactly
    /// with that thing's real shape instead of a generic square. Draws
    /// either the tile's actual flat-top hexagon (6 vertices, same geometry
    /// HexGridManager uses to place tiles), or a building's own
    /// PolygonCollider2D - the exact shape you must click inside of, not an
    /// approximation of it.
    /// World-space + LineRenderer, same proven setup as RangeIndicator, so
    /// it renders correctly regardless of any UI canvas/camera math - it
    /// only needs a world position, no screen-space conversion at all.
    /// </summary>
    public class TutorialWorldHighlight : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        private const int HexVertexCount = 6;

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

        /// <summary>Outlines a building's real clickable shape by tracing its own PolygonCollider2D points straight into world space - whatever you have to click inside of, exactly.</summary>
        public void ShowPolygon(PolygonCollider2D collider)
        {
            if (_lineRenderer == null || collider == null || collider.pathCount == 0)
            {
                return;
            }

            gameObject.SetActive(true);
            Vector2[] path = collider.GetPath(0);
            _lineRenderer.positionCount = path.Length;

            for (int i = 0; i < path.Length; i++)
            {
                Vector3 localPoint = (Vector2)collider.offset + path[i];
                _lineRenderer.SetPosition(i, collider.transform.TransformPoint(localPoint));
            }
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
