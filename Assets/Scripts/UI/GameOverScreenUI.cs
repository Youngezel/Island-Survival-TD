using Game.Systems;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Shows the run's result once GameOverController resolves the XP award,
    /// with a button to restart by reloading the scene. A real main menu
    /// (with persisted upgrades) arrives with the save system (§7.11).
    /// </summary>
    public class GameOverScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Text _resultText;
        [SerializeField] private Button _restartButton;

        private void OnEnable()
        {
            GameOverController.OnGameOverResolved += HandleGameOverResolved;
            _restartButton.onClick.AddListener(Restart);
        }

        private void OnDisable()
        {
            GameOverController.OnGameOverResolved -= HandleGameOverResolved;
            _restartButton.onClick.RemoveListener(Restart);
        }

        private void HandleGameOverResolved(int xpAwarded, int wavesSurvived)
        {
            if (_resultText != null)
            {
                _resultText.text = $"Game Over\nWave {wavesSurvived} bereikt\nXP verdiend: {xpAwarded}";
            }

            _panel.SetActive(true);
        }

        private void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
