using Game.Buildings;
using Game.Data;
using Game.Economy;
using Game.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Popup opened by clicking a placed turret or its hotbar slot: shows
    /// its live stats and lets the player buy a run-only upgrade level with
    /// in-run coins. The level is tracked per building TYPE (RunUpgradeManager),
    /// so it applies to every turret of that type - already placed and
    /// future - separate from the permanent per-type levels bought with XP
    /// in the main menu.
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

        private BuildingData _currentData;
        private Building _currentBuilding;

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

        /// <summary>
        /// Opens the inspector for a building type. <paramref name="building"/>
        /// is the specific placed instance that was clicked, if any - passing
        /// it lets the panel auto-close if that turret dies while open; pass
        /// null when opened from a hotbar slot, before anything is placed.
        /// </summary>
        public void Open(BuildingData data, Building building = null)
        {
            _currentData = data;
            _currentBuilding = building;
            if (_currentBuilding != null)
            {
                _currentBuilding.Health.OnDeath += HandleBuildingDied;
            }

            _panel.SetActive(true);
            Refresh();
        }

        private void Close()
        {
            if (_currentBuilding != null)
            {
                _currentBuilding.Health.OnDeath -= HandleBuildingDied;
            }

            _currentData = null;
            _currentBuilding = null;
            _panel.SetActive(false);
        }

        private void HandleBuildingDied()
        {
            Close();
        }

        private void HandleCoinsChanged(int coins)
        {
            if (_currentData != null)
            {
                Refresh();
            }
        }

        private void Upgrade()
        {
            if (_currentData == null || CoinWallet.Instance == null || RunUpgradeManager.Instance == null)
            {
                return;
            }

            int level = RunUpgradeManager.Instance.GetLevel(_currentData.UpgradeSaveKey);
            if (level >= _currentData.MaxUpgradeLevel)
            {
                return;
            }

            int cost = _currentData.RunUpgradeCost * (level + 1);
            if (!CoinWallet.Instance.TrySpend(cost))
            {
                return;
            }

            RunUpgradeManager.Instance.AddLevel(_currentData.UpgradeSaveKey);
            Refresh();
        }

        private void Refresh()
        {
            if (_currentData == null)
            {
                return;
            }

            int permanentLevel = SaveManager.Instance != null ? SaveManager.Instance.GetUpgradeLevel(_currentData.UpgradeSaveKey) : 0;
            int runLevel = RunUpgradeManager.Instance != null ? RunUpgradeManager.Instance.GetLevel(_currentData.UpgradeSaveKey) : 0;
            int currentDamage = _currentData.Damage + _currentData.DamagePerUpgradeLevel * (permanentLevel + runLevel);
            bool isMaxLevel = runLevel >= _currentData.MaxUpgradeLevel;

            if (_nameText != null)
            {
                _nameText.text = _currentData.DisplayName.ToUpperInvariant();
            }

            if (_damageText != null)
            {
                _damageText.text = $"DAMAGE: {currentDamage}";
            }

            if (_rangeText != null)
            {
                _rangeText.text = $"RANGE: {_currentData.Range:0.#} TILES";
            }

            if (_fireRateText != null)
            {
                _fireRateText.text = $"FIRE RATE: {_currentData.FireRate:0.#}/s";
            }

            if (_levelText != null)
            {
                _levelText.text = $"RUN UPGRADE LVL {runLevel}";
            }

            for (int i = 0; i < _pips.Length; i++)
            {
                if (_pips[i] != null)
                {
                    _pips[i].color = i < runLevel ? UITheme.Gold : UITheme.Divider;
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
                int cost = _currentData.RunUpgradeCost * (runLevel + 1);
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
