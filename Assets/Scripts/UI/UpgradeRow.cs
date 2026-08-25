using System;
using Game.Data;
using Game.Economy;
using Game.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// One building's upgrade row in the main menu: icon, name, and both
    /// upgrade paths' 3 tiers, each unlockable in order with XP. Unlocking
    /// a tier here only makes it permanently available - it doesn't apply
    /// to a run by itself; the player picks/advances one path per building
    /// type in-game with coins, via BuildingInspectorUI.
    /// </summary>
    public class UpgradeRow : MonoBehaviour
    {
        [Serializable]
        private class NodeRow
        {
            public Image Background;
            public TMP_Text Label;
            public Button Button;
        }

        [SerializeField] private BuildingData _buildingData;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private NodeRow[] _pathARows = new NodeRow[3];
        [SerializeField] private NodeRow[] _pathBRows = new NodeRow[3];

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
            for (int i = 0; i < _pathARows.Length; i++)
            {
                int tierIndex = i;
                _pathARows[i].Button.onClick.AddListener(() => TryUnlock(true, tierIndex));
            }

            for (int i = 0; i < _pathBRows.Length; i++)
            {
                int tierIndex = i;
                _pathBRows[i].Button.onClick.AddListener(() => TryUnlock(false, tierIndex));
            }
        }

        private void OnDisable()
        {
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

        private void TryUnlock(bool pathA, int tierIndex)
        {
            if (_buildingData == null || XPWallet.Instance == null || SaveManager.Instance == null)
            {
                return;
            }

            int currentTier = SaveManager.Instance.GetUnlockedTier(_buildingData.UpgradeSaveKey, pathA);
            if (currentTier != tierIndex)
            {
                return;
            }

            UpgradePath path = pathA ? _buildingData.PathA : _buildingData.PathB;
            UpgradeNode node = path.Nodes[tierIndex];

            if (!XPWallet.Instance.TrySpend(node.UnlockCost))
            {
                return;
            }

            SaveManager.Instance.SetUnlockedTier(_buildingData.UpgradeSaveKey, pathA, tierIndex + 1);
            SaveManager.Instance.Save();
            Refresh();
        }

        private void Refresh()
        {
            RefreshPath(_buildingData.PathA, true, _pathARows);
            RefreshPath(_buildingData.PathB, false, _pathBRows);
        }

        private void RefreshPath(UpgradePath path, bool isPathA, NodeRow[] rows)
        {
            if (path == null || _buildingData == null)
            {
                return;
            }

            int unlockedTier = SaveManager.Instance != null ? SaveManager.Instance.GetUnlockedTier(_buildingData.UpgradeSaveKey, isPathA) : 0;

            for (int i = 0; i < rows.Length && i < path.Nodes.Length; i++)
            {
                UpgradeNode node = path.Nodes[i];
                NodeRow row = rows[i];
                bool unlocked = unlockedTier > i;
                bool isNext = unlockedTier == i;

                if (unlocked)
                {
                    row.Label.text = $"{node.Name}\nVRIJGESPEELD";
                    row.Background.color = UITheme.Gold;
                    row.Button.interactable = false;
                    row.Label.color = UITheme.ButtonTextDark;
                }
                else if (isNext)
                {
                    bool affordable = XPWallet.Instance != null && XPWallet.Instance.XP >= node.UnlockCost;
                    row.Label.text = $"{node.Name}\n{node.UnlockCost} XP";
                    row.Background.color = affordable ? UITheme.Gold : UITheme.SlotBackground;
                    row.Button.interactable = affordable;
                    row.Label.color = affordable ? UITheme.ButtonTextDark : UITheme.TextDisabled;
                }
                else
                {
                    row.Label.text = node.Name;
                    row.Background.color = UITheme.SlotBackground;
                    row.Button.interactable = false;
                    row.Label.color = UITheme.TextDisabled;
                }
            }
        }
    }
}
