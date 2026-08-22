using Game.Buildings;
using Game.Combat;
using Game.Economy;
using Game.Waves;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Live HUD: village health, coins and wave number. Purely a listener -
    /// decoupled from gameplay via the existing Health/CoinWallet/WaveManager
    /// events rather than polling.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Text _healthText;
        [SerializeField] private Text _coinsText;
        [SerializeField] private Text _waveText;

        private Health _villageHealth;

        private void OnEnable()
        {
            WaveManager.OnWaveStarted += HandleWaveStarted;
        }

        private void Start()
        {
            // Waits for Start (called after every object's Awake) rather than
            // subscribing in OnEnable, since Village/CoinWallet may not have
            // set their static Instance yet if this HUD's OnEnable runs first.
            if (CoinWallet.Instance != null)
            {
                CoinWallet.Instance.OnCoinsChanged += HandleCoinsChanged;
                HandleCoinsChanged(CoinWallet.Instance.Coins);
            }

            if (Village.Instance != null)
            {
                _villageHealth = Village.Instance.GetComponent<Health>();
                _villageHealth.OnDamaged += HandleVillageHealthChanged;
                _villageHealth.OnDeath += HandleVillageDeath;
                RefreshHealthText();
            }
        }

        private void OnDisable()
        {
            if (CoinWallet.Instance != null)
            {
                CoinWallet.Instance.OnCoinsChanged -= HandleCoinsChanged;
            }

            WaveManager.OnWaveStarted -= HandleWaveStarted;

            if (_villageHealth != null)
            {
                _villageHealth.OnDamaged -= HandleVillageHealthChanged;
                _villageHealth.OnDeath -= HandleVillageDeath;
            }
        }

        private void HandleCoinsChanged(int coins)
        {
            if (_coinsText != null)
            {
                _coinsText.text = $"Munten: {coins}";
            }
        }

        private void HandleWaveStarted(int wave)
        {
            if (_waveText != null)
            {
                _waveText.text = $"Wave: {wave}";
            }
        }

        private void HandleVillageHealthChanged(int damageAmount)
        {
            RefreshHealthText();
        }

        private void HandleVillageDeath()
        {
            RefreshHealthText();
        }

        private void RefreshHealthText()
        {
            if (_healthText != null && _villageHealth != null)
            {
                _healthText.text = $"Dorp: {_villageHealth.CurrentHealth}/{_villageHealth.MaxHealth}";
            }
        }
    }
}
