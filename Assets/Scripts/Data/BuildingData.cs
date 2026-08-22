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

        public string DisplayName => _displayName;
        public int MaxHealth => _maxHealth;
        public int Damage => _damage;
        public int Cost => _cost;
        public float Range => _range;
        public float FireRate => _fireRate;
        public bool Splash => _splash;
    }
}
