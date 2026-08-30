using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// A short-lived radial burst spawned wherever a projectile hits: a
    /// small fixed-size spark for the turret and long-range turret, or
    /// scaled up to match the mortar's actual splash radius so the visual
    /// footprint lines up with which enemies actually took damage. Grows in
    /// quickly, then fades out and destroys itself.
    /// </summary>
    public class ImpactEffect : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private float _growDuration = 0.08f;
        [SerializeField] private float _holdDuration = 0.03f;
        [SerializeField] private float _fadeDuration = 0.15f;

        // The sprite is a 32x32 circle at 32 PPU, so at scale 1 it's 1 world
        // unit across - a radius of 0.5.
        private const float NativeRadius = 0.5f;

        private float _targetScale = 1f;

        /// <summary>Call before the effect starts playing to size it to a world-space radius (e.g. the mortar's splash radius) instead of the small default.</summary>
        public void SetRadius(float worldRadius)
        {
            _targetScale = Mathf.Max(0.1f, worldRadius / NativeRadius);
        }

        private void Start()
        {
            StartCoroutine(PlayEffect());
        }

        private IEnumerator PlayEffect()
        {
            transform.localScale = Vector3.zero;

            float t = 0f;
            while (t < _growDuration)
            {
                t += Time.deltaTime;
                float s = Mathf.Lerp(0f, _targetScale, t / _growDuration);
                transform.localScale = new Vector3(s, s, 1f);
                yield return null;
            }

            transform.localScale = new Vector3(_targetScale, _targetScale, 1f);

            if (_holdDuration > 0f)
            {
                yield return new WaitForSeconds(_holdDuration);
            }

            Color startColor = _spriteRenderer != null ? _spriteRenderer.color : Color.white;
            t = 0f;
            while (t < _fadeDuration)
            {
                t += Time.deltaTime;
                if (_spriteRenderer != null)
                {
                    float a = Mathf.Lerp(1f, 0f, t / _fadeDuration);
                    _spriteRenderer.color = new Color(startColor.r, startColor.g, startColor.b, a);
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
