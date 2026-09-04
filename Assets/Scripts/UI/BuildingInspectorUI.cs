using System;
using Game.Buildings;
using Game.Data;
using Game.Economy;
using Game.Grid;
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
            public Image StateGlyph;
            public Image TypeGlyph;
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
        [SerializeField] private RangeIndicator _rangeIndicator;

        // Row-state art from the sidebar UI design handoff - swapped onto
        // NodeRow.Background instead of tinting a flat color rect.
        [SerializeField] private Sprite _rowLockedSprite;
        [SerializeField] private Sprite _rowBuySprite;
        [SerializeField] private Sprite _rowActiveSprite;
        [SerializeField] private Sprite _rowPathLockedSprite;
        [SerializeField] private Sprite _glyphPadlock;
        [SerializeField] private Sprite _glyphCheck;
        [SerializeField] private Sprite _glyphCross;
        [SerializeField] private Sprite _glyphDamage;
        [SerializeField] private Sprite _glyphRange;
        [SerializeField] private Sprite _glyphFireRate;
        [SerializeField] private Sprite _glyphSplash;
        [SerializeField] private Sprite _glyphPierce;
        [SerializeField] private Sprite _glyphMultiShot;

        // "Purchasable but can't afford it yet" isn't a state the handoff
        // cut a separate asset for - its own recommendation is row_buy
        // multiply-tinted with the gold-shadow color instead.
        private static readonly Color UnaffordableTint = UITheme.GoldShadow;

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
            _rangeIndicator?.Hide();
        }

        /// <summary>Draws (or updates) the range ring around the inspected turret - called on open and again on Refresh so a range upgrade resizes it live.</summary>
        private void RefreshRangeIndicator()
        {
            if (_rangeIndicator == null)
            {
                return;
            }

            if (_currentBuilding == null || _currentBuilding.Shooter == null || HexGridManager.Instance == null)
            {
                _rangeIndicator.Hide();
                return;
            }

            float worldRadius = _currentBuilding.Shooter.CurrentRange * HexGridManager.Instance.HexStepWorldDistance;
            _rangeIndicator.Show(_currentBuilding.transform.position, worldRadius);
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
            RefreshRangeIndicator();
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

                if (row.TypeGlyph != null)
                {
                    row.TypeGlyph.sprite = GlyphFor(node.Effect);
                }

                if (otherPathCommitted)
                {
                    ApplyPathLockedVisual(row, node);
                }
                else if (isActive)
                {
                    ApplyActiveVisual(row, node);
                }
                else if (!permanentlyUnlocked)
                {
                    ApplyLockedVisual(row, node, "(hoofdmenu)");
                }
                else if (isNextActivatable)
                {
                    bool affordable = CoinWallet.Instance != null && CoinWallet.Instance.Coins >= node.ApplyCost;
                    ApplyBuyVisual(row, node, affordable);
                }
                else
                {
                    // Permanently unlocked but this run hasn't reached this
                    // tier yet - reads the same as "locked" since it isn't
                    // reachable until the earlier tiers are bought.
                    ApplyLockedVisual(row, node, null);
                }
            }
        }

        private Sprite GlyphFor(UpgradeEffect effect)
        {
            switch (effect)
            {
                case UpgradeEffect.Damage: return _glyphDamage;
                case UpgradeEffect.Range: return _glyphRange;
                case UpgradeEffect.FireRate: return _glyphFireRate;
                case UpgradeEffect.SplashDamage: return _glyphSplash;
                case UpgradeEffect.PiercingShot: return _glyphPierce;
                case UpgradeEffect.SpreadShot:
                case UpgradeEffect.SequentialDoubleShot:
                case UpgradeEffect.MultiTargetShot:
                    return _glyphMultiShot;
                case UpgradeEffect.FireDamage:
                    return _glyphDamage;
                default:
                    return null;
            }
        }

        private void ApplyLockedVisual(NodeRow row, UpgradeNode node, string suffix)
        {
            row.Label.text = suffix != null ? $"{node.Name}\n{suffix}" : node.Name;
            row.Background.sprite = _rowLockedSprite;
            row.Background.color = Color.white;
            row.Button.interactable = false;
            row.Label.color = UITheme.TextDisabled;
            SetGlyph(row.StateGlyph, _glyphPadlock, UITheme.TextDisabled);
        }

        private void ApplyActiveVisual(NodeRow row, UpgradeNode node)
        {
            row.Label.text = $"{node.Name}\nACTIEF";
            row.Background.sprite = _rowActiveSprite;
            row.Background.color = Color.white;
            row.Button.interactable = false;
            row.Label.color = UITheme.Gold;
            SetGlyph(row.StateGlyph, _glyphCheck, UITheme.Gold);
        }

        private void ApplyPathLockedVisual(NodeRow row, UpgradeNode node)
        {
            row.Label.text = node.Name;
            row.Background.sprite = _rowPathLockedSprite;
            row.Background.color = Color.white;
            row.Button.interactable = false;
            row.Label.color = UITheme.TextDisabled;
            SetGlyph(row.StateGlyph, _glyphCross, UITheme.TextDisabled);
        }

        private void ApplyBuyVisual(NodeRow row, UpgradeNode node, bool affordable)
        {
            row.Label.text = $"{node.Name}\n{node.ApplyCost} COINS";
            row.Background.sprite = _rowBuySprite;
            row.Background.color = affordable ? Color.white : UnaffordableTint;
            row.Button.interactable = affordable;
            row.Label.color = UITheme.ButtonTextDark;
            SetGlyph(row.StateGlyph, null, UITheme.ButtonTextDark);
        }

        private static void SetGlyph(Image glyph, Sprite sprite, Color tint)
        {
            if (glyph == null)
            {
                return;
            }

            glyph.sprite = sprite;
            glyph.color = tint;
            glyph.enabled = sprite != null;
        }
    }
}
