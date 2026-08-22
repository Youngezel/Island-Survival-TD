using Game.Data;
using Game.Economy;
using Game.Systems;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// One row in the main menu upgrade shop: shows a building's current
    /// upgrade level and the XP cost for the next one, and buys it on click.
    /// </summary>
    public class UpgradeRow : MonoBehaviour
    {
        [SerializeField] private BuildingData _buildingData;
        [SerializeField] private Text _nameText;
        [SerializeField] private Text _levelText;
        [SerializeField] private Text _costText;
        [SerializeField] private Button _upgradeButton;

        private int CurrentLevel => SaveManager.Instance != null ? SaveManager.Instance.GetUpgradeLevel(_buildingData.UpgradeSaveKey) : 0;
        private int CostForNextLevel => _buildingData.UpgradeCost * (CurrentLevel + 1);

        private void OnEnable()
        {
            _upgradeButton.onClick.AddListener(Upgrade);
            if (XPWallet.Instance != null)
            {
                XPWallet.Instance.OnXPChanged += HandleXPChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            _upgradeButton.onClick.RemoveListener(Upgrade);
            if (XPWallet.Instance != null)
            {
                XPWallet.Instance.OnXPChanged -= HandleXPChanged;
            }
        }

        private void HandleXPChanged(int xp)
        {
            Refresh();
        }

        private void Upgrade()
        {
            if (XPWallet.Instance == null || SaveManager.Instance == null)
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
            _nameText.text = _buildingData.DisplayName;
            _levelText.text = $"Level {CurrentLevel}";
            _costText.text = $"{CostForNextLevel} XP";
            _upgradeButton.interactable = XPWallet.Instance != null && XPWallet.Instance.XP >= CostForNextLevel;
        }
    }
}
