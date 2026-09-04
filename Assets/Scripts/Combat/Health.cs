using System;
using UnityEngine;

namespace Game.Combat
{
    /// <summary>
    /// Generic damageable health pool, shared by the village and every
    /// building/enemy that can take damage.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [SerializeField] private int _maxHealth = 1;
        [SerializeField] private DamageNumber _damageNumberPrefab;

        public int MaxHealth => _maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead => CurrentHealth <= 0;

        public event Action<int> OnDamaged;
        public event Action OnDeath;

        private void Awake()
        {
            CurrentHealth = _maxHealth;
        }

        public void SetMaxHealth(int maxHealth)
        {
            _maxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnDamaged?.Invoke(amount);
            SpawnDamageNumber(amount);

            if (IsDead)
            {
                OnDeath?.Invoke();
            }
        }

        private void SpawnDamageNumber(int amount)
        {
            if (_damageNumberPrefab == null)
            {
                return;
            }

            DamageNumber number = Instantiate(_damageNumberPrefab, transform.position, Quaternion.identity);
            number.SetValue(amount);
        }
    }
}
