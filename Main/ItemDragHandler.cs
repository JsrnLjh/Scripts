using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Transform originalParent;
    private Slot originalSlot;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas rootCanvas;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalSlot = originalParent.GetComponent<Slot>();

        // Move dragged item to top-level canvas so it appears above everything
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform);
        }
        else
        {
            transform.SetParent(transform.root);
        }

        transform.SetAsLastSibling();

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Slot targetSlot = GetTargetSlot(eventData);

        // No valid slot found, return to original
        if (targetSlot == null)
        {
            ReturnToOriginalSlot();
            return;
        }

        // Dropped in the same slot, just snap back
        if (targetSlot == originalSlot)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = Vector2.zero;
            return;
        }

        GameObject targetItem = targetSlot.currentItem;

        // If target slot already has an item, swap them
        if (targetItem != null)
        {
            targetItem.transform.SetParent(originalSlot.transform);
            targetItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            originalSlot.currentItem = targetItem;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        // Place dragged item in target slot
        transform.SetParent(targetSlot.transform);
        rectTransform.anchoredPosition = Vector2.zero;
        targetSlot.currentItem = gameObject;
    }

    private Slot GetTargetSlot(PointerEventData eventData)
    {
        if (eventData.pointerEnter == null)
            return null;

        // Case 1: pointer is directly over a slot
        Slot slot = eventData.pointerEnter.GetComponent<Slot>();
        if (slot != null)
            return slot;

        // Case 2: pointer is over an item inside a slot
        return eventData.pointerEnter.GetComponentInParent<Slot>();
    }

    private void ReturnToOriginalSlot()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;

        if (originalSlot != null)
        {
            originalSlot.currentItem = gameObject;
        }
    }
}