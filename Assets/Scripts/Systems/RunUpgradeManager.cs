using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems
{
    /// <summary>
    /// In-run-only upgrade progress, one per building type (keyed by
    /// BuildingData.UpgradeSaveKey): which of its two upgrade paths (if any)
    /// this run has committed to, and how many tiers of it are active.
    /// Bought with coins either from a placed turret's inspector or straight
    /// from its hotbar slot; applies to every turret of that type - already
    /// placed and future. Separate from the permanent per-type unlocked
    /// tiers in SaveManager, which are bought with XP in the main menu and
    /// survive between runs - a tier can only be activated here once it's
    /// unlocked there. Resets naturally with the scene, like CoinWallet.
    /// </summary>
    public class RunUpgradeManager : MonoBehaviour
    {
        public static RunUpgradeManager Instance { get; private set; }

        private class TypeState
        {
            public bool PathA;
            public int Tier;
        }

        private readonly Dictionary<string, TypeState> _states = new Dictionary<string, TypeState>();

        public event Action<string> OnChanged;

        private void Awake()
        {
            Instance = this;
        }

        /// <summary>True once this building type has committed to a path this run.</summary>
        public bool HasCommittedPath(string upgradeSaveKey)
        {
            return _states.ContainsKey(upgradeSaveKey);
        }

        /// <summary>Which path is committed; only meaningful when HasCommittedPath is true.</summary>
        public bool IsPathA(string upgradeSaveKey)
        {
            return _states.TryGetValue(upgradeSaveKey, out TypeState state) && state.PathA;
        }

        /// <summary>How many tiers (0-3) of the committed path are active this run.</summary>
        public int GetTier(string upgradeSaveKey)
        {
            return _states.TryGetValue(upgradeSaveKey, out TypeState state) ? state.Tier : 0;
        }

        /// <summary>
        /// Activates the next tier of the given path for this building type,
        /// committing to that path if none is committed yet. Returns false
        /// without changing anything if the other path is already
        /// committed, or this path is already fully activated.
        /// </summary>
        public bool TryActivateNextTier(string upgradeSaveKey, bool pathA)
        {
            if (!_states.TryGetValue(upgradeSaveKey, out TypeState state))
            {
                state = new TypeState { PathA = pathA, Tier = 0 };
                _states[upgradeSaveKey] = state;
            }
            else if (state.PathA != pathA)
            {
                return false;
            }

            if (state.Tier >= 3)
            {
                return false;
            }

            state.Tier++;
            OnChanged?.Invoke(upgradeSaveKey);
            return true;
        }
    }
}
