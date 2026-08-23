using Game.Data;
using Game.Economy;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI
{
    /// <summary>
    /// A hotbar button representing one purchasable building or hex tile.
    /// Click it to select it for placement, then click a hex tile on the
    /// map to place it there (see PlacementCursor). Shows a gold border
    /// while selected and grays out its cost label while unaffordable,
    /// per the visual identity spec.
    /// </summary>
    public class HotbarSlot : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private HotbarItemData _item;
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _costText;

        private Image[] _borderEdges;

        private void Awake()
        {
            if (_item != null)
            {
                if (_icon != null)
                {
                    _icon.sprite = _item.Icon;
                }

                if (_costText != null)
                {
                    _costText.text = _item.Cost.ToString();
                }
            }

            _borderEdges = new[]
            {
                transform.Find("BorderTop")?.GetComponent<Image>(),
                transform.Find("BorderBottom")?.GetComponent<Image>(),
                transform.Find("BorderLeft")?.GetComponent<Image>(),
                transform.Find("BorderRight")?.GetComponent<Image>(),
            };
        }

        private void OnDisable()
        {
            if (CoinWallet.Instance != null)
            {
                CoinWallet.Instance.OnCoinsChanged -= HandleCoinsChanged;
            }
        }

        private void Start()
        {
            // Waits for Start (called after every object's Awake) rather than
            // subscribing in OnEnable, since CoinWallet may not have set its
            // static Instance yet if this slot's OnEnable runs first.
            if (CoinWallet.Instance != null)
            {
                CoinWallet.Instance.OnCoinsChanged += HandleCoinsChanged;
            }

            RefreshAffordability();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (_item != null && PlacementCursor.Instance != null)
            {
                PlacementCursor.Instance.SelectItem(_item);
            }
        }

        private void Update()
        {
            bool isSelected = PlacementCursor.Instance != null && PlacementCursor.Instance.SelectedItem == _item;
            SetBorderColor(isSelected ? UITheme.Gold : UITheme.WoodShadow);
        }

        private void HandleCoinsChanged(int coins)
        {
            RefreshAffordability();
        }

        private void RefreshAffordability()
        {
            if (_costText == null || _item == null)
            {
                return;
            }

            bool affordable = CoinWallet.Instance == null || CoinWallet.Instance.Coins >= _item.Cost;
            _costText.color = affordable ? UITheme.Gold : UITheme.TextDisabled;
        }

        private void SetBorderColor(Color color)
        {
            foreach (Image edge in _borderEdges)
            {
                if (edge != null)
                {
                    edge.color = color;
                }
            }
        }
    }
}
