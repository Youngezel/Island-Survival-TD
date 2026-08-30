using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Tracks the player's chosen game speed (1x/2x, cycled from a HUD
    /// button) and is the single source of truth for what Time.timeScale
    /// should be whenever the game isn't paused - both PauseController
    /// (Escape) and WaveChoiceUI (the between-wave build phase) resume to
    /// CurrentSpeed instead of hardcoding 1, so toggling speed doesn't get
    /// silently reset by pausing/unpausing.
    /// </summary>
    public class GameSpeedController : MonoBehaviour
    {
        public static GameSpeedController Instance { get; private set; }

        [SerializeField] private float[] _speedSteps = { 1f, 2f };

        private int _speedIndex;

        public float CurrentSpeed => _speedSteps[_speedIndex];

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>Cycles to the next speed step; applies immediately unless the game is currently paused (Time.timeScale == 0).</summary>
        public void CycleSpeed()
        {
            _speedIndex = (_speedIndex + 1) % _speedSteps.Length;
            if (Time.timeScale > 0f)
            {
                Time.timeScale = CurrentSpeed;
            }
        }
    }
}
