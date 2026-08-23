using Game.Economy;
using Game.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Main menu: shows the player's meta XP and best wave, and starts a
    /// run. Upgrade purchases are handled by the individual UpgradeRow
    /// entries. Settings has no screen yet, so that button is a placeholder.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private TMP_Text _xpAmountText;
        [SerializeField] private TMP_Text _bestWaveText;
        [SerializeField] private string _gameSceneName = "SampleScene";

        private void OnEnable()
        {
            _playButton.onClick.AddListener(Play);
            _quitButton.onClick.AddListener(Quit);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(Play);
            _quitButton.onClick.RemoveListener(Quit);

            if (XPWallet.Instance != null)
            {
                XPWallet.Instance.OnXPChanged -= HandleXPChanged;
            }
        }

        private void Start()
        {
            // Waits for Start (called after every object's Awake) rather than
            // subscribing in OnEnable, since XPWallet may not have set its
            // static Instance yet if this controller's OnEnable runs first.
            if (XPWallet.Instance != null)
            {
                XPWallet.Instance.OnXPChanged += HandleXPChanged;
                HandleXPChanged(XPWallet.Instance.XP);
            }

            if (_bestWaveText != null && SaveManager.Instance != null)
            {
                _bestWaveText.text = $"WAVE {SaveManager.Instance.Current.bestWave}";
            }
        }

        private void HandleXPChanged(int xp)
        {
            if (_xpAmountText != null)
            {
                _xpAmountText.text = xp.ToString();
            }
        }

        private void Play()
        {
            SceneManager.LoadScene(_gameSceneName);
        }

        private void Quit()
        {
            Application.Quit();
        }
    }
}
