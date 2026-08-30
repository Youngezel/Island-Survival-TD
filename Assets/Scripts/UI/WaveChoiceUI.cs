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
    /// Shown between waves: pauses the game (Time.timeScale = 0) so the
    /// player can freely place and upgrade buildings without time pressure -
    /// UI input still works while paused. The player picks either a coin
    /// bonus or a free hex tile, then presses "start next wave" whenever
    /// ready to resume (at whatever speed GameSpeedController is set to).
    /// </summary>
    public class WaveChoiceUI : MonoBehaviour
    {
        /// <summary>True while the build-phase panel is open, so PauseController doesn't also try to pause/unpause on top of it.</summary>
        public static bool IsBuildPhaseActive { get; private set; }

        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _waveSurvivedText;
        [SerializeField] private Button _coinsButton;
        [SerializeField] private Button _tileButton;
        [SerializeField] private Button _startNextWaveButton;
        [SerializeField] private HotbarItemData _freeTileItem;
        [SerializeField] private int _coinBonus = 15;

        private bool _hasChosenReward;

        private void OnEnable()
        {
            WaveManager.OnWaveCleared += HandleWaveCleared;
            _coinsButton.onClick.AddListener(ChooseCoins);
            _tileButton.onClick.AddListener(ChooseTile);
            _startNextWaveButton.onClick.AddListener(StartNextWave);
        }

        private void OnDisable()
        {
            WaveManager.OnWaveCleared -= HandleWaveCleared;
            _coinsButton.onClick.RemoveListener(ChooseCoins);
            _tileButton.onClick.RemoveListener(ChooseTile);
            _startNextWaveButton.onClick.RemoveListener(StartNextWave);
        }

        private void HandleWaveCleared(int waveNumber)
        {
            if (_waveSurvivedText != null)
            {
                _waveSurvivedText.text = $"WAVE {waveNumber} OVERLEEFD";
            }

            _hasChosenReward = false;
            _coinsButton.interactable = true;
            _tileButton.interactable = true;
            _startNextWaveButton.interactable = false;

            IsBuildPhaseActive = true;
            Time.timeScale = 0f;
            _panel.SetActive(true);
        }

        private void ChooseCoins()
        {
            if (_hasChosenReward)
            {
                return;
            }

            _hasChosenReward = true;
            CoinWallet.Instance.AddCoins(_coinBonus);
            LockRewardChoice();
        }

        private void ChooseTile()
        {
            if (_hasChosenReward)
            {
                return;
            }

            _hasChosenReward = true;
            PlacementCursor.Instance.SelectItem(_freeTileItem, free: true);
            LockRewardChoice();
        }

        private void LockRewardChoice()
        {
            _coinsButton.interactable = false;
            _tileButton.interactable = false;
            _startNextWaveButton.interactable = true;
        }

        private void StartNextWave()
        {
            if (!_hasChosenReward)
            {
                return;
            }

            IsBuildPhaseActive = false;
            Time.timeScale = GameSpeedController.Instance != null ? GameSpeedController.Instance.CurrentSpeed : 1f;
            _panel.SetActive(false);
            WaveManager.Instance.StartNextWave();
        }
    }
}
