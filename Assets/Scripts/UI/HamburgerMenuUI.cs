using Game.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// The hamburger menu button (or Escape) opens a fully blocking panel
    /// with a setting and a way to quit to the main menu - unlike the
    /// between-wave build-phase pause, this locks out all other game
    /// interaction while open (its background is a normal raycast-blocking
    /// panel). Restores whatever pause state existed before opening when
    /// closed, so opening it mid build-phase doesn't accidentally resume
    /// the next wave.
    /// </summary>
    public class HamburgerMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _menuButton;
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _autoStartButton;
        [SerializeField] private TMP_Text _autoStartButtonText;
        [SerializeField] private Button _quitButton;
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        private bool _isOpen;
        private bool _wasPausedBeforeOpening;

        private void OnEnable()
        {
            _menuButton.onClick.AddListener(ToggleMenu);
            _closeButton.onClick.AddListener(CloseMenu);
            _autoStartButton.onClick.AddListener(ToggleAutoStart);
            _quitButton.onClick.AddListener(Quit);
        }

        private void OnDisable()
        {
            _menuButton.onClick.RemoveListener(ToggleMenu);
            _closeButton.onClick.RemoveListener(CloseMenu);
            _autoStartButton.onClick.RemoveListener(ToggleAutoStart);
            _quitButton.onClick.RemoveListener(Quit);
        }

        private void Start()
        {
            RefreshAutoStartLabel();
        }

        private void Update()
        {
            if (Game.Systems.GameOverController.IsGameOver || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                ToggleMenu();
            }
        }

        private void ToggleMenu()
        {
            if (_isOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        private void OpenMenu()
        {
            if (Game.Systems.GameOverController.IsGameOver || _isOpen || PauseController.Instance == null)
            {
                return;
            }

            _isOpen = true;
            _wasPausedBeforeOpening = PauseController.Instance.IsPaused;
            PauseController.Instance.SetPaused(true);
            _menuPanel.SetActive(true);
        }

        private void CloseMenu()
        {
            if (!_isOpen)
            {
                return;
            }

            _isOpen = false;
            _menuPanel.SetActive(false);
            PauseController.Instance.SetPaused(_wasPausedBeforeOpening);
        }

        private void ToggleAutoStart()
        {
            GameSettings.AutoStartNextWave = !GameSettings.AutoStartNextWave;
            RefreshAutoStartLabel();
        }

        private void RefreshAutoStartLabel()
        {
            if (_autoStartButtonText != null)
            {
                _autoStartButtonText.text = GameSettings.AutoStartNextWave
                    ? "VOLGENDE WAVE AUTOMATISCH STARTEN: AAN"
                    : "VOLGENDE WAVE AUTOMATISCH STARTEN: UIT";
            }
        }

        private void Quit()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_mainMenuSceneName);
        }
    }
}
