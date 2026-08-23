using Game.Data;
using Game.Economy;
using Game.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// One row in the main menu upgrade shop: icon, name, level pips and an
    /// upgrade button that shows the next level's XP cost, or MAX once the
    /// building's max upgrade level is reached.
    /// </summary>
    public class UpgradeRow : MonoBehaviour
    {
        [SerializeField] private BuildingData _buildingData;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _levelText;
        [SerializeField] private Image[] _pips;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Image _upgradeButtonBackground;
        [SerializeField] private TMP_Text _upgradeButtonText;

        private int CurrentLevel => SaveManager.Instance != null ? SaveManager.Instance.GetUpgradeLevel(_buildingData.UpgradeSaveKey) : 0;
        private bool IsMaxLevel => CurrentLevel >= _buildingData.MaxUpgradeLevel;
        private int CostForNextLevel => _buildingData.UpgradeCost * (CurrentLevel + 1);

        private void Awake()
        {
            if (_icon != null && _buildingData != null)
            {
                _icon.sprite = _buildingData.Icon;
            }

            if (_nameText != null && _buildingData != null)
            {
                _nameText.text = _buildingData.DisplayName.ToUpperInvariant();
            }
        }

        private void OnEnable()
        {
            _upgradeButton.onClick.AddListener(Upgrade);
        }

        private void OnDisable()
        {
            _upgradeButton.onClick.RemoveListener(Upgrade);
            if (XPWallet.Instance != null)
            {
                XPWallet.Instance.OnXPChanged -= HandleXPChanged;
            }
        }

        private void Start()
        {
            // Waits for Start (called after every object's Awake) rather than
            // subscribing in OnEnable, since XPWallet may not have set its
            // static Instance yet if this row's OnEnable runs first.
            if (XPWallet.Instance != null)
            {
                XPWallet.Instance.OnXPChanged += HandleXPChanged;
            }

            Refresh();
        }

        private void HandleXPChanged(int xp)
        {
            Refresh();
        }

        private void Upgrade()
        {
            if (XPWallet.Instance == null || SaveManager.Instance == null || IsMaxLevel)
            {
                return;
            }

            int level = CurrentLevel;
            if (!XPWallet.Instance.TrySpend(CostForNextLevel))
            {
                return;
            }

            SaveManager.Instance.SetUpgradeLevel(_buildingData.UpgradeSaveKey, level + 1);
            SaveManager.Instance.Save();
            Refresh();
        }

        private void Refresh()
        {
            int level = CurrentLevel;

            if (_levelText != null)
            {
                _levelText.text = $"LVL {level}";
            }

            for (int i = 0; i < _pips.Length; i++)
            {
                if (_pips[i] != null)
                {
                    _pips[i].color = i < level ? UITheme.Gold : UITheme.Divider;
                }
            }

            if (IsMaxLevel)
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
                bool affordable = XPWallet.Instance != null && XPWallet.Instance.XP >= CostForNextLevel;
                _upgradeButton.interactable = affordable;
                if (_upgradeButtonBackground != null)
                {
                    _upgradeButtonBackground.color = affordable ? UITheme.Gold : UITheme.SlotBackground;
                }
                if (_upgradeButtonText != null)
                {
                    _upgradeButtonText.text = $"UPGRADE\n{CostForNextLevel} XP";
                    _upgradeButtonText.color = affordable ? UITheme.ButtonTextDark : UITheme.TextDisabled;
                }
            }
        }
    }
}
