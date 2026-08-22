using Game.Data;
using Game.Economy;
using Game.Grid;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// Validates and executes placing a hotbar item (a building, or a new hex
    /// tile) on the map: enough coins, and a free/appropriate target cell.
    /// </summary>
    public class BuildPlacer : MonoBehaviour
    {
        public static BuildPlacer Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public bool TryPlace(HotbarItemData item, Vector3Int cell, bool free = false)
        {
            if (item == null || HexGridManager.Instance == null || CoinWallet.Instance == null)
            {
                return false;
            }

            bool hasTile = HexGridManager.Instance.HasGroundTile(cell);

            if (item.IsGroundTile)
            {
                if (hasTile || !HasAdjacentTile(cell))
                {
                    return false;
                }
            }
            else if (!hasTile || HexGridManager.Instance.IsOccupied(cell))
            {
                return false;
            }

            if (!free && !CoinWallet.Instance.TrySpend(item.Cost))
            {
                return false;
            }

            if (item.IsGroundTile)
            {
                HexGridManager.Instance.PlaceGroundTile(cell);
            }
            else
            {
                Vector3 worldPosition = HexGridManager.Instance.CellToWorld(cell);
                Instantiate(item.BuildingPrefab, worldPosition, Quaternion.identity);

                // Mark occupied immediately rather than relying on the
                // building's own Start() (deferred to next frame), so two
                // placements attempted back-to-back can't both succeed on
                // the same cell.
                HexGridManager.Instance.SetOccupied(cell, true);
            }

            return true;
        }

        private bool HasAdjacentTile(Vector3Int cell)
        {
            foreach (Vector3Int neighbor in HexGridManager.Instance.GetNeighbors(cell))
            {
                if (HexGridManager.Instance.HasGroundTile(neighbor))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
