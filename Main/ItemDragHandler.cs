using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("Drop Settings")]
    public float minDropDistance = 2f;
    public float maxDropDistance = 3f;

    private Transform originalParent;
    private Slot originalSlot;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private bool isDragging = false; // Track if a drag occurred

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Only use item if it was a clean tap, not a drag
        if (!isDragging)
        {
            Item item = GetComponent<Item>();
            if (item != null)
                item.UseItem();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        rootCanvas = GetComponentInParent<Canvas>();
        originalParent = transform.parent;
        originalSlot = originalParent.GetComponent<Slot>();

        if (originalSlot != null)
            originalSlot.currentItem = null;

        Transform canvasTransform = rootCanvas != null ? rootCanvas.transform : transform.root;
        transform.SetParent(canvasTransform);
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
        isDragging = false; // Reset drag flag

        Slot targetSlot = GetTargetSlot(eventData);

        if (targetSlot == null)
        {
            if (!IsWithinInventory(eventData.position))
            {
                DropItem(originalSlot);
                return;
            }

            ReturnToOriginalSlot();
            return;
        }

        if (targetSlot == originalSlot)
        {
            transform.SetParent(originalParent);
            rectTransform.anchoredPosition = Vector2.zero;
            originalSlot.currentItem = gameObject;
            return;
        }

        GameObject targetItem = targetSlot.currentItem;

        if (targetItem != null)
        {
            targetItem.transform.SetParent(originalParent);
            targetItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            originalSlot.currentItem = targetItem;
        }
        else
        {
            originalSlot.currentItem = null;
        }

        transform.SetParent(targetSlot.transform);
        rectTransform.anchoredPosition = Vector2.zero;
        targetSlot.currentItem = gameObject;
    }

    private bool IsWithinInventory(Vector2 mousePosition)
    {
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);
    }

    private void DropItem(Slot slot)
    {
        if (slot != null)
            slot.currentItem = null;

        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("DropItem: Missing 'Player' tag on player object.");
            ReturnToOriginalSlot();
            return;
        }

        Item itemData = GetComponent<Item>();
        if (itemData == null)
        {
            Debug.LogError("DropItem: No Item component found on dragged object.");
            ReturnToOriginalSlot();
            return;
        }

        if (itemData.itemPrefab == null)
        {
            Debug.LogError($"DropItem: '{itemData.Name}' has no itemPrefab assigned in the Inspector.");
            ReturnToOriginalSlot();
            return;
        }

        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        GameObject worldItem = Instantiate(itemData.itemPrefab, dropPosition, Quaternion.identity);
        worldItem.tag = "Item";

        Destroy(gameObject);
    }

    private Slot GetTargetSlot(PointerEventData eventData)
    {
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (RaycastResult result in results)
        {
            if (result.gameObject == gameObject) continue;

            Slot slot = result.gameObject.GetComponent<Slot>()
                        ?? result.gameObject.GetComponentInParent<Slot>();

            if (slot != null)
                return slot;
        }

        return null;
    }

    private void ReturnToOriginalSlot()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = Vector2.zero;

        if (originalSlot != null)
            originalSlot.currentItem = gameObject;
    }
}