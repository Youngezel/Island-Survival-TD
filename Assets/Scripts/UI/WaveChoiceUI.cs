using Game.Data;
using Game.Economy;
using Game.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Shown between waves: the player picks either a coin bonus or a free
    /// hex tile to place, within a countdown - if it runs out, the coin
    /// bonus (the left card) is chosen automatically. Confirming closes the
    /// panel and starts the next wave.
    /// </summary>
    public class WaveChoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _waveSurvivedText;
        [SerializeField] private TMP_Text _timerText;
        [SerializeField] private Image _timerBarFill;
        [SerializeField] private Button _coinsButton;
        [SerializeField] private Button _tileButton;
        [SerializeField] private HotbarItemData _freeTileItem;
        [SerializeField] private int _coinBonus = 15;
        [SerializeField] private float _decisionTimeSeconds = 8f;

        private float _timerBarMaxWidth;
        private float _remainingTime;
        private bool _waitingForChoice;

        private void Awake()
        {
            if (_timerBarFill != null)
            {
                _timerBarMaxWidth = _timerBarFill.rectTransform.sizeDelta.x;
            }
        }

        private void OnEnable()
        {
            WaveManager.OnWaveCleared += HandleWaveCleared;
            _coinsButton.onClick.AddListener(ChooseCoins);
            _tileButton.onClick.AddListener(ChooseTile);
        }

        private void OnDisable()
        {
            WaveManager.OnWaveCleared -= HandleWaveCleared;
            _coinsButton.onClick.RemoveListener(ChooseCoins);
            _tileButton.onClick.RemoveListener(ChooseTile);
        }

        private void HandleWaveCleared(int waveNumber)
        {
            if (_waveSurvivedText != null)
            {
                _waveSurvivedText.text = $"WAVE {waveNumber} OVERLEEFD";
            }

            _remainingTime = _decisionTimeSeconds;
            _waitingForChoice = true;
            RefreshTimer(WaveManager.Instance != null ? WaveManager.Instance.CurrentWave + 1 : waveNumber + 1);
            _panel.SetActive(true);
        }

        private void Update()
        {
            if (!_waitingForChoice)
            {
                return;
            }

            _remainingTime -= Time.deltaTime;
            RefreshTimer(WaveManager.Instance != null ? WaveManager.Instance.CurrentWave + 1 : 0);

            if (_remainingTime <= 0f)
            {
                ChooseCoins();
            }
        }

        private void RefreshTimer(int nextWaveNumber)
        {
            float clamped = Mathf.Max(0f, _remainingTime);

            if (_timerText != null)
            {
                _timerText.text = $"WAVE {nextWaveNumber} START OVER 0:{Mathf.CeilToInt(clamped):00} - KIEZEN IS VERPLICHT";
            }

            if (_timerBarFill != null)
            {
                float ratio = _decisionTimeSeconds > 0f ? clamped / _decisionTimeSeconds : 0f;
                _timerBarFill.rectTransform.sizeDelta = new Vector2(_timerBarMaxWidth * Mathf.Clamp01(ratio), _timerBarFill.rectTransform.sizeDelta.y);
            }
        }

        private void ChooseCoins()
        {
            _waitingForChoice = false;
            CoinWallet.Instance.AddCoins(_coinBonus);
            Close();
        }

        private void ChooseTile()
        {
            _waitingForChoice = false;
            PlacementCursor.Instance.SelectItem(_freeTileItem, free: true);
            Close();
        }

        private void Close()
        {
            _panel.SetActive(false);
            WaveManager.Instance.StartNextWave();
        }
    }
}
