using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// In-run-only upgrade levels, one per building type (keyed by
    /// BuildingData.UpgradeSaveKey), bought with coins either from a placed
    /// turret's inspector or straight from its hotbar slot. Applies to every
    /// turret of that type - already placed and future - unlike the
    /// permanent per-type levels in SaveManager, which are bought with XP
    /// in the main menu and survive between runs. Resets naturally with the
    /// scene, like CoinWallet.
    /// </summary>
    public class RunUpgradeManager : MonoBehaviour
    {
        public static RunUpgradeManager Instance { get; private set; }

        private readonly Dictionary<string, int> _levels = new Dictionary<string, int>();

        public event Action<string> OnLevelChanged;

        private void Awake()
        {
            Instance = this;
        }

        public int GetLevel(string upgradeSaveKey)
        {
            if (string.IsNullOrEmpty(upgradeSaveKey))
            {
                return 0;
            }

            return _levels.TryGetValue(upgradeSaveKey, out int level) ? level : 0;
        }

        public void AddLevel(string upgradeSaveKey)
        {
            if (string.IsNullOrEmpty(upgradeSaveKey))
            {
                return;
            }

            _levels[upgradeSaveKey] = GetLevel(upgradeSaveKey) + 1;
            OnLevelChanged?.Invoke(upgradeSaveKey);
        }
    }
}
