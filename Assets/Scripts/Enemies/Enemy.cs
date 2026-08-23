using System.Collections.Generic;
using Game.Buildings;
using Game.Combat;
using Game.Data;
using Game.Economy;
using Game.Grid;
using UnityEngine;

namespace Game.Enemies
{
    /// <summary>
    /// Sails toward the village but attacks whatever damageable thing is
    /// nearest first - a building or a hex tile in its way - and only
    /// engages the village once nothing closer is left standing. Re-picks
    /// its target every frame, so once a tile/building it was attacking is
    /// destroyed it naturally moves on to the next-nearest obstacle. Ranged
    /// enemies fall out of the same logic: a larger range just lets them
    /// hit an obstacle from farther away without needing to reach it.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Enemy : MonoBehaviour
    {
        public static readonly List<Enemy> ActiveEnemies = new List<Enemy>();

        private enum TargetKind
        {
            None,
            Village,
            Building,
            Tile,
        }

        [SerializeField] private EnemyData _data;
        [SerializeField] private Projectile _projectilePrefab;

        private Health _health;
        private float _attackCooldown;

        private TargetKind _targetKind;
        private Health _targetHealth;
        private Vector3 _targetPosition;
        private Vector3Int _targetCell;

        public bool IsDead => _health.IsDead;

        private void Awake()
        {
            _health = GetComponent<Health>();
            if (_data != null)
            {
                _health.SetMaxHealth(_data.MaxHealth);
            }
        }

        private void OnEnable()
        {
            ActiveEnemies.Add(this);
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            ActiveEnemies.Remove(this);
            _health.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            if (_data == null || HexGridManager.Instance == null || Village.Instance == null)
            {
                return;
            }

            AcquireNearestTarget();
            if (_targetKind == TargetKind.None)
            {
                return;
            }

            Vector3 toTarget = _targetPosition - transform.position;
            float rangeWorldUnits = _data.Range * HexGridManager.Instance.HexStepWorldDistance;

            if (toTarget.sqrMagnitude > rangeWorldUnits * rangeWorldUnits)
            {
                transform.position += toTarget.normalized * _data.MoveSpeed * Time.deltaTime;
                return;
            }

            _attackCooldown -= Time.deltaTime;
            if (_attackCooldown <= 0f)
            {
                Attack();
                _attackCooldown = 1f / _data.AttackRate;
            }
        }

        /// <summary>Finds the nearest damageable thing: any building, any standing hex tile, or the village itself.</summary>
        private void AcquireNearestTarget()
        {
            _targetKind = TargetKind.None;
            float bestSqrDistance = float.MaxValue;

            Health villageHealth = Village.Instance.GetComponent<Health>();
            if (villageHealth != null && !villageHealth.IsDead)
            {
                float sqrDistance = (Village.Instance.transform.position - transform.position).sqrMagnitude;
                bestSqrDistance = sqrDistance;
                _targetKind = TargetKind.Village;
                _targetHealth = villageHealth;
                _targetPosition = Village.Instance.transform.position;
            }

            foreach (Building building in Building.ActiveBuildings)
            {
                if (building == null)
                {
                    continue;
                }

                Health buildingHealth = building.GetComponent<Health>();
                if (buildingHealth == null || buildingHealth.IsDead)
                {
                    continue;
                }

                float sqrDistance = (building.transform.position - transform.position).sqrMagnitude;
                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    _targetKind = TargetKind.Building;
                    _targetHealth = buildingHealth;
                    _targetPosition = building.transform.position;
                }
            }

            Vector3Int villageCell = HexGridManager.Instance.WorldToCell(Village.Instance.transform.position);
            foreach (Vector3Int cell in HexGridManager.Instance.GetAllTileCells())
            {
                if (cell == villageCell || HexGridManager.Instance.IsOccupied(cell))
                {
                    continue;
                }

                if (HexGridManager.Instance.GetTileHealth(cell) <= 0)
                {
                    continue;
                }

                Vector3 tileWorldPosition = HexGridManager.Instance.CellToWorld(cell);
                float sqrDistance = (tileWorldPosition - transform.position).sqrMagnitude;
                if (sqrDistance < bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    _targetKind = TargetKind.Tile;
                    _targetCell = cell;
                    _targetPosition = tileWorldPosition;
                }
            }
        }

        private void Attack()
        {
            if (_projectilePrefab == null)
            {
                return;
            }

            Projectile projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);

            if (_targetKind == TargetKind.Tile)
            {
                Vector3Int cell = _targetCell;
                projectile.InitializeAtFixedTarget(_targetPosition, _data.ProjectileSpeed, _data.Damage,
                    damage => HexGridManager.Instance.DamageTile(cell, damage));
            }
            else
            {
                Health targetHealth = _targetHealth;
                projectile.InitializeAtFixedTarget(_targetPosition, _data.ProjectileSpeed, _data.Damage,
                    damage =>
                    {
                        if (targetHealth != null && !targetHealth.IsDead)
                        {
                            targetHealth.TakeDamage(damage);
                        }
                    });
            }
        }

        private void HandleDeath()
        {
            if (_data != null && CoinWallet.Instance != null)
            {
                CoinWallet.Instance.AddCoins(_data.CoinReward);
            }

            if (Game.Systems.KillTracker.Instance != null)
            {
                Game.Systems.KillTracker.Instance.RegisterKill();
            }

            Destroy(gameObject);
        }
    }
}
