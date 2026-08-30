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
    /// Popup opened by clicking a placed turret: shows its live stats and
    /// its two upgrade paths. Each path has 3 tiers; a tier must first be
    /// permanently unlocked with XP in the main menu before it can be
    /// activated here with in-run coins. Activating the first tier of a
    /// path commits this specific placed building (its Shooter) to that
    /// path for the rest of the run - the other path locks out entirely,
    /// but every other turret of the same type keeps upgrading (or not)
    /// completely independently.
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
        }

        /// <summary>
        /// Opens the inspector for the specific placed turret that was
        /// clicked - its stats and upgrade path belong to that instance
        /// alone. Also lets the panel auto-close if that turret dies or
        /// refresh if its upgrade state changes while open.
        /// </summary>
        public void Open(BuildingData data, Building building)
        {
            _currentData = data;
            _currentBuilding = building;
            if (_currentBuilding != null)
            {
                _currentBuilding.Health.OnDeath += HandleBuildingDied;
                _currentBuilding.Shooter.OnUpgradeChanged += HandleUpgradeChanged;
            }

            _panel.SetActive(true);
            Refresh();
        }

        private void Close()
        {
            if (_currentBuilding != null)
            {
                _currentBuilding.Health.OnDeath -= HandleBuildingDied;
                _currentBuilding.Shooter.OnUpgradeChanged -= HandleUpgradeChanged;
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

        private void HandleUpgradeChanged()
        {
            if (_currentData != null)
            {
                Refresh();
            }
        }

        private void TryActivate(bool pathA, int tierIndex)
        {
            if (_currentData == null || _currentBuilding == null || CoinWallet.Instance == null || SaveManager.Instance == null)
            {
                return;
            }

            Shooter shooter = _currentBuilding.Shooter;
            string key = _currentData.UpgradeSaveKey;
            int unlockedTier = SaveManager.Instance.GetUnlockedTier(key, pathA);
            if (unlockedTier <= tierIndex)
            {
                return;
            }

            bool committed = shooter.HasCommittedPath;
            if (committed && shooter.IsPathACommitted != pathA)
            {
                return;
            }

            int currentTier = committed ? shooter.RunUpgradeTier : 0;
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

            shooter.TryActivateNextTier(pathA);
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

            Shooter shooter = _currentBuilding != null ? _currentBuilding.Shooter : null;
            int tier = shooter != null ? shooter.RunUpgradeTier : 0;
            bool committed = shooter != null && shooter.HasCommittedPath;
            bool pathAActive = committed && shooter.IsPathACommitted;

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
            if (path == null || _currentData == null || _currentBuilding == null)
            {
                return;
            }

            Shooter shooter = _currentBuilding.Shooter;
            string key = _currentData.UpgradeSaveKey;
            int unlockedTier = SaveManager.Instance != null ? SaveManager.Instance.GetUnlockedTier(key, isPathA) : 0;
            bool committed = shooter.HasCommittedPath;
            bool thisPathCommitted = committed && shooter.IsPathACommitted == isPathA;
            bool otherPathCommitted = committed && !thisPathCommitted;
            int activeTier = thisPathCommitted ? shooter.RunUpgradeTier : 0;

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
