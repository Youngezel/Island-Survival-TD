using Game.Combat;
using Game.Data;
using Game.Enemies;
using Game.Grid;
using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// Automatically damages the nearest enemy in range at a fixed fire
    /// rate. Shared by the village and every turret. Hit-scan for now
    /// (instant damage, no travel time); a visible traveling projectile
    /// and splash damage for the mortar arrive with the projectile system.
    /// </summary>
    [RequireComponent(typeof(Targeting))]
    public class Shooter : MonoBehaviour
    {
        private Targeting _targeting;
        private BuildingData _data;
        private float _cooldown;

        private void Awake()
        {
            _targeting = GetComponent<Targeting>();
        }

        public void Initialize(BuildingData data)
        {
            _data = data;
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
                target.GetComponent<Health>().TakeDamage(_data.Damage);
                _cooldown = 1f / _data.FireRate;
            }
        }
    }
}
