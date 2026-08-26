using UnityEngine;

namespace Game.VFX
{
    /// <summary>
    /// Loops a SpriteRenderer through a fixed sequence of sprites at a
    /// steady frame rate - a lightweight stand-in for a full Animator
    /// Controller for simple ambient loops like a boat bobbing on waves.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class SpriteFlipbook : MonoBehaviour
    {
        [SerializeField] private Sprite[] _frames;
        [SerializeField] private float _framesPerSecond = 6f;
        [SerializeField] private bool _randomStartOffset = true;

        private SpriteRenderer _renderer;
        private float _time;

        private void Awake()
        {
            _renderer = GetComponent<SpriteRenderer>();
            if (_randomStartOffset && _frames != null && _frames.Length > 0)
            {
                _time = Random.Range(0f, _frames.Length / _framesPerSecond);
            }
        }

        private void Update()
        {
            if (_frames == null || _frames.Length == 0)
            {
                return;
            }

            _time += Time.deltaTime;
            int index = Mathf.FloorToInt(_time * _framesPerSecond) % _frames.Length;
            _renderer.sprite = _frames[index];
        }
    }
}
