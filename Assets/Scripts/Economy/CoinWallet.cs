using System;
using UnityEngine;

namespace Game.Economy
{
    /// <summary>
    /// In-run coin balance. The HUD hooks into this later (section 7.9/7.10);
    /// for now it just holds the total and notifies listeners when it changes.
    /// </summary>
    public class CoinWallet : MonoBehaviour
    {
        public static CoinWallet Instance { get; private set; }

        [SerializeField] private int _coins;

        public int Coins => _coins;
        public event Action<int> OnCoinsChanged;

        private void Awake()
        {
            Instance = this;
        }

        public void AddCoins(int amount)
        {
            if (amount == 0)
            {
                return;
            }

            _coins += amount;
            OnCoinsChanged?.Invoke(_coins);
        }

        /// <summary>Deducts amount if affordable. Returns whether the spend succeeded.</summary>
        public bool TrySpend(int amount)
        {
            if (amount < 0 || amount > _coins)
            {
                return false;
            }

            _coins -= amount;
            OnCoinsChanged?.Invoke(_coins);
            return true;
        }
    }
}
