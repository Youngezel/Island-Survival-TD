using Game.Data;
using Game.Grid;
using Game.Systems;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// A hotbar button representing one purchasable building or hex tile.
    /// Drag it onto the map to attempt placement.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class HotbarSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private HotbarItemData _item;
        [SerializeField] private Image _ghostIcon;
        [SerializeField] private Camera _worldCamera;

        private void Awake()
        {
            if (_worldCamera == null)
            {
                _worldCamera = Camera.main;
            }

            if (_item != null)
            {
                GetComponent<Image>().sprite = _item.Icon;
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_item == null || _ghostIcon == null)
            {
                return;
            }

            _ghostIcon.sprite = _item.Icon;
            _ghostIcon.gameObject.SetActive(true);
            _ghostIcon.transform.position = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_ghostIcon != null && _ghostIcon.gameObject.activeSelf)
            {
                _ghostIcon.transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_ghostIcon != null)
            {
                _ghostIcon.gameObject.SetActive(false);
            }

            if (_item == null || HexGridManager.Instance == null || BuildPlacer.Instance == null)
            {
                return;
            }

            Vector3Int cell = HexGridManager.Instance.ScreenToCell(eventData.position, _worldCamera);
            BuildPlacer.Instance.TryPlace(_item, cell);
        }
    }
}
