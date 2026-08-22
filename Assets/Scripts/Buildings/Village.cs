using System;
using Game.Combat;
using Game.Data;
using Game.Grid;
using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// The player's fixed, non-purchasable starting point. Loss condition:
    /// when its health reaches zero, the run ends.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class Village : MonoBehaviour
    {
        public static event Action OnVillageDestroyed;

        [SerializeField] private BuildingData _data;

        private Health _health;

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
            if (HexGridManager.Instance != null)
            {
                Vector3Int cell = HexGridManager.Instance.WorldToCell(transform.position);
                HexGridManager.Instance.SetOccupied(cell, true);
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

        private void HandleDeath()
        {
            OnVillageDestroyed?.Invoke();
        }
    }
}
