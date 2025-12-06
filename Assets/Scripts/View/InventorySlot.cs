using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InventoryDir;
using InventoryDir.Items;

namespace View
{
    public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image icon;
        public Text countText;

        private Inventory inventory;
        private int index;
        private InventoryUI ui;
        private ItemStack current;

        private static GameObject dragIcon;
        private static ItemStack draggingStack;
        private static int draggingFrom = -1;

        public void Setup(Inventory inv, int idx, InventoryUI uiRef)
        {
            inventory = inv;
            index = idx;
            ui = uiRef;
            if (!icon) icon = GetComponentInChildren<Image>();
            if (!countText) countText = GetComponentInChildren<Text>();
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
            }
            else
            {
                ItemData data = inventory.GetItemData(s.itemId);
                if (icon)
                {
                    icon.enabled = data && data.icon;
                    icon.sprite = data ? data.icon : null;
                }

                if (countText) countText.text = (s.count > 1) ? s.count.ToString() : "";
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                inventory.UseItem(index);
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (current.IsEmpty) return;
            draggingStack = current;
            draggingFrom = index;

            // create icon
            dragIcon = new GameObject("DragIcon");
            Image img = dragIcon.AddComponent<Image>();
            img.raycastTarget = false;
            ItemData data = inventory.GetItemData(current.itemId);
            
            if (data && data.icon) img.sprite = data.icon;
            dragIcon.transform.SetParent(transform.root, false);
            dragIcon.transform.SetAsLastSibling();
            RectTransform rt = dragIcon.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(40, 40);

            SetSlotData(new ItemStack());
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (dragIcon)
            {
                dragIcon.transform.position = eventData.position;
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (dragIcon) Destroy(dragIcon);
            if (draggingFrom >= 0)
            {
                // if not dropped on a slot, put back
                if (!draggingStack.IsEmpty)
                {
                    // restore original if not swapped
                    inventory.MoveItem(draggingFrom, draggingFrom);
                }
            }

            draggingStack = new ItemStack();
            draggingFrom = -1;
            ui.RefreshUI();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (draggingFrom < 0) return;
            
            // perform move on inventory
            inventory.MoveItem(draggingFrom, index);
            draggingStack = new ItemStack();
            draggingFrom = -1;
            ui.RefreshUI();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (current.IsEmpty) return;
            
            ItemData d = inventory.GetItemData(current.itemId);
            
            ui.ShowTooltip(d ? d.displayName : current.itemId, d ? d.description : "",
                d ? d.icon : null, eventData.position);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            ui.HideTooltip();
        }
    }
}