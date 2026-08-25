using System;
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
    /// its live stats and its two upgrade paths. Each path has 3 tiers;
    /// a tier must first be permanently unlocked with XP in the main menu
    /// before it can be activated here with in-run coins. Activating the
    /// first tier of a path commits this building TYPE to that path for
    /// the rest of the run - the other path locks out entirely, matching
    /// RunUpgradeManager, which tracks the committed path and reached tier
    /// per building type (not per placed instance).
    /// </summary>
    public class BuildingInspectorUI : MonoBehaviour
    {
        [Serializable]
        private class NodeRow
        {
            public Image Background;
            public TMP_Text Label;
            public Button Button;
        }

        public static BuildingInspectorUI Instance { get; private set; }

        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _nameText;
        [SerializeField] private TMP_Text _damageText;
        [SerializeField] private TMP_Text _rangeText;
        [SerializeField] private TMP_Text _fireRateText;
        [SerializeField] private NodeRow[] _pathARows = new NodeRow[3];
        [SerializeField] private NodeRow[] _pathBRows = new NodeRow[3];
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _sellButton;
        [SerializeField] private TMP_Text _sellButtonText;

        private const float SellRefundFraction = 0.5f;

        private BuildingData _currentData;
        private Building _currentBuilding;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(Close);
            _sellButton.onClick.AddListener(Sell);

            for (int i = 0; i < _pathARows.Length; i++)
            {
                int tierIndex = i;
                _pathARows[i].Button.onClick.AddListener(() => TryActivate(true, tierIndex));
            }

            for (int i = 0; i < _pathBRows.Length; i++)
            {
                int tierIndex = i;
                _pathBRows[i].Button.onClick.AddListener(() => TryActivate(false, tierIndex));
            }
        }

        private void Start()
        {
            // Waits for Start (called after every object's Awake) rather than
            // subscribing in OnEnable, since CoinWallet may not have set its
            // static Instance yet if this panel's OnEnable runs first.
            if (CoinWallet.Instance != null)
            {
                CoinWallet.Instance.OnCoinsChanged += HandleChanged;
            }

            if (RunUpgradeManager.Instance != null)
            {
                RunUpgradeManager.Instance.OnChanged += HandleKeyChanged;
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

        private void HandleChanged(int _)
        {
            if (_currentData != null)
            {
                Refresh();
            }
        }

        private void HandleKeyChanged(string key)
        {
            if (_currentData != null && _currentData.UpgradeSaveKey == key)
            {
                Refresh();
            }
        }

        private void TryActivate(bool pathA, int tierIndex)
        {
            if (_currentData == null || CoinWallet.Instance == null || RunUpgradeManager.Instance == null || SaveManager.Instance == null)
            {
                return;
            }

            string key = _currentData.UpgradeSaveKey;
            int unlockedTier = SaveManager.Instance.GetUnlockedTier(key, pathA);
            if (unlockedTier <= tierIndex)
            {
                return;
            }

            bool committed = RunUpgradeManager.Instance.HasCommittedPath(key);
            if (committed && RunUpgradeManager.Instance.IsPathA(key) != pathA)
            {
                return;
            }

            int currentTier = committed ? RunUpgradeManager.Instance.GetTier(key) : 0;
            if (currentTier != tierIndex)
            {
                return;
            }

            UpgradePath path = pathA ? _currentData.PathA : _currentData.PathB;
            UpgradeNode node = path.Nodes[tierIndex];

            if (!CoinWallet.Instance.TrySpend(node.ApplyCost))
            {
                return;
            }

            RunUpgradeManager.Instance.TryActivateNextTier(key, pathA);
            Refresh();
        }

        /// <summary>Sells the specific placed building this panel was opened for, refunding a fraction of its cost - not available when opened from a hotbar slot with no placed instance.</summary>
        private void Sell()
        {
            if (_currentBuilding == null || _currentData == null || CoinWallet.Instance == null)
            {
                return;
            }

            int refund = Mathf.RoundToInt(_currentData.Cost * SellRefundFraction);
            CoinWallet.Instance.AddCoins(refund);
            Building buildingToSell = _currentBuilding;
            Close();
            buildingToSell.Sell();
        }

        private void Refresh()
        {
            if (_currentData == null)
            {
                return;
            }

            if (_sellButton != null)
            {
                _sellButton.gameObject.SetActive(_currentBuilding != null);
                if (_currentBuilding != null && _sellButtonText != null)
                {
                    int refund = Mathf.RoundToInt(_currentData.Cost * SellRefundFraction);
                    _sellButtonText.text = $"VERKOOP\n+{refund} MUNT";
                }
            }

            int tier = RunUpgradeManager.Instance != null ? RunUpgradeManager.Instance.GetTier(_currentData.UpgradeSaveKey) : 0;
            bool committed = RunUpgradeManager.Instance != null && RunUpgradeManager.Instance.HasCommittedPath(_currentData.UpgradeSaveKey);
            bool pathAActive = committed && RunUpgradeManager.Instance.IsPathA(_currentData.UpgradeSaveKey);

            int damageBonus = 0;
            float rangeBonus = 0f;
            float fireRateBonus = 0f;
            UpgradePath activePath = pathAActive ? _currentData.PathA : _currentData.PathB;
            if (committed && activePath != null)
            {
                for (int i = 0; i < tier && i < activePath.Nodes.Length; i++)
                {
                    UpgradeNode node = activePath.Nodes[i];
                    if (node.Effect == UpgradeEffect.Damage) damageBonus += Mathf.RoundToInt(node.Value);
                    else if (node.Effect == UpgradeEffect.Range) rangeBonus += node.Value;
                    else if (node.Effect == UpgradeEffect.FireRate) fireRateBonus += node.Value;
                }
            }

            if (_nameText != null)
            {
                _nameText.text = _currentData.DisplayName.ToUpperInvariant();
            }

            if (_damageText != null)
            {
                _damageText.text = $"DAMAGE: {_currentData.Damage + damageBonus}";
            }

            if (_rangeText != null)
            {
                _rangeText.text = $"RANGE: {(_currentData.Range + rangeBonus):0.#} TILES";
            }

            if (_fireRateText != null)
            {
                _fireRateText.text = $"FIRE RATE: {(_currentData.FireRate + fireRateBonus):0.#}/s";
            }

            RefreshPath(_currentData.PathA, true, _pathARows);
            RefreshPath(_currentData.PathB, false, _pathBRows);
        }

        private void RefreshPath(UpgradePath path, bool isPathA, NodeRow[] rows)
        {
            if (path == null || _currentData == null)
            {
                return;
            }

            string key = _currentData.UpgradeSaveKey;
            int unlockedTier = SaveManager.Instance != null ? SaveManager.Instance.GetUnlockedTier(key, isPathA) : 0;
            bool committed = RunUpgradeManager.Instance != null && RunUpgradeManager.Instance.HasCommittedPath(key);
            bool thisPathCommitted = committed && RunUpgradeManager.Instance.IsPathA(key) == isPathA;
            bool otherPathCommitted = committed && !thisPathCommitted;
            int activeTier = thisPathCommitted && RunUpgradeManager.Instance != null ? RunUpgradeManager.Instance.GetTier(key) : 0;

            for (int i = 0; i < rows.Length && i < path.Nodes.Length; i++)
            {
                UpgradeNode node = path.Nodes[i];
                NodeRow row = rows[i];
                bool permanentlyUnlocked = unlockedTier > i;
                bool isActive = thisPathCommitted && activeTier > i;
                bool isNextActivatable = permanentlyUnlocked && !otherPathCommitted && !isActive && activeTier == i;

                if (otherPathCommitted)
                {
                    row.Label.text = node.Name;
                    row.Background.color = UITheme.SlotBackground;
                    row.Button.interactable = false;
                    row.Label.color = UITheme.TextDisabled;
                }
                else if (isActive)
                {
                    row.Label.text = $"{node.Name}\nACTIEF";
                    row.Background.color = UITheme.Gold;
                    row.Button.interactable = false;
                    row.Label.color = UITheme.ButtonTextDark;
                }
                else if (!permanentlyUnlocked)
                {
                    row.Label.text = $"{node.Name}\n(hoofdmenu)";
                    row.Background.color = UITheme.SlotBackground;
                    row.Button.interactable = false;
                    row.Label.color = UITheme.TextDisabled;
                }
                else if (isNextActivatable)
                {
                    bool affordable = CoinWallet.Instance != null && CoinWallet.Instance.Coins >= node.ApplyCost;
                    row.Label.text = $"{node.Name}\n{node.ApplyCost} COINS";
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
