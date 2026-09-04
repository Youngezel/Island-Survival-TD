using System.Collections;
using TMPro;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// A world-space number that pops up wherever damage was just dealt,
    /// drifts upward, and fades out - spawned once per Health.TakeDamage
    /// call so every hit (a direct shot, a splash tick, a burn tick) gets
    /// its own readable number instead of only the total.
    /// </summary>
    public class DamageNumber : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private float _riseDistance = 0.6f;
        [SerializeField] private float _duration = 0.7f;
        [SerializeField] private float _randomXSpread = 0.15f;

        public void SetValue(int amount)
        {
            if (_text != null)
            {
                _text.text = amount.ToString();
            }
        }

        private void Start()
        {
            transform.position += new Vector3(Random.Range(-_randomXSpread, _randomXSpread), 0f, 0f);
            StartCoroutine(PlayAndDestroy());
        }

        private IEnumerator PlayAndDestroy()
        {
            Vector3 start = transform.position;
            Vector3 end = start + new Vector3(0f, _riseDistance, 0f);
            Color startColor = _text != null ? _text.color : Color.white;

            float t = 0f;
            while (t < _duration)
            {
                t += Time.deltaTime;
                float normalized = t / _duration;
                transform.position = Vector3.Lerp(start, end, normalized);

                if (_text != null)
                {
                    float alpha = Mathf.Lerp(1f, 0f, normalized);
                    _text.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
                }

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
