using System;
using Game.Systems;
using UnityEngine;

namespace Game.UI
{
    /// <summary>
    /// The single source of truth for whether the game is time-frozen.
    /// Only HamburgerMenuUI pauses this, while its fully-blocking menu is
    /// open - the between-wave screen (WaveChoiceUI) deliberately does NOT
    /// touch this, so water, projectiles and everything else keep animating
    /// while the player picks a reward or places/upgrades buildings.
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
