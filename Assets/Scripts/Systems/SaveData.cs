using System;

namespace Game.Systems
{
    /// <summary>
    /// Everything persisted between runs: meta XP and, per building type, how
    /// many tiers of each upgrade path have been permanently unlocked with
    /// that XP. Fixed fields rather than a dictionary, since JsonUtility
    /// cannot serialize dictionaries and there are only three upgradeable
    /// building types right now. Unlocking a tier here only makes it
    /// available to pick from during a run - it doesn't apply itself;
    /// RunUpgradeManager tracks which path (if any) a run has committed to
    /// and how far into it.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int xp;
        public int bestWave;

        public int turretPathATier;
        public int turretPathBTier;
        public int longRangeTurretPathATier;
        public int longRangeTurretPathBTier;
        public int mortarPathATier;
        public int mortarPathBTier;
    }
}
