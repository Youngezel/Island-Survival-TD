using Game.Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// A hotbar button representing one purchasable building or hex tile.
    /// Click it to select it for placement, then click a hex tile on the
    /// map to place it there (see PlacementCursor).
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class HotbarSlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private HotbarItemData _item;

        private void Awake()
        {
            if (_item != null)
            {
                GetComponent<Image>().sprite = _item.Icon;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_item != null && PlacementCursor.Instance != null)
            {
                PlacementCursor.Instance.SelectItem(_item);
            }
        }
    }
}
