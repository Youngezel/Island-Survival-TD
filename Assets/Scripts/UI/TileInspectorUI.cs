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
    /// Popup opened by clicking a hex tile (anywhere on it except directly on
    /// a turret standing there, which opens BuildingInspectorUI instead):
    /// shows that cell's own "Fundering" upgrade path - a separate, per-cell
    /// upgrade tree that boosts whatever turret is (or later gets) placed on
    /// this specific tile, independent of that turret's own upgrade path.
    /// Works the same whether the tile is empty (the bonus is banked for
    /// whenever a turret gets built there - see Building.Start) or already
    /// has a turret on it (a Health tier applies to it immediately; Damage/
    /// Range bonuses already apply live via Shooter.ComputeActiveEffects).
    /// </summary>
    public class TileInspectorUI : MonoBehaviour
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

        public static TileInspectorUI Instance { get; private set; }

        [SerializeField] private GameObject _panel;
        [SerializeField] private TMP_Text _healthBonusText;
        [SerializeField] private NodeRow[] _pathARows = new NodeRow[3];
        [SerializeField] private NodeRow[] _pathBRows = new NodeRow[3];
        [SerializeField] private Button _closeButton;

        // Same row-state art as BuildingInspectorUI - shared design handoff assets.
        [SerializeField] private Sprite _rowLockedSprite;
        [SerializeField] private Sprite _rowBuySprite;
        [SerializeField] private Sprite _rowActiveSprite;
        [SerializeField] private Sprite _rowPathLockedSprite;
        [SerializeField] private Sprite _glyphPadlock;
        [SerializeField] private Sprite _glyphCheck;
        [SerializeField] private Sprite _glyphCross;
        [SerializeField] private Sprite _glyphDamage;
        [SerializeField] private Sprite _glyphRange;

        private static readonly Color UnaffordableTint = UITheme.GoldShadow;

        private bool _isOpen;
        private Vector3Int _currentCell;

        private void Awake()
        {
            Instance = this;
        }

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(Close);

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

        /// <summary>Opens the inspector for this specific hex cell's own Fundering upgrade progress.</summary>
        public void Open(Vector3Int cell)
        {
            BuildingInspectorUI.Instance?.Close();

            _isOpen = true;
            _currentCell = cell;
            _panel.SetActive(true);
            Refresh();
        }

        public void Close()
        {
            _isOpen = false;
            _panel.SetActive(false);
        }

        private void HandleChanged(int _)
        {
            if (_isOpen)
            {
                Refresh();
            }
        }

        /// <summary>
        /// Activates the next tier of the given path on this cell. A Health
        /// effect applies immediately to whichever turret (if any) currently
        /// stands here, since it's a one-shot bonus rather than something
        /// Shooter recomputes continuously like Damage/Range - a turret built
        /// here later instead picks up the accumulated bonus at spawn (see
        /// Building.Start).
        /// </summary>
        private void TryActivate(bool pathA, int tierIndex)
        {
            if (!_isOpen || CoinWallet.Instance == null || SaveManager.Instance == null || HexGridManager.Instance == null)
            {
                return;
            }

            BuildingData tileData = HexGridManager.Instance.TileUpgradeData;
            if (tileData == null)
            {
                return;
            }

            int unlockedTier = SaveManager.Instance.GetUnlockedTier(tileData.UpgradeSaveKey, pathA);
            if (unlockedTier <= tierIndex)
            {
                return;
            }

            bool committed = HexGridManager.Instance.TileHasCommittedPath(_currentCell);
            if (committed && HexGridManager.Instance.TileIsPathACommitted(_currentCell) != pathA)
            {
                return;
            }

            int currentTier = committed ? HexGridManager.Instance.GetTileUpgradeTier(_currentCell) : 0;
            if (currentTier != tierIndex)
            {
                return;
            }

            UpgradePath path = pathA ? tileData.PathA : tileData.PathB;
            UpgradeNode node = path.Nodes[tierIndex];

            if (!CoinWallet.Instance.TrySpend(node.ApplyCost))
            {
                return;
            }

            HexGridManager.Instance.TryActivateTileNextTier(_currentCell, pathA);
            if (node.Effect == UpgradeEffect.Health)
            {
                foreach (Building building in Building.ActiveBuildings)
                {
                    if (building != null && building.Cell == _currentCell)
                    {
                        building.Health.AddMaxHealth(Mathf.RoundToInt(node.Value));
                        break;
                    }
                }
            }

            Refresh();
        }

        private void Refresh()
        {
            if (!_isOpen || HexGridManager.Instance == null)
            {
                return;
            }

            BuildingData tileData = HexGridManager.Instance.TileUpgradeData;
            if (tileData == null)
            {
                return;
            }

            if (_healthBonusText != null)
            {
                int healthBonus = HexGridManager.Instance.GetTileHealthBonus(_currentCell);
                _healthBonusText.text = $"HEALTH: +{healthBonus}";
            }

            RefreshPath(tileData.PathA, true, _pathARows, tileData);
            RefreshPath(tileData.PathB, false, _pathBRows, tileData);
        }

        private void RefreshPath(UpgradePath path, bool isPathA, NodeRow[] rows, BuildingData tileData)
        {
            if (path == null)
            {
                return;
            }

            int unlockedTier = SaveManager.Instance != null ? SaveManager.Instance.GetUnlockedTier(tileData.UpgradeSaveKey, isPathA) : 0;
            bool committed = HexGridManager.Instance.TileHasCommittedPath(_currentCell);
            bool thisPathCommitted = committed && HexGridManager.Instance.TileIsPathACommitted(_currentCell) == isPathA;
            bool otherPathCommitted = committed && !thisPathCommitted;
            int activeTier = thisPathCommitted ? HexGridManager.Instance.GetTileUpgradeTier(_currentCell) : 0;

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
                default: return null;
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
