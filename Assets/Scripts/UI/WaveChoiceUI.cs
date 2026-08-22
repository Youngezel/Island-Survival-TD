using Game.Data;
using Game.Economy;
using Game.Waves;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Shown between waves: the player picks either a coin bonus or a free
    /// hex tile to place. Confirming closes the panel and starts the next
    /// wave.
    /// </summary>
    public class WaveChoiceUI : MonoBehaviour
    {
        [SerializeField] private GameObject _panel;
        [SerializeField] private Button _coinsButton;
        [SerializeField] private Button _tileButton;
        [SerializeField] private HotbarItemData _freeTileItem;
        [SerializeField] private int _coinBonus = 15;

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
            _panel.SetActive(true);
        }

        private void ChooseCoins()
        {
            CoinWallet.Instance.AddCoins(_coinBonus);
            Close();
        }

        private void ChooseTile()
        {
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
