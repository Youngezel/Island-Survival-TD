using Game.Combat;
using Game.Data;
using Game.Grid;
using UnityEngine;

namespace Game.Buildings
{
    /// <summary>
    /// A turret placed on a hex tile. Automatically targets and damages the
    /// nearest enemy in range at its fire rate.
    /// </summary>
    [RequireComponent(typeof(Health), typeof(Targeting), typeof(Shooter))]
    public class Building : MonoBehaviour
    {
        [SerializeField] private BuildingData _data;

        private void Awake()
        {
            Health health = GetComponent<Health>();
            if (_data != null)
            {
                health.SetMaxHealth(_data.MaxHealth);
            }

            GetComponent<Shooter>().Initialize(_data);
        }

        private void Start()
        {
            if (HexGridManager.Instance != null)
            {
                Vector3Int cell = HexGridManager.Instance.WorldToCell(transform.position);
                HexGridManager.Instance.SetOccupied(cell, true);
            }
        }
    }
}
