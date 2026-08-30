using Game.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI
{
    /// <summary>
    /// Escape or the HUD pause button toggles a pause indicator and
    /// freezes/unfreezes Time.timeScale. The indicator doesn't block clicks
    /// to the hotbar/game view (its background has raycastTarget off) - like
    /// the between-wave build phase, pausing is meant to let the player
    /// freely place and upgrade buildings, not lock them out behind a menu.
    /// Ignored once the game is already over (that has its own freeze).
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        public static PauseController Instance { get; private set; }

        [SerializeField] private GameObject _pausePanel;

        private bool _isPaused;

        public bool IsPaused => _isPaused;

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (GameOverController.IsGameOver || WaveChoiceUI.IsBuildPhaseActive || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetPaused(!_isPaused);
            }
        }

        /// <summary>Toggles pause from the HUD button; ignored under the same conditions Escape is.</summary>
        public void TogglePause()
        {
            if (GameOverController.IsGameOver || WaveChoiceUI.IsBuildPhaseActive)
            {
                return;
            }

            SetPaused(!_isPaused);
        }

        private void SetPaused(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = paused ? 0f : (GameSpeedController.Instance != null ? GameSpeedController.Instance.CurrentSpeed : 1f);
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(paused);
            }
        }
    }
}
