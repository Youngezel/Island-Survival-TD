using System;
using Game.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI
{
    /// <summary>
    /// Escape or the HUD pause button freezes/unfreezes Time.timeScale.
    /// There's no on-screen "paused" indicator - the HUD button's own label
    /// (PAUZE/HERVAT) is the only sign. Pausing never blocks the hotbar or
    /// building UI, so placing/upgrading works the same whether paused
    /// manually or by WaveChoiceUI between waves, which also drives this
    /// same paused state (see SetPaused/OnPauseChanged) rather than
    /// managing Time.timeScale on its own. Ignored once the game is
    /// already over (that has its own freeze).
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }

        /// <summary>Fired whenever the paused state changes, from any source (Escape, the HUD button, or WaveChoiceUI).</summary>
        public static event Action<bool> OnPauseChanged;

        private bool _isPaused;

        public bool IsPaused => _isPaused;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (GameOverController.IsGameOver || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                TogglePause();
            }
        }

        /// <summary>Toggles pause, e.g. from the HUD button; ignored once the game is over.</summary>
        public void TogglePause()
        {
            if (GameOverController.IsGameOver)
            {
                return;
            }

            SetPaused(!_isPaused);
        }

        /// <summary>Pauses or resumes directly - used by WaveChoiceUI to force a pause on wave-clear regardless of the current state.</summary>
        public void SetPaused(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = paused ? 0f : (GameSpeedController.Instance != null ? GameSpeedController.Instance.CurrentSpeed : 1f);
            OnPauseChanged?.Invoke(paused);
        }
    }
}
