using System;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// What a single upgrade node does. Flat stat boosts (Damage/Range/FireRate)
    /// stack additively across every reached tier; the rest are on/off
    /// combat behaviors that turn on once their tier is reached.
    /// </summary>
    public enum UpgradeEffect
    {
        Damage,
        Range,
        FireRate,
        SpreadShot,
        SequentialDoubleShot,
        FireDamage,
        PiercingShot,
        MultiTargetShot,
        SplashDamage,
    }

    /// <summary>
    /// One tier of an upgrade path: what it does, how much XP it costs to
    /// unlock permanently in the main menu, and how many coins it costs to
    /// activate on a building type during a run once unlocked.
    /// </summary>
    [Serializable]
    public class UpgradeNode
    {
        [SerializeField] private string _name;
        [SerializeField] private UpgradeEffect _effect;
        [SerializeField] private float _value;
        [SerializeField] private int _unlockCost;
        [SerializeField] private int _applyCost;

        public string Name => _name;
        public UpgradeEffect Effect => _effect;

        /// <summary>Magnitude of the effect - damage/range/fire-rate bonus, pierce count, burn DPS, etc. depending on Effect.</summary>
        public float Value => _value;

        /// <summary>Permanent XP cost to unlock this tier in the main menu shop.</summary>
        public int UnlockCost => _unlockCost;

        /// <summary>In-run coin cost to activate this tier on a building type, once unlocked.</summary>
        public int ApplyCost => _applyCost;
    }

    /// <summary>Exactly three sequential upgrade tiers forming one path.</summary>
    [Serializable]
    public class UpgradePath
    {
        [SerializeField] private UpgradeNode[] _nodes = new UpgradeNode[3];

        public UpgradeNode[] Nodes => _nodes;
    }
}
