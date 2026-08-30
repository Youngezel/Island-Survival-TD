using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>HUD button next to the speed toggle that pauses/resumes the game via PauseController, so building/upgrading is reachable without the keyboard.</summary>
    public class PauseToggleButton : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private TMP_Text _label;

        private void OnEnable()
        {
            _button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            PauseController.Instance?.TogglePause();
        }

        private void Update()
        {
            if (_label != null && PauseController.Instance != null)
            {
                _label.text = PauseController.Instance.IsPaused ? "HERVAT" : "PAUZE";
            }
        }
    }
}
