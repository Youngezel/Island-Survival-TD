using Game.Buildings;
using Game.Combat;
using Game.Data;
using Game.Economy;
using Game.Grid;
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Sails toward the village and attacks once within range. Ranged
    /// enemies simply have a larger range value and stop farther away -
    /// no special-case behavior needed, it falls out of the shared logic.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        [SerializeField] private EnemyData _data;

        private Health _health;
        private Health _targetHealth;
        private Transform _target;
        private float _attackCooldown;

        private void Awake()
        {
            _health = GetComponent<Health>();
            if (_data != null)
            {
                _health.SetMaxHealth(_data.MaxHealth);
            }
        }

        private void Start()
        {
            if (Village.Instance != null)
            {
                _target = Village.Instance.transform;
                _targetHealth = Village.Instance.GetComponent<Health>();
            }
        }

        private void OnEnable()
        {
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            _health.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            if (_target == null || _data == null || HexGridManager.Instance == null)
            {
                return;
            }

            Vector3 toTarget = _target.position - transform.position;
            float rangeWorldUnits = _data.Range * HexGridManager.Instance.HexStepWorldDistance;

            if (toTarget.sqrMagnitude > rangeWorldUnits * rangeWorldUnits)
            {
                Vector3 direction = toTarget.normalized;
                transform.position += direction * _data.MoveSpeed * Time.deltaTime;
                return;
            }

            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown <= 0f)
            {
                if (_targetHealth != null)
                {
                    _targetHealth.TakeDamage(_data.Damage);
                }
                _attackCooldown = 1f / _data.AttackRate;
            }
        }

        private void HandleDeath()
        {
            if (_data != null && CoinWallet.Instance != null)
            {
                CoinWallet.Instance.AddCoins(_data.CoinReward);
            }
            Destroy(gameObject);
        }
    }
}
