using Game.Combat;
using Game.Data;
using Game.Enemies;
using Game.Grid;
using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// Automatically fires a projectile at the nearest enemy in range, at a
    /// fixed fire rate. Shared by the village and every turret.
    /// </summary>
    [RequireComponent(typeof(Targeting))]
    public class Shooter : MonoBehaviour
    {
        [SerializeField] private Projectile _projectilePrefab;

        private Targeting _targeting;
        private BuildingData _data;
        private int _permanentUpgradeLevel;
        private int _runUpgradeLevel;
        private float _cooldown;

        public BuildingData Data => _data;

        /// <summary>Run-only upgrade level bought in-game via the building inspector; resets every run, separate from the permanent main-menu level.</summary>
        public int RunUpgradeLevel => _runUpgradeLevel;

        /// <summary>Current damage per shot, including both the permanent (main menu) and run-only (in-game) upgrade bonuses.</summary>
        public int CurrentDamage => _data.Damage + _data.DamagePerUpgradeLevel * (_permanentUpgradeLevel + _runUpgradeLevel);

        private void Awake()
        {
            _targeting = GetComponent<Targeting>();
        }

        public void Initialize(BuildingData data, int permanentUpgradeLevel = 0)
        {
            _data = data;
            _permanentUpgradeLevel = permanentUpgradeLevel;
        }

        /// <summary>Bumps the run-only upgrade level by one; called after a successful in-game upgrade purchase.</summary>
        public void AddRunUpgradeLevel()
        {
            _runUpgradeLevel++;
        }

        private void Update()
        {
            if (_data == null || HexGridManager.Instance == null)
            {
                return;
            }

            _cooldown -= Time.deltaTime;

            float rangeWorldUnits = _data.Range * HexGridManager.Instance.HexStepWorldDistance;
            Enemy target = _targeting.FindNearestEnemyInRange(rangeWorldUnits);

            if (target != null && _cooldown <= 0f)
            {
                Fire(target);
                _cooldown = 1f / _data.FireRate;
            }
        }

        private void Fire(Enemy target)
        {
            if (_projectilePrefab == null)
            {
                return;
            }

            Projectile projectile = Instantiate(_projectilePrefab, transform.position, Quaternion.identity);
            float splashRadiusWorldUnits = _data.SplashRadius * HexGridManager.Instance.HexStepWorldDistance;
            projectile.Initialize(target, _data.ProjectileSpeed, CurrentDamage, _data.Splash, splashRadiusWorldUnits);
        }
    }
}
