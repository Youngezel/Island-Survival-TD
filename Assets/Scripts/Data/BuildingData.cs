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
        [SerializeField] private UpgradePath _pathA;
        [SerializeField] private UpgradePath _pathB;
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

        /// <summary>Key into SaveManager's unlocked-tier data; empty means not upgradeable.</summary>
        public string UpgradeSaveKey => _upgradeSaveKey;

        /// <summary>The first of the two mutually-exclusive upgrade paths.</summary>
        public UpgradePath PathA => _pathA;

        /// <summary>The second of the two mutually-exclusive upgrade paths.</summary>
        public UpgradePath PathB => _pathB;

        /// <summary>Icon shown in the hotbar/upgrade shop, matching the in-world sprite.</summary>
        public Sprite Icon => _icon;
    }
}
