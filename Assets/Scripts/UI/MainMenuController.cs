using Game.Economy;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// Main menu: shows the player's meta XP and starts a run. Upgrade
    /// purchases are handled by the individual UpgradeRow entries.
    /// </summary>
    public class MainMenuController : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Text _xpText;
        [SerializeField] private string _gameSceneName = "SampleScene";

        private void OnEnable()
        {
            _playButton.onClick.AddListener(Play);
        }

        private void OnDisable()
        {
            _playButton.onClick.RemoveListener(Play);
        }

        private void Start()
        {
            if (XPWallet.Instance != null)
            {
                XPWallet.Instance.OnXPChanged += HandleXPChanged;
                HandleXPChanged(XPWallet.Instance.XP);
            }
        }

        private void HandleXPChanged(int xp)
        {
            if (_xpText != null)
            {
                _xpText.text = $"XP: {xp}";
            }
        }

        private void Play()
        {
            SceneManager.LoadScene(_gameSceneName);
        }
    }
}
