using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// A single persistent circle outline, shown around whichever turret's
    /// inspector is currently open so the player can see its actual reach.
    /// One shared instance repositioned/resized on demand rather than
    /// spawned per turret, since only one inspector can be open at a time.
    /// </summary>
    public class RangeIndicator : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        private const int Segments = 48;

        public void Show(Vector3 worldCenter, float worldRadius)
        {
            if (_lineRenderer == null)
            {
                return;
            }

            gameObject.SetActive(true);
            _lineRenderer.positionCount = Segments;

            for (int i = 0; i < Segments; i++)
            {
                float angle = i / (float)Segments * Mathf.PI * 2f;
                Vector3 point = worldCenter + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * worldRadius;
                _lineRenderer.SetPosition(i, point);
            }
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }
    }
}
