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
        private int _upgradeLevel;
        private float _cooldown;

        private void Awake()
        {
            _targeting = GetComponent<Targeting>();
        }

        public void Initialize(BuildingData data, int upgradeLevel = 0)
        {
            _data = data;
            _upgradeLevel = upgradeLevel;
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
            int damage = _data.Damage + _data.DamagePerUpgradeLevel * _upgradeLevel;
            projectile.Initialize(target, _data.ProjectileSpeed, damage, _data.Splash, splashRadiusWorldUnits);
        }
    }
}
