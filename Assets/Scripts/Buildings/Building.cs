using System.Collections.Generic;
using Game.Combat;
using Game.Data;
using Game.Grid;
using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// A turret placed on a hex tile. Automatically targets and damages the
    /// nearest enemy in range at its fire rate, boosted by whatever run-only
    /// upgrade path this building type has committed to (see Shooter).
    /// Registers itself so enemies can find and attack it if it stands in
    /// their way to the village, and so BuildingClickController can find it
    /// under the cursor to open the building inspector (view stats,
    /// pick/advance an upgrade path, or sell it back).
    /// </summary>
    [RequireComponent(typeof(Health), typeof(Targeting), typeof(Shooter))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class Building : MonoBehaviour
    {
        public static readonly List<Building> ActiveBuildings = new List<Building>();

        [SerializeField] private BuildingData _data;

        private Health _health;
        private Shooter _shooter;
        private Vector3Int _cell;

        public BuildingData Data => _data;
        public Shooter Shooter => _shooter;
        public Health Health => _health;

        private void Awake()
        {
            _health = GetComponent<Health>();
            _shooter = GetComponent<Shooter>();
            if (_data != null)
            {
                _health.SetMaxHealth(_data.MaxHealth);
            }

            _shooter.Initialize(_data);
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

        /// <summary>Removes this building via a player-initiated sale, freeing its tile - as opposed to dying to enemy damage.</summary>
        public void Sell()
        {
            RemoveFromGrid();
        }

        private void HandleDeath()
        {
            RemoveFromGrid();
        }

        private void RemoveFromGrid()
        {
            if (HexGridManager.Instance != null)
            {
                HexGridManager.Instance.SetOccupied(_cell, false);
            }

            Destroy(gameObject);
        }
    }
}
