using Game.Buildings;
using Game.Data;
using Game.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Popup opened by clicking a placed turret: shows its live stats and
    /// lets the player buy a run-only upgrade level with in-run coins,
    /// separate from the permanent levels bought with XP in the main menu.
    /// </summary>
    public class BuildingInspectorUI : MonoBehaviour
    {
        public static BuildingInspectorUI Instance { get; private set; }

        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_Text _rangeText;
        [SerializeField] private TMP_Text _fireRateText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image[] _pips;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Image _upgradeButtonBackground;
        [SerializeField] private TMP_Text _upgradeButtonText;
        [SerializeField] private Button _closeButton;

        private Building _current;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            _upgradeButton.onClick.AddListener(Upgrade);
            _closeButton.onClick.AddListener(Close);
        }

        private void OnDisable()
        {
            _upgradeButton.onClick.RemoveListener(Upgrade);
            _closeButton.onClick.RemoveListener(Close);
            if (CoinWallet.Instance != null)
            {
                CoinWallet.Instance.OnCoinsChanged -= HandleCoinsChanged;
            }
        }

        private void Start()
        {
            // Waits for Start (called after every object's Awake) rather than
            // subscribing in OnEnable, since CoinWallet may not have set its
            // static Instance yet if this panel's OnEnable runs first.
            if (CoinWallet.Instance != null)
            {
                CoinWallet.Instance.OnCoinsChanged += HandleCoinsChanged;
            }
        }

        public void Open(Building building)
        {
            _current = building;
            _current.Health.OnDeath += HandleBuildingDied;
            _panel.SetActive(true);
            Refresh();
        }

        private void Close()
        {
            if (_current != null)
            {
                _current.Health.OnDeath -= HandleBuildingDied;
            }

            _current = null;
            _panel.SetActive(false);
        }

        private void HandleBuildingDied()
        {
            Close();
        }

        private void HandleCoinsChanged(int coins)
        {
            if (_current != null)
            {
                Refresh();
            }
        }

        private void Upgrade()
        {
            if (_current == null || CoinWallet.Instance == null)
            {
                return;
            }

            BuildingData data = _current.Data;
            Shooter shooter = _current.Shooter;
            if (shooter.RunUpgradeLevel >= data.MaxUpgradeLevel)
            {
                return;
            }

            int cost = data.RunUpgradeCost * (shooter.RunUpgradeLevel + 1);
            if (!CoinWallet.Instance.TrySpend(cost))
            {
                return;
            }

            shooter.AddRunUpgradeLevel();
            Refresh();
        }

        private void Refresh()
        {
            if (_current == null)
            {
                return;
            }

            BuildingData data = _current.Data;
            Shooter shooter = _current.Shooter;
            int level = shooter.RunUpgradeLevel;
            bool isMaxLevel = level >= data.MaxUpgradeLevel;

            if (_nameText != null)
            {
                _nameText.text = data.DisplayName.ToUpperInvariant();
            }

            if (_damageText != null)
            {
                _damageText.text = $"DAMAGE: {shooter.CurrentDamage}";
            }

            if (_rangeText != null)
            {
                _rangeText.text = $"RANGE: {data.Range:0.#} TILES";
            }

            if (_fireRateText != null)
            {
                _fireRateText.text = $"FIRE RATE: {data.FireRate:0.#}/s";
            }

            if (_levelText != null)
            {
                _levelText.text = $"RUN UPGRADE LVL {level}";
            }

            for (int i = 0; i < _pips.Length; i++)
            {
                if (_pips[i] != null)
                {
                    _pips[i].color = i < level ? UITheme.Gold : UITheme.Divider;
                }
            }

            if (isMaxLevel)
            {
                _upgradeButton.interactable = false;
                if (_upgradeButtonBackground != null)
                {
                    _upgradeButtonBackground.color = UITheme.SlotBackground;
                }
                if (_upgradeButtonText != null)
                {
                    _upgradeButtonText.text = "MAX";
                    _upgradeButtonText.color = UITheme.TextDisabled;
                }
            }
            else
            {
                int cost = data.RunUpgradeCost * (level + 1);
                bool affordable = CoinWallet.Instance != null && CoinWallet.Instance.Coins >= cost;
                _upgradeButton.interactable = affordable;
                if (_upgradeButtonBackground != null)
                {
                    _upgradeButtonBackground.color = affordable ? UITheme.Gold : UITheme.SlotBackground;
                }
                if (_upgradeButtonText != null)
                {
                    _upgradeButtonText.text = $"UPGRADE\n{cost} COINS";
                    _upgradeButtonText.color = affordable ? UITheme.ButtonTextDark : UITheme.TextDisabled;
                }
            }
        }
    }
}
