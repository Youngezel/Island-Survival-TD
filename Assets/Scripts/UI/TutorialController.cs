using System.Collections;
using System.Collections.Generic;
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
    /// see the coins-vs-free-tile reward choice explained once it appears,
    /// then click a highlighted empty tile and their own turret in turn to
    /// see both upgrade panels for real. Only the welcome and wrap-up steps
    /// are simple dismiss-to-continue cards; every step in between only
    /// advances once the player performs the actual action. World targets
    /// (the tile, the turret) are outlined with TutorialWorldHighlight in
    /// their own real shape (hexagon / turret box); the hotbar slot is
    /// outlined with the UI-space TutorialHighlight. Entirely inert if
    /// GameSettings.IsTutorial wasn't set before this scene loaded, i.e.
    /// every normal run.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        public const int TutorialWaveCount = 5;

        private enum Step
        {
            Welcome,
            PlaceTurret,
            WatchWave,
            RewardChoice,
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
        [SerializeField] private GameObject _hintHeading;
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
        [SerializeField] private TutorialWorldHighlight _worldHighlight;
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
            WaveChoiceUI.OnResolved += HandleRewardResolved;
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
            WaveChoiceUI.OnResolved -= HandleRewardResolved;
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

        /// <summary>
        /// Picks two distinct, currently-free tiles near the village: one
        /// for the guided turret placement, one for the guided "click an
        /// empty tile" step. The second one deliberately excludes every
        /// neighbor of the turret's cell too, not just the cell itself -
        /// otherwise it can end up sharing an edge with the placed turret,
        /// where a click meant for the tile lands on the turret's own
        /// collider instead and opens the wrong inspector.
        /// Both also explicitly exclude the village's own cell, rather than
        /// trusting IsOccupied alone - Village registers its cell as
        /// occupied in its own Start(), and Unity doesn't guarantee that
        /// runs before this Start() does, so IsOccupied can still read false
        /// for the village's cell at pick time (Enemy's own damageable-
        /// target search has the same defensive check for the same reason).
        /// Without it, the village's cell could get picked as the "click
        /// this empty tile" target - a tile a building already stands on,
        /// so clicking it opens the building's inspector instead and the
        /// step can never be completed the way it's described.
        /// </summary>
        private void PickTargetCells()
        {
            _turretCell = default;
            _tileHighlightCell = default;

            if (HexGridManager.Instance == null)
            {
                return;
            }

            Vector3Int? villageCell = Village.Instance != null
                ? HexGridManager.Instance.WorldToCell(Village.Instance.transform.position)
                : (Vector3Int?)null;

            bool haveTurretCell = false;
            foreach (Vector3Int cell in HexGridManager.Instance.GetAllTileCells())
            {
                if (HexGridManager.Instance.IsOccupied(cell) || cell == villageCell)
                {
                    continue;
                }

                _turretCell = cell;
                haveTurretCell = true;
                break;
            }

            if (!haveTurretCell)
            {
                return;
            }

            var excluded = new HashSet<Vector3Int> { _turretCell };
            if (villageCell.HasValue)
            {
                excluded.Add(villageCell.Value);
            }

            foreach (Vector3Int neighbor in HexGridManager.Instance.GetNeighbors(_turretCell))
            {
                excluded.Add(neighbor);
            }

            foreach (Vector3Int cell in HexGridManager.Instance.GetAllTileCells())
            {
                if (HexGridManager.Instance.IsOccupied(cell) || excluded.Contains(cell))
                {
                    continue;
                }

                _tileHighlightCell = cell;
                break;
            }
        }

        /// <summary>A tile's center-to-vertex radius, derived from the grid's actual configured spacing rather than a hardcoded constant (see HexGridManager.HexStepWorldDistance's doc comment: adjacent-cell center distance = sqrt(3) * this radius for a flat-top hex).</summary>
        private float HexVertexRadius => HexGridManager.Instance != null ? HexGridManager.Instance.HexStepWorldDistance / 1.7320508f : 1f;

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

                    if (HexGridManager.Instance != null)
                    {
                        _worldHighlight.ShowHex(HexGridManager.Instance.CellToWorld(_turretCell), HexVertexRadius);
                    }

                    break;

                case Step.WatchWave:
                    TutorialGate.RestrictedHotbarItem = null;
                    TutorialGate.RestrictedPlacementCell = null;
                    _highlight.Hide();
                    _worldHighlight.Hide();
                    ShowBanner("Kijk hoe wave 1 verloopt - let op de schade-cijfers die verschijnen als je turret raak schiet.");
                    if (WaveManager.Instance != null)
                    {
                        WaveManager.Instance.HoldFirstWave = false;
                    }

                    // This step isn't gated on a click - the wave just plays
                    // out over however long it takes - so the banner would
                    // otherwise sit on screen the whole time. Read it, then
                    // get out of the way of the actual fight.
                    StartCoroutine(HideWatchWaveBannerAfterDelay());
                    break;

                case Step.RewardChoice:
                    ShowBanner("Elke wave kies je hierboven: munten (direct te besteden) of een gratis hex-tegel (permanent erbij). Klik daarna op HERVAT om verder te gaan - dit kun je automatiseren via het menu > Instellingen > 'Volgende wave automatisch starten'.");
                    break;

                case Step.ClickTile:
                    ShowBanner("Klik op de gemarkeerde lege hex-tegel om 'm te upgraden.");
                    if (HexGridManager.Instance != null)
                    {
                        _worldHighlight.ShowHex(HexGridManager.Instance.CellToWorld(_tileHighlightCell), HexVertexRadius);
                    }

                    break;

                case Step.TileUpgradeShown:
                    _worldHighlight.Hide();
                    ShowBanner("Hier koop je met munten upgrades voor de tegel - ze gelden voor de turret die erop staat (of ooit komt te staan). Sluit 'm maar weer.");
                    break;

                case Step.ClickTurret:
                    ShowBanner("Klik nu op je eigen turret om 'm te bekijken en te upgraden.");
                    if (_placedTurret != null)
                    {
                        // Traces the turret's own PolygonCollider2D - the
                        // exact pixel-perfect shape you have to click inside
                        // of - rather than an approximate box or circle.
                        _worldHighlight.ShowPolygon(_placedTurret.GetComponent<PolygonCollider2D>());
                    }

                    break;

                case Step.TurretUpgradeShown:
                    _worldHighlight.Hide();
                    ShowBanner("Hier koop je met munten upgrades voor de turret zelf - kies pad A of pad B. Sluit 'm maar weer.");
                    break;

                case Step.Wrapup:
                    HideModal();
                    _highlight.Hide();
                    _worldHighlight.Hide();
                    ShowModalCard(
                        "Dat is de basis! Deze tutorial duurt nog tot wave 5 - experimenteer gerust verder, munten en XP tellen gewoon mee. Veel succes!",
                        "START!");
                    break;

                case Step.Done:
                    HideModal();
                    _highlight.Hide();
                    _worldHighlight.Hide();
                    break;
            }
        }

        private void ShowModalCard(string message, string buttonLabel)
        {
            _hintPanel.SetActive(true);
            _hintPanelBackground.raycastTarget = true;
            _hintHeading.SetActive(true);
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
            _hintHeading.SetActive(false);
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

        private IEnumerator HideWatchWaveBannerAfterDelay()
        {
            yield return new WaitForSeconds(2.5f);
            if (_step == Step.WatchWave)
            {
                HideModal();
            }
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
                EnterStep(Step.RewardChoice);
            }

            if (waveNumber >= TutorialWaveCount)
            {
                _completePanel.SetActive(true);
            }
        }

        /// <summary>Fires once the player has picked (or skipped) the coins-vs-tile reward for wave 1 - see WaveChoiceUI.OnResolved.</summary>
        private void HandleRewardResolved()
        {
            if (_step == Step.RewardChoice)
            {
                EnterStep(Step.ClickTile);
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
