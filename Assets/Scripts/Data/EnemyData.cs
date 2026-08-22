using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// Stats for one enemy type. A new enemy is a new asset, not a new class.
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Island Survival TD/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private int _maxHealth;
        [SerializeField] private int _damage;
        [SerializeField] private int _coinReward;
        [SerializeField] private float _moveSpeed;
        [SerializeField] private float _range;
        [SerializeField] private float _attackRate;
        [SerializeField] private float _projectileSpeed = 10f;

        public string DisplayName => _displayName;
        public int MaxHealth => _maxHealth;
        public int Damage => _damage;
        public int CoinReward => _coinReward;
        public float MoveSpeed => _moveSpeed;
        public float Range => _range;
        public float AttackRate => _attackRate;

        /// <summary>World units per second the fired projectile travels at.</summary>
        public float ProjectileSpeed => _projectileSpeed;
    }
}
