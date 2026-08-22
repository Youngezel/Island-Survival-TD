using System.Collections.Generic;
using Game.Combat;
using Game.Data;
using Game.Grid;
using Game.Systems;
using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// A turret placed on a hex tile. Automatically targets and damages the
    /// nearest enemy in range at its fire rate, boosted by its saved upgrade
    /// level if it has one. Registers itself so enemies can find and attack
    /// it if it stands in their way to the village.
    /// </summary>
    [RequireComponent(typeof(Health), typeof(Targeting), typeof(Shooter))]
    public class Building : MonoBehaviour
    {
        public static readonly List<Building> ActiveBuildings = new List<Building>();

        [SerializeField] private BuildingData _data;

        private Health _health;
        private Vector3Int _cell;

        private void Awake()
        {
            _health = GetComponent<Health>();
            if (_data != null)
            {
                _health.SetMaxHealth(_data.MaxHealth);
            }

            GetComponent<Shooter>().Initialize(_data, GetUpgradeLevel());
        }

        private void Start()
        {
            if (HexGridManager.Instance != null)
            {
                _cell = HexGridManager.Instance.WorldToCell(transform.position);
                HexGridManager.Instance.SetOccupied(_cell, true);
            }
        }

        private void OnEnable()
        {
            ActiveBuildings.Add(this);
            _health.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            ActiveBuildings.Remove(this);
            _health.OnDeath -= HandleDeath;
        }

        private int GetUpgradeLevel()
        {
            if (_data == null || string.IsNullOrEmpty(_data.UpgradeSaveKey) || SaveManager.Instance == null)
            {
                return 0;
            }

            return SaveManager.Instance.GetUpgradeLevel(_data.UpgradeSaveKey);
        }

        private void HandleDeath()
        {
            if (HexGridManager.Instance != null)
            {
                HexGridManager.Instance.SetOccupied(_cell, false);
            }

            Destroy(gameObject);
        }
    }
}
