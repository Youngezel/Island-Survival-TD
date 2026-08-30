using System.Collections;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// A small looping flame shown on an enemy while it's Burning, alternating
    /// between two frames for a simple flicker. Purely visual - lifetime is
    /// controlled by whoever instantiates it (Burning destroys it via its own
    /// OnDestroy, so the flame never outlives the burn or the enemy it's on).
    /// </summary>
    public class FireVfx : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _spriteRenderer;
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _frameDuration = 0.12f;

        private void OnEnable()
        {
            if (_spriteRenderer != null && _frames != null && _frames.Length > 0)
            {
                StartCoroutine(Flicker());
            }
        }

        private IEnumerator Flicker()
        {
            int index = 0;
            while (true)
            {
                _spriteRenderer.sprite = _frames[index % _frames.Length];
                index++;
                yield return new WaitForSeconds(_frameDuration);
            }
        }
    }
}
