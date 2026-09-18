using Game.Buildings;
using Game.Data;
using Game.Grid;
using Game.Systems;
using Game.Waves;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Drives the optional 5-wave tutorial run started from the main menu's
    /// Tutorial button. This is a guided, action-gated walkthrough rather
    /// than a slideshow: the player must actually pick up the basic Turret
    /// (every other hotbar item is locked out - see TutorialGate), place it
    /// on one specific highlighted tile, watch wave 1 land real hits on it,
    /// then click a highlighted empty tile and their own turret in turn to
    /// see both upgrade panels for real. Only the welcome and wrap-up steps
    /// are simple dismiss-to-continue cards; every step in between only
    /// advances once the player performs the actual action, each time with
    /// TutorialHighlight pointing at exactly what to click or drag.
    /// Entirely inert if GameSettings.IsTutorial wasn't set before this
    /// scene loaded, i.e. every normal run.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        public const int TutorialWaveCount = 5;

        private enum Step
        {
            Welcome,
            PlaceTurret,
            WatchWave,
            ClickTile,
            TileUpgradeShown,
            ClickTurret,
            TurretUpgradeShown,
            Wrapup,
            Done,
        }

        [Header("Hint card (welcome/wrap-up: modal; guided steps: compact banner)")]
        [SerializeField] private GameObject _hintPanel;
        [SerializeField] private Image _hintPanelBackground;
        [SerializeField] private RectTransform _hintCardRect;
        [SerializeField] private TMP_Text _hintProgressText;
        [SerializeField] private TMP_Text _hintBodyText;
        [SerializeField] private Button _hintNextButton;
        [SerializeField] private TMP_Text _hintNextButtonText;

        [Header("Completion")]
        [SerializeField] private GameObject _completePanel;
        [SerializeField] private Button _completeMainMenuButton;

        [Header("Guided-step targets")]
        [SerializeField] private TutorialHighlight _highlight;
        [SerializeField] private HotbarItemData _turretItem;
        [SerializeField] private RectTransform _turretHotbarSlotRect;

        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        // Modal card geometry (welcome/wrap-up) vs. compact banner geometry
        // (every guided step) - swapped in code rather than needing two
        // separate panel hierarchies.
        private static readonly Vector2 ModalCardPos = new Vector2(140f, -118f);
        private static readonly Vector2 ModalCardSize = new Vector2(360f, 180f);
        private static readonly Vector2 BannerCardPos = new Vector2(40f, -8f);
        private static readonly Vector2 BannerCardSize = new Vector2(560f, 40f);

        private Step _step;
        private Vector3Int _turretCell;
        private Vector3Int _tileHighlightCell;
        private GameObject _placedTurret;

        private void Awake()
        {
            if (!GameSettings.IsTutorial)
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _hintNextButton.onClick.AddListener(HandleNextClicked);
            _completeMainMenuButton.onClick.AddListener(BackToMainMenu);
            WaveManager.OnWaveCleared += HandleWaveCleared;
            BuildPlacer.OnPlaced += HandlePlaced;
            TileInspectorUI.OnOpened += HandleTileInspectorOpened;
            TileInspectorUI.OnClosed += HandleTileInspectorClosed;
            BuildingInspectorUI.OnOpened += HandleBuildingInspectorOpened;
            BuildingInspectorUI.OnClosed += HandleBuildingInspectorClosed;
        }

        private void OnDisable()
        {
            _hintNextButton.onClick.RemoveListener(HandleNextClicked);
            _completeMainMenuButton.onClick.RemoveListener(BackToMainMenu);
            WaveManager.OnWaveCleared -= HandleWaveCleared;
            BuildPlacer.OnPlaced -= HandlePlaced;
            TileInspectorUI.OnOpened -= HandleTileInspectorOpened;
            TileInspectorUI.OnClosed -= HandleTileInspectorClosed;
            BuildingInspectorUI.OnOpened -= HandleBuildingInspectorOpened;
            BuildingInspectorUI.OnClosed -= HandleBuildingInspectorClosed;

            // Never leave a restriction active if the tutorial ends abnormally mid-step.
            TutorialGate.RestrictedHotbarItem = null;
            TutorialGate.RestrictedPlacementCell = null;
        }

        private void Start()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.HoldFirstWave = true;
            }

            PickTargetCells();
            EnterStep(Step.Welcome);
        }

        /// <summary>Picks two distinct, currently-free tiles near the village: one for the guided turret placement, one for the guided "click an empty tile" step.</summary>
        private void PickTargetCells()
        {
            _turretCell = default;
            _tileHighlightCell = default;
            bool haveTurretCell = false;

            if (HexGridManager.Instance == null)
            {
                return;
            }

            foreach (Vector3Int cell in HexGridManager.Instance.GetAllTileCells())
            {
                if (HexGridManager.Instance.IsOccupied(cell))
                {
                    continue;
                }

                if (!haveTurretCell)
                {
                    _turretCell = cell;
                    haveTurretCell = true;
                }
                else
                {
                    _tileHighlightCell = cell;
                    break;
                }
            }
        }

        private void EnterStep(Step step)
        {
            _step = step;

            switch (step)
            {
                case Step.Welcome:
                    ShowModalCard(
                        "Welkom bij Island Survival TD! Verdedig je dorp tegen golven piraten. Deze tutorial laat je alles zelf doen - laten we een turret neerzetten.",
                        "VOLGENDE");
                    break;

                case Step.PlaceTurret:
                    HideModal();
                    ShowBanner("Sleep de TURRET uit de hotbar naar de gemarkeerde tegel - je kunt nu even niets anders pakken.");
                    TutorialGate.RestrictedHotbarItem = _turretItem;
                    TutorialGate.RestrictedPlacementCell = _turretCell;
                    if (_turretHotbarSlotRect != null)
                    {
                        _highlight.TrackUI(_turretHotbarSlotRect);
                    }

                    break;

                case Step.WatchWave:
                    TutorialGate.RestrictedHotbarItem = null;
                    TutorialGate.RestrictedPlacementCell = null;
                    _highlight.Hide();
                    ShowBanner("Kijk hoe wave 1 verloopt - let op de schade-cijfers die verschijnen als je turret raak schiet.");
                    if (WaveManager.Instance != null)
                    {
                        WaveManager.Instance.HoldFirstWave = false;
                    }

                    break;

                case Step.ClickTile:
                    ShowBanner("Klik op de gemarkeerde lege hex-tegel om 'm te upgraden.");
                    if (HexGridManager.Instance != null)
                    {
                        Vector3 worldPos = HexGridManager.Instance.CellToWorld(_tileHighlightCell);
                        Transform proxy = GetOrCreateWorldProxy(worldPos);
                        _highlight.TrackWorld(proxy, new Vector2(64f, 56f));
                    }

                    break;

                case Step.TileUpgradeShown:
                    _highlight.Hide();
                    ShowBanner("Hier koop je met munten upgrades voor de tegel - ze gelden voor de turret die erop staat (of ooit komt te staan). Sluit 'm maar weer.");
                    break;

                case Step.ClickTurret:
                    ShowBanner("Klik nu op je eigen turret om 'm te bekijken en te upgraden.");
                    if (_placedTurret != null)
                    {
                        _highlight.TrackWorld(_placedTurret.transform, new Vector2(40f, 40f));
                    }

                    break;

                case Step.TurretUpgradeShown:
                    _highlight.Hide();
                    ShowBanner("Hier koop je met munten upgrades voor de turret zelf - kies pad A of pad B. Sluit 'm maar weer.");
                    break;

                case Step.Wrapup:
                    HideModal();
                    _highlight.Hide();
                    ShowModalCard(
                        "Dat is de basis! Deze tutorial duurt nog tot wave 5 - experimenteer gerust verder, munten en XP tellen gewoon mee. Veel succes!",
                        "START!");
                    break;

                case Step.Done:
                    HideModal();
                    _highlight.Hide();
                    break;
            }
        }

        // A lightweight positional proxy so TutorialHighlight can track a
        // fixed world point (a tile's center) with the same Transform-based
        // API used for tracking a moving turret - created once and reused.
        private Transform _worldProxy;

        private Transform GetOrCreateWorldProxy(Vector3 worldPosition)
        {
            if (_worldProxy == null)
            {
                _worldProxy = new GameObject("TutorialHighlightWorldProxy").transform;
            }

            _worldProxy.position = worldPosition;
            return _worldProxy;
        }

        private void ShowModalCard(string message, string buttonLabel)
        {
            _hintPanel.SetActive(true);
            _hintPanelBackground.raycastTarget = true;
            _hintCardRect.anchoredPosition = ModalCardPos;
            _hintCardRect.sizeDelta = ModalCardSize;
            _hintNextButton.gameObject.SetActive(true);
            _hintProgressText.gameObject.SetActive(false);
            _hintNextButtonText.text = buttonLabel;
            _hintBodyText.fontSize = 10f;
            _hintBodyText.text = message;
        }

        private void ShowBanner(string message)
        {
            _hintPanel.SetActive(true);
            _hintPanelBackground.raycastTarget = false;
            _hintCardRect.anchoredPosition = BannerCardPos;
            _hintCardRect.sizeDelta = BannerCardSize;
            _hintNextButton.gameObject.SetActive(false);
            _hintProgressText.gameObject.SetActive(false);
            _hintBodyText.fontSize = 8f;
            _hintBodyText.text = message;
        }

        private void HideModal()
        {
            _hintPanel.SetActive(false);
        }

        private void HandleNextClicked()
        {
            if (_step == Step.Welcome)
            {
                EnterStep(Step.PlaceTurret);
            }
            else if (_step == Step.Wrapup)
            {
                HideModal();
                EnterStep(Step.Done);
            }
        }

        private void HandlePlaced(Vector3Int cell, GameObject building)
        {
            if (_step != Step.PlaceTurret || cell != _turretCell)
            {
                return;
            }

            _placedTurret = building;
            EnterStep(Step.WatchWave);
        }

        private void HandleWaveCleared(int waveNumber)
        {
            if (_step == Step.WatchWave && waveNumber == 1)
            {
                EnterStep(Step.ClickTile);
            }

            if (waveNumber >= TutorialWaveCount)
            {
                _completePanel.SetActive(true);
            }
        }

        private void HandleTileInspectorOpened()
        {
            if (_step == Step.ClickTile)
            {
                EnterStep(Step.TileUpgradeShown);
            }
        }

        private void HandleTileInspectorClosed()
        {
            if (_step == Step.TileUpgradeShown)
            {
                EnterStep(Step.ClickTurret);
            }
        }

        private void HandleBuildingInspectorOpened()
        {
            if (_step == Step.ClickTurret)
            {
                EnterStep(Step.TurretUpgradeShown);
            }
        }

        private void HandleBuildingInspectorClosed()
        {
            if (_step == Step.TurretUpgradeShown)
            {
                EnterStep(Step.Wrapup);
            }
        }

        private void BackToMainMenu()
        {
            GameSettings.IsTutorial = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(_mainMenuSceneName);
        }
    }
}
