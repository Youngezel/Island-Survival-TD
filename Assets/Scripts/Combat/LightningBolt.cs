using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// A brief jagged line between two points - the Tesla Coil's shot is a
    /// lightning bolt rather than a traveling projectile, so this is what
    /// actually reads as "the shot": one segment from the turret to its
    /// target, then one more per chain-lightning hop. Flashes on instantly
    /// and fades out fast, since real arcing electricity doesn't fly there
    /// over time the way a cannonball does.
    /// </summary>
    public class LightningBolt : MonoBehaviour
    {
        [SerializeField] private LineRenderer _lineRenderer;
        [SerializeField] private int _segments = 5;
        [SerializeField] private float _jitter = 0.18f;
        [SerializeField] private float _duration = 0.12f;

        public void Play(Vector3 from, Vector3 to)
        {
            if (_lineRenderer == null)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 direction = to - from;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f).normalized;

            _lineRenderer.positionCount = _segments + 1;
            for (int i = 0; i <= _segments; i++)
            {
                float t = i / (float)_segments;
                Vector3 point = Vector3.Lerp(from, to, t);
                if (i > 0 && i < _segments)
                {
                    point += perpendicular * Random.Range(-_jitter, _jitter);
                }

                _lineRenderer.SetPosition(i, point);
            }

            StartCoroutine(FadeAndDestroy());
        }

        private IEnumerator FadeAndDestroy()
        {
            Color startColor = _lineRenderer.startColor;
            float t = 0f;
            while (t < _duration)
            {
                t += Time.deltaTime;
                Color c = startColor;
                c.a = Mathf.Lerp(startColor.a, 0f, t / _duration);
                _lineRenderer.startColor = c;
                _lineRenderer.endColor = c;
                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
