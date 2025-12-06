using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InventoryDir;
using InventoryDir.Items;
using TMPro;

namespace View
{
    public class InventorySlot : MonoBehaviour, IPointerClickHandler
    {
        public Image icon;
        public TMP_Text countText;

        private Inventory inventory;
        private int index;
        private ItemStack current;

        public void Setup(Inventory inv, int idx)
        {
            inventory = inv;
            index = idx;

            if (!icon) icon = GetComponentInChildren<Image>();
            if (!countText) countText = GetComponentInChildren<TMP_Text>();
        }

        public void SetSlotData(ItemStack s)
        {
            current = s;

            if (s.IsEmpty)
            {
                if (icon)
                {
                    icon.enabled = false;
                    icon.sprite = null;
                }

                if (countText) countText.text = "";
                return;
            }

            ItemData data = inventory ? inventory.GetItemData(s.itemId) : null;

            if (icon)
            {
                icon.enabled = data && data.icon;
                icon.sprite = data ? data.icon : null;
            }

            if (countText)
                countText.text = (s.count > 1) ? s.count.ToString() : "";
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!inventory) return;

            if (eventData.button == PointerEventData.InputButton.Right)
            {
                inventory.UseItem(index);
            }
        }
    }
}