using System;
using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// What a single upgrade node does. Flat stat boosts (Damage/Range/
    /// FireRate/PiercingShot/FireDamage/SplashDamage/ChainLightning) stack
    /// additively across every reached tier. SpreadShot/SequentialDoubleShot
    /// are mutually-exclusive firing patterns - only the highest reached
    /// tier's pattern is active. MultiTargetShot instead sets the building's
    /// simultaneous-target count directly to its Value (not additive) - the
    /// highest reached tier with this effect wins. Health is a one-shot
    /// effect applied the instant a tier is bought (adding to both max and
    /// current HP right then) rather than something recomputed every frame
    /// like the others.
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

        /// <summary>Adds to how many enemies a bolt jumps to after its initial hit (ricochet/chain lightning) - stacks across tiers like PiercingShot.</summary>
        ChainLightning,

        /// <summary>Permanently raises max HP by Value the instant this tier is purchased - see the Health effect note above.</summary>
        Health,
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
