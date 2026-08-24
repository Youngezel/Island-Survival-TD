using System.Collections.Generic;
using Game.Combat;
using Game.Data;
using Game.Grid;
using Game.Systems;
using Game.UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Game.Buildings
{
    /// <summary>
    /// A turret placed on a hex tile. Automatically targets and damages the
    /// nearest enemy in range at its fire rate, boosted by its saved upgrade
    /// level if it has one. Registers itself so enemies can find and attack
    /// it if it stands in their way to the village. Clicking it (when not
    /// mid-placement) opens the building inspector for run-only upgrades.
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

            _shooter.Initialize(_data, GetUpgradeLevel());
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

        private void OnMouseDown()
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }

            if (PlacementCursor.Instance != null && PlacementCursor.Instance.SelectedItem != null)
            {
                return;
            }

            if (_data == null || string.IsNullOrEmpty(_data.UpgradeSaveKey))
            {
                return;
            }

            BuildingInspectorUI.Instance?.Open(_data, this);
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
