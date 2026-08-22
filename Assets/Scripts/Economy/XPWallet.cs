using System;
using UnityEngine;

namespace Game.Economy
{
    /// <summary>
    /// Meta-progression currency, earned on game over and spent on building
    /// upgrades in the main menu. Persisting it across runs is section 7.11's
    /// job; this just holds the total for the current session.
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
    }
}
