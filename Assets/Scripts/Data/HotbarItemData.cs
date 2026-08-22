using UnityEngine;

namespace Game.Data
{
    /// <summary>
    /// One purchasable hotbar entry: either a building prefab, or a new hex
    /// tile to expand the buildable area.
    /// </summary>
    [CreateAssetMenu(fileName = "HotbarItemData", menuName = "Island Survival TD/Hotbar Item Data")]
    public class HotbarItemData : ScriptableObject
    {
        [SerializeField] private string _displayName;
        [SerializeField] private Sprite _icon;
        [SerializeField] private int _cost;
        [SerializeField] private bool _isGroundTile;
        [SerializeField] private GameObject _buildingPrefab;

        public string DisplayName => _displayName;
        public Sprite Icon => _icon;
        public int Cost => _cost;

        /// <summary>True for the plain hex tile purchase; false for a building.</summary>
        public bool IsGroundTile => _isGroundTile;

        /// <summary>The prefab to instantiate; unused when IsGroundTile is true.</summary>
        public GameObject BuildingPrefab => _buildingPrefab;
    }
}
