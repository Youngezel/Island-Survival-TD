using System;
using Game.Systems;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// The single source of truth for whether the game is time-frozen:
    /// WaveChoiceUI pauses this between waves (still lets the player build,
    /// via the hotbar/inspector which don't depend on Time.timeScale), and
    /// HamburgerMenuUI pauses this while its fully-blocking menu is open.
    /// Neither owns "pause" exclusively - SetPaused just reflects the
    /// current desired state, and OnPauseChanged lets each side react.
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }

        /// <summary>Fired whenever the paused state changes, from any source.</summary>
        public static event Action<bool> OnPauseChanged;

        public bool IsPaused { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void SetPaused(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : (GameSpeedController.Instance != null ? GameSpeedController.Instance.CurrentSpeed : 1f);
            OnPauseChanged?.Invoke(paused);
        }
    }
}
