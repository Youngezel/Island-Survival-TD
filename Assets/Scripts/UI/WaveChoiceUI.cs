using Game.Data;
using Game.Economy;
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
    /// a free hex tile, then resume with the same HUD pause button used
    /// during combat - there's no dedicated "start next wave" button, since
    /// resuming and starting the next wave are the same action here.
    /// </summary>
    public class WaveChoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _waveSurvivedText;
        [SerializeField] private Button _coinsButton;
        [SerializeField] private Button _tileButton;
        [SerializeField] private HotbarItemData _freeTileItem;
        [SerializeField] private int _coinBonus = 15;

        private bool _hasChosenReward;
        private bool _waitingToStartNextWave;

        private void OnEnable()
        {
            WaveManager.OnWaveCleared += HandleWaveCleared;
            _coinsButton.onClick.AddListener(ChooseCoins);
            _tileButton.onClick.AddListener(ChooseTile);
            PauseController.OnPauseChanged += HandlePauseChanged;
        }

        private void OnDisable()
        {
            WaveManager.OnWaveCleared -= HandleWaveCleared;
            _coinsButton.onClick.RemoveListener(ChooseCoins);
            _tileButton.onClick.RemoveListener(ChooseTile);
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
        }

        /// <summary>When the shared pause state resumes while a wave was waiting to start, that resume is what kicks off the next wave.</summary>
        private void HandlePauseChanged(bool paused)
        {
            if (paused || !_waitingToStartNextWave)
            {
                return;
            }

            _waitingToStartNextWave = false;

            // Resumed before picking a reward (e.g. via the pause button) - default to the coin bonus rather than losing it.
            if (!_hasChosenReward)
            {
                CoinWallet.Instance.AddCoins(_coinBonus);
            }

            _panel.SetActive(false);
            WaveManager.Instance.StartNextWave();
        }
    }
}
