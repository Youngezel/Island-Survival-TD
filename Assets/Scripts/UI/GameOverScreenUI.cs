using Game.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Shows the run's result once GameOverController resolves the XP award,
    /// with a button back to the main menu so the player can spend the XP
    /// they just earned on upgrades before the next run.
    /// </summary>
    public class GameOverScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _resultText;
        [SerializeField] private Button _restartButton;
        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        private void OnEnable()
        {
            GameOverController.OnGameOverResolved += HandleGameOverResolved;
            _restartButton.onClick.AddListener(BackToMainMenu);
        }

        private void OnDisable()
        {
            GameOverController.OnGameOverResolved -= HandleGameOverResolved;
            _restartButton.onClick.RemoveListener(BackToMainMenu);
        }

        private void HandleGameOverResolved(int xpAwarded, int wavesSurvived)
        {
            if (_resultText != null)
            {
                _resultText.text = $"Game Over\nWave {wavesSurvived} bereikt\nXP verdiend: {xpAwarded}";
            }

            _panel.SetActive(true);
        }

        private void BackToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_mainMenuSceneName);
        }
    }
}
