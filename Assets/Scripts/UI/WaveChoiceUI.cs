using Game.Data;
using Game.Economy;
using Game.Systems;
using Game.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Shown between waves: forces the game paused via PauseController (UI
    /// input still works while paused) so the player can freely place and
    /// upgrade buildings without time pressure. Pick either a coin bonus or
    /// a free hex tile, then either the next wave starts on its own
    /// (GameSettings.AutoStartNextWave) or the resume button becomes usable
    /// - that button only ever resumes the wave-clear pause, it can't pause
    /// the game mid-wave (that's what the hamburger menu is for).
    /// </summary>
    public class WaveChoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _waveSurvivedText;
        [SerializeField] private Button _coinsButton;
        [SerializeField] private Button _tileButton;
        [SerializeField] private Button _resumeButton;
        [SerializeField] private HotbarItemData _freeTileItem;
        [SerializeField] private int _coinBonus = 15;

        private bool _hasChosenReward;
        private bool _waitingToStartNextWave;

        private void OnEnable()
        {
            WaveManager.OnWaveCleared += HandleWaveCleared;
            _coinsButton.onClick.AddListener(ChooseCoins);
            _tileButton.onClick.AddListener(ChooseTile);
            _resumeButton.onClick.AddListener(HandleResumeButtonClicked);
            PauseController.OnPauseChanged += HandlePauseChanged;
        }

        private void OnDisable()
        {
            WaveManager.OnWaveCleared -= HandleWaveCleared;
            _coinsButton.onClick.RemoveListener(ChooseCoins);
            _tileButton.onClick.RemoveListener(ChooseTile);
            _resumeButton.onClick.RemoveListener(HandleResumeButtonClicked);
            PauseController.OnPauseChanged -= HandlePauseChanged;
        }

        private void HandleWaveCleared(int waveNumber)
        {
            if (_waveSurvivedText != null)
            {
                _waveSurvivedText.text = $"WAVE {waveNumber} OVERLEEFD";
            }

            _hasChosenReward = false;
            _waitingToStartNextWave = true;
            _coinsButton.interactable = true;
            _tileButton.interactable = true;
            _resumeButton.interactable = true;

            _panel.SetActive(true);
            PauseController.Instance.SetPaused(true);
        }

        private void ChooseCoins()
        {
            if (_hasChosenReward)
            {
                return;
            }

            _hasChosenReward = true;
            CoinWallet.Instance.AddCoins(_coinBonus);
            _panel.SetActive(false);
            MaybeAutoStart();
        }

        private void ChooseTile()
        {
            if (_hasChosenReward)
            {
                return;
            }

            _hasChosenReward = true;
            PlacementCursor.Instance.SelectItem(_freeTileItem, free: true);
            _panel.SetActive(false);
            MaybeAutoStart();
        }

        private void MaybeAutoStart()
        {
            if (!GameSettings.AutoStartNextWave || !_waitingToStartNextWave)
            {
                return;
            }

            _waitingToStartNextWave = false;
            StartNextWave();
            PauseController.Instance.SetPaused(false);
        }

        /// <summary>The resume button only ever un-pauses the wave-clear pause - a no-op the rest of the time, since it must never pause an active wave.</summary>
        private void HandleResumeButtonClicked()
        {
            if (!_waitingToStartNextWave)
            {
                return;
            }

            PauseController.Instance.SetPaused(false);
        }

        /// <summary>Reacts to any resume, including one triggered elsewhere (e.g. the hamburger menu closing) while a wave was waiting to start.</summary>
        private void HandlePauseChanged(bool paused)
        {
            if (paused || !_waitingToStartNextWave)
            {
                return;
            }

            _waitingToStartNextWave = false;
            StartNextWave();
        }

        /// <summary>Grants the fallback reward if none was picked, closes the panel, and starts the next wave. Does not touch pause state - callers already have.</summary>
        private void StartNextWave()
        {
            _resumeButton.interactable = false;

            if (!_hasChosenReward)
            {
                CoinWallet.Instance.AddCoins(_coinBonus);
            }

            _panel.SetActive(false);
            WaveManager.Instance.StartNextWave();
        }
    }
}
