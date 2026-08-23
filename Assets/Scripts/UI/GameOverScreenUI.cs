using Game.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Shows the run's result once GameOverController resolves the XP award:
    /// wave reached, pirates sunk, XP earned and the best wave so far, with
    /// buttons back to the main menu or straight into another run.
    /// </summary>
    public class GameOverScreenUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _waveReachedValue;
        [SerializeField] private TMP_Text _piratesSunkValue;
        [SerializeField] private TMP_Text _xpEarnedValue;
        [SerializeField] private TMP_Text _bestRunText;
        [SerializeField] private Button _mainMenuButton;
        [SerializeField] private Button _anotherRoundButton;
        [SerializeField] private string _mainMenuSceneName = "MainMenu";
        [SerializeField] private string _gameSceneName = "SampleScene";

        private void OnEnable()
        {
            GameOverController.OnGameOverResolved += HandleGameOverResolved;
            _mainMenuButton.onClick.AddListener(BackToMainMenu);
            _anotherRoundButton.onClick.AddListener(AnotherRound);
        }

        private void OnDisable()
        {
            GameOverController.OnGameOverResolved -= HandleGameOverResolved;
            _mainMenuButton.onClick.RemoveListener(BackToMainMenu);
            _anotherRoundButton.onClick.RemoveListener(AnotherRound);
        }

        private void HandleGameOverResolved(int xpAwarded, int wavesSurvived)
        {
            if (_waveReachedValue != null)
            {
                _waveReachedValue.text = wavesSurvived.ToString();
            }

            if (_piratesSunkValue != null)
            {
                _piratesSunkValue.text = KillTracker.Instance != null ? KillTracker.Instance.Kills.ToString() : "0";
            }

            if (_xpEarnedValue != null)
            {
                _xpEarnedValue.text = $"+{xpAwarded}";
            }

            if (_bestRunText != null && SaveManager.Instance != null)
            {
                _bestRunText.text = $"Beste run: wave {SaveManager.Instance.Current.bestWave}";
            }

            _panel.SetActive(true);
        }

        private void BackToMainMenu()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_mainMenuSceneName);
        }

        private void AnotherRound()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(_gameSceneName);
        }
    }
}
