using Game.Combat;
using Game.Data;
using Game.Enemies;
using Game.Grid;
using Game.Systems;
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
        private float _cooldown;

        public BuildingData Data => _data;

        /// <summary>
        /// Run-only upgrade level for this building's type, bought in-game
        /// via the hotbar or a placed turret's inspector - shared by every
        /// turret of that type this run, separate from the permanent
        /// main-menu level.
        /// </summary>
        public int RunUpgradeLevel => RunUpgradeManager.Instance != null && _data != null ? RunUpgradeManager.Instance.GetLevel(_data.UpgradeSaveKey) : 0;

        /// <summary>Current damage per shot, including both the permanent (main menu) and run-only (in-game) upgrade bonuses.</summary>
        public int CurrentDamage => _data.Damage + _data.DamagePerUpgradeLevel * (_permanentUpgradeLevel + RunUpgradeLevel);

        private void Awake()
        {
            _targeting = GetComponent<Targeting>();
        }

        public void Initialize(BuildingData data, int permanentUpgradeLevel = 0)
        {
            _data = data;
            _permanentUpgradeLevel = permanentUpgradeLevel;
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
