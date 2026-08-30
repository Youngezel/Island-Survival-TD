using Game.Systems;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>HUD button that cycles the game speed (1x/2x) via GameSpeedController and shows the current multiplier.</summary>
    public class SpeedToggleButton : MonoBehaviour
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

        private void Start()
        {
            Refresh();
        }

        private void HandleClick()
        {
            if (GameSpeedController.Instance == null)
            {
                return;
            }

            GameSpeedController.Instance.CycleSpeed();
            Refresh();
        }

        private void Refresh()
        {
            if (_label != null && GameSpeedController.Instance != null)
            {
                _label.text = $"{GameSpeedController.Instance.CurrentSpeed:0.#}x";
            }
        }
    }
}
