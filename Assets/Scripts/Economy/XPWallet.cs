using System;
using Game.Systems;
using UnityEngine;

namespace Game.Economy
{
    /// <summary>
    /// Meta-progression currency, earned on game over and spent on building
    /// upgrades in the main menu. Starts from whatever SaveManager loaded
    /// from disk, so it persists across restarts.
    /// </summary>
    public class XPWallet : MonoBehaviour
    {
        public static XPWallet Instance { get; private set; }

        [SerializeField] private int _xp;

        public int XP => _xp;
        public event Action<int> OnXPChanged;

        private void Awake()
        {
            Instance = this;
            if (SaveManager.Instance != null)
            {
                _xp = SaveManager.Instance.Current.xp;
            }
        }

        public void AddXP(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _xp += amount;
            OnXPChanged?.Invoke(_xp);
        }

        /// <summary>Deducts amount if affordable and persists it immediately. Returns whether the spend succeeded.</summary>
        public bool TrySpend(int amount)
        {
            if (amount < 0 || amount > _xp)
            {
                return false;
            }

            _xp -= amount;
            OnXPChanged?.Invoke(_xp);

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Current.xp = _xp;
                SaveManager.Instance.Save();
            }

            return true;
        }
    }
}
