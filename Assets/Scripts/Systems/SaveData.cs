using System;

namespace Game.Systems
{
    /// <summary>
    /// Everything persisted between runs: meta XP and per-building-type
    /// upgrade levels. Fixed fields rather than a dictionary, since
    /// JsonUtility cannot serialize dictionaries and there are only three
    /// upgradeable building types right now.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int xp;
        public int turretUpgradeLevel;
        public int longRangeTurretUpgradeLevel;
        public int mortarUpgradeLevel;
        public int bestWave;
    }
}
