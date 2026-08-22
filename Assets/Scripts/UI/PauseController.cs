using Game.Systems;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Game.UI
{
    /// <summary>
    /// Escape toggles a pause panel and freezes/unfreezes Time.timeScale.
    /// Ignored once the game is already over (that has its own freeze).
    /// </summary>
    public class PauseController : MonoBehaviour
    {
        [SerializeField] private GameObject _pausePanel;

        private bool _isPaused;

        private void Update()
        {
            if (GameOverController.IsGameOver || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                SetPaused(!_isPaused);
            }
        }

        private void SetPaused(bool paused)
        {
            _isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
            if (_pausePanel != null)
            {
                _pausePanel.SetActive(paused);
            }
        }
    }
}
