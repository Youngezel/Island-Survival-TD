using Game.Buildings;
using Game.Combat;
using Game.Economy;
using Game.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Live HUD top bar: village health (icon + bar + text), coins and wave
    /// number. Purely a listener - decoupled from gameplay via the existing
    /// Health/CoinWallet/WaveManager events rather than polling.
    /// </summary>
    public class HUD : MonoBehaviour
    {
        [SerializeField] private Image _healthBarFill;
        [SerializeField] private TMP_Text _healthText;
        [SerializeField] private TMP_Text _coinsText;
        [SerializeField] private TMP_Text _waveNumberText;

        private Health _villageHealth;
        private float _healthBarMaxWidth;

        private void Awake()
        {
            if (_healthBarFill != null)
            {
                _healthBarMaxWidth = _healthBarFill.rectTransform.sizeDelta.x;
            }
        }

        private void OnEnable()
        {
            WaveManager.OnWaveStarted += HandleWaveStarted;
        }

        private void OnDisable()
        {
            WaveManager.OnWaveStarted -= HandleWaveStarted;

            if (_villageHealth != null)
            {
                _villageHealth.OnDamaged -= HandleVillageHealthChanged;
                _villageHealth.OnDeath -= HandleVillageDeath;
            }
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
                RefreshHealthDisplay();
            }
        }

        private void HandleCoinsChanged(int coins)
        {
            if (_coinsText != null)
            {
                _coinsText.text = coins.ToString();
            }
        }

        private void HandleWaveStarted(int wave)
        {
            if (_waveNumberText != null)
            {
                _waveNumberText.text = wave.ToString();
            }
        }

        private void HandleVillageHealthChanged(int damageAmount)
        {
            RefreshHealthDisplay();
        }

        private void HandleVillageDeath()
        {
            RefreshHealthDisplay();
        }

        private void RefreshHealthDisplay()
        {
            if (_villageHealth == null)
            {
                return;
            }

            if (_healthText != null)
            {
                _healthText.text = $"{_villageHealth.CurrentHealth}/{_villageHealth.MaxHealth}";
            }

            if (_healthBarFill != null)
            {
                float ratio = _villageHealth.MaxHealth > 0 ? (float)_villageHealth.CurrentHealth / _villageHealth.MaxHealth : 0f;
                _healthBarFill.rectTransform.sizeDelta = new Vector2(_healthBarMaxWidth * Mathf.Clamp01(ratio), _healthBarFill.rectTransform.sizeDelta.y);
            }
        }
    }
}
