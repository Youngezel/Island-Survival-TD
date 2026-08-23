using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// Stats for a building placed on a hex tile - shared by the village and
    /// every turret type. A new building is a new asset, not a new class.
    /// </summary>
    [CreateAssetMenu(fileName = "BuildingData", menuName = "Island Survival TD/Building Data")]
    public class BuildingData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private int _maxHealth;
        [SerializeField] private int _damage;
        [SerializeField] private int _cost;
        [SerializeField] private float _range;
        [SerializeField] private float _fireRate;
        [SerializeField] private bool _splash;
        [SerializeField] private float _splashRadius;
        [SerializeField] private float _projectileSpeed = 10f;
        [SerializeField] private string _upgradeSaveKey;
        [SerializeField] private int _damagePerUpgradeLevel = 1;
        [SerializeField] private int _upgradeCost = 20;
        [SerializeField] private int _maxUpgradeLevel = 5;
        [SerializeField] private Sprite _icon;

        public string DisplayName => _displayName;
        public int MaxHealth => _maxHealth;
        public int Damage => _damage;
        public int Cost => _cost;
        public float Range => _range;
        public float FireRate => _fireRate;
        public bool Splash => _splash;

        /// <summary>Splash radius in hex tiles; only meaningful when Splash is true.</summary>
        public float SplashRadius => _splashRadius;

        /// <summary>World units per second the fired projectile travels at.</summary>
        public float ProjectileSpeed => _projectileSpeed;

        /// <summary>Key into SaveManager's upgrade levels; empty means not upgradeable.</summary>
        public string UpgradeSaveKey => _upgradeSaveKey;

        /// <summary>Extra damage granted per upgrade level.</summary>
        public int DamagePerUpgradeLevel => _damagePerUpgradeLevel;

        /// <summary>Base meta-XP cost to buy the next upgrade level; scales with the level already bought.</summary>
        public int UpgradeCost => _upgradeCost;

        /// <summary>Highest upgrade level purchasable in the upgrade shop.</summary>
        public int MaxUpgradeLevel => _maxUpgradeLevel;

        /// <summary>Icon shown in the hotbar/upgrade shop, matching the in-world sprite.</summary>
        public Sprite Icon => _icon;
    }
}
