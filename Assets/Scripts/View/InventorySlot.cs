using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using InventoryDir;
using InventoryDir.Items;
using TMPro;

namespace View
{
    public class InventorySlot : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IDropHandler, IPointerEnterHandler, IPointerExitHandler
    {
        public Image icon;
        public TMP_Text countText;

        private Inventory inventory;
        private int index;
        private InventoryUI ui;
        private ItemStack current;

        private static GameObject dragIcon;
        private static ItemStack draggingStack;
        private static int draggingFrom = -1;
        private static bool isDragging;
        private Coroutine tooltipCoroutine;
        private bool isHovering;
        private bool allowTooltip = true;

        public void Setup(Inventory inv, int idx, InventoryUI uiRef)
        {
            inventory = inv;
            index = idx;
            ui = uiRef;
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
            }
            else
            {
                ItemData data = inventory ? inventory.GetItemData(s.itemId) : null;
                if (icon)
                {
                    icon.enabled = data && data.icon;
                    icon.sprite = data ? data.icon : null;
                }

                if (countText)
                    countText.text = (s.count > 1) ? s.count.ToString() : "";
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

            isDragging = true;
            isHovering = false;
            allowTooltip = false;

            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }
            if (ui) ui.HideTooltip();

            draggingStack = current;
            draggingFrom = index;

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
                if (!draggingStack.IsEmpty)
                {
                    inventory.MoveItem(draggingFrom, draggingFrom);
                }
            }

            isDragging = false;
            draggingStack = new ItemStack();
            draggingFrom = -1;
            ui.RefreshUI();
        }

        public void OnDrop(PointerEventData eventData)
        {
            if (draggingFrom < 0) return;

            inventory.MoveItem(draggingFrom, index);
            draggingStack = new ItemStack();
            draggingFrom = -1;
            
            allowTooltip = false;
            
            ui.RefreshUI();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!ui || !inventory || current.IsEmpty) return;

            isHovering = true;
            allowTooltip = true;

            if (isDragging) return;

            if (tooltipCoroutine != null)
                StopCoroutine(tooltipCoroutine);

            tooltipCoroutine = StartCoroutine(ShowTooltipDelayed(eventData));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            allowTooltip = true;

            if (tooltipCoroutine != null)
            {
                StopCoroutine(tooltipCoroutine);
                tooltipCoroutine = null;
            }

            if (!ui) return;
            ui.HideTooltip();
        }

        private IEnumerator ShowTooltipDelayed(PointerEventData eventData)
        {
            yield return new WaitForSeconds(TooltipUI.ShowDelay);

            if (!isHovering || !allowTooltip || isDragging) yield break;

            ItemData d = inventory.GetItemData(current.itemId);

            ui.ShowTooltip(
                d ? d.displayName : current.itemId,
                d ? d.description : "",
                d ? d.icon : null,
                eventData.position
            );

            while (isHovering && !isDragging)
            {
                ui.UpdateTooltipPosition(eventData.position);
                yield return null;
            }
        }
    }
}