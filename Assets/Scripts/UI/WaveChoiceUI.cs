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
    /// Shown between waves: only holds the next wave back from spawning so
    /// the player can freely place and upgrade buildings without time
    /// pressure - it does NOT touch Time.timeScale/PauseController, so
    /// water, in-flight projectiles and anything else already moving keeps
    /// animating instead of the whole screen freezing solid. Pick either a
    /// coin bonus or a free hex tile, then either the next wave starts on
    /// its own (GameSettings.AutoStartNextWave) or the resume button
    /// becomes usable.
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
        }

        private void OnDisable()
        {
            WaveManager.OnWaveCleared -= HandleWaveCleared;
            _coinsButton.onClick.RemoveListener(ChooseCoins);
            _tileButton.onClick.RemoveListener(ChooseTile);
            _resumeButton.onClick.RemoveListener(HandleResumeButtonClicked);
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
        }

        /// <summary>The resume button only ever starts a wave that's waiting on a choice - a no-op the rest of the time, since it must never touch an active wave.</summary>
        private void HandleResumeButtonClicked()
        {
            if (!_waitingToStartNextWave)
            {
                return;
            }

            _waitingToStartNextWave = false;
            StartNextWave();
        }

        /// <summary>Grants the fallback reward if none was picked, closes the panel, and starts the next wave.</summary>
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
