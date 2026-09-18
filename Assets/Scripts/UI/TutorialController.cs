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
    /// Tutorial button. Shows a short intro hint sequence (possibilities
    /// overview, tile-click, turret-click, wrap-up) that holds wave 1 back
    /// until dismissed, then lets the run play out normally through
    /// WaveChoiceUI - except once TutorialWaveCount clears, this takes over
    /// instead of offering a next wave (WaveChoiceUI knows to stay out of
    /// the way past that point) and shows a completion screen back to the
    /// main menu. Entirely inert if GameSettings.IsTutorial wasn't set
    /// before this scene loaded, i.e. every normal run.
    /// </summary>
    public class TutorialController : MonoBehaviour
    {
        public const int TutorialWaveCount = 5;

        [SerializeField] private GameObject _hintPanel;
        [SerializeField] private TMP_Text _hintProgressText;
        [SerializeField] private TMP_Text _hintBodyText;
        [SerializeField] private Button _hintNextButton;
        [SerializeField] private TMP_Text _hintNextButtonText;

        [SerializeField] private GameObject _completePanel;
        [SerializeField] private Button _completeMainMenuButton;

        [SerializeField] private string _mainMenuSceneName = "MainMenu";

        [TextArea]
        [SerializeField]
        private string[] _hintMessages =
        {
            "Welkom bij Island Survival TD! Verdedig je dorp tegen golven piraten. Gebruik de hotbar onderin om hex-tegels en turrets neer te zetten - hoe meer tegels, hoe groter je eiland.",
            "Klik op een lege hex-tegel om 'm te upgraden: geef de turret die er ooit op komt te staan extra health, damage of range. Dit staat los van de turret zelf.",
            "Klik op een geplaatste turret om 'm te bekijken en te upgraden met munten - kies pad A of B. Nieuwe tiers ontgrendel je permanent met XP in het hoofdmenu.",
            "Deze tutorial duurt 5 golven. Experimenteer gerust - munten en XP uit deze run tellen gewoon mee. Veel succes!",
        };

        private int _hintIndex;

        private void Awake()
        {
            // Every normal run leaves this component fully inert - it never
            // even subscribes to anything - so it can sit on the same
            // always-present Canvas object as the other run-wide UI
            // controllers without any cost or risk outside a tutorial run.
            if (!GameSettings.IsTutorial)
            {
                enabled = false;
            }
        }

        private void OnEnable()
        {
            _hintNextButton.onClick.AddListener(AdvanceHint);
            _completeMainMenuButton.onClick.AddListener(BackToMainMenu);
            WaveManager.OnWaveCleared += HandleWaveCleared;
        }

        private void OnDisable()
        {
            _hintNextButton.onClick.RemoveListener(AdvanceHint);
            _completeMainMenuButton.onClick.RemoveListener(BackToMainMenu);
            WaveManager.OnWaveCleared -= HandleWaveCleared;
        }

        private void Start()
        {
            if (WaveManager.Instance != null)
            {
                WaveManager.Instance.HoldFirstWave = true;
            }

            _hintIndex = 0;
            ShowCurrentHint();
            _hintPanel.SetActive(true);
        }

        private void ShowCurrentHint()
        {
            _hintProgressText.text = $"{_hintIndex + 1} / {_hintMessages.Length}";
            _hintBodyText.text = _hintMessages[_hintIndex];
            _hintNextButtonText.text = _hintIndex == _hintMessages.Length - 1 ? "START!" : "VOLGENDE";
        }

        /// <summary>Advances to the next hint card, or releases wave 1 once the last one is dismissed.</summary>
        private void AdvanceHint()
        {
            _hintIndex++;
            if (_hintIndex >= _hintMessages.Length)
            {
                _hintPanel.SetActive(false);
                if (WaveManager.Instance != null)
                {
                    WaveManager.Instance.HoldFirstWave = false;
                }

                return;
            }

            ShowCurrentHint();
        }

        /// <summary>Once the tutorial's wave cap clears, shows the completion screen instead of letting WaveChoiceUI offer another wave.</summary>
        private void HandleWaveCleared(int waveNumber)
        {
            if (waveNumber < TutorialWaveCount)
            {
                return;
            }

            _completePanel.SetActive(true);
        }

        private void BackToMainMenu()
        {
            GameSettings.IsTutorial = false;
            Time.timeScale = 1f;
            SceneManager.LoadScene(_mainMenuSceneName);
        }
    }
}
