using UnityEngine;

public class PlayerItemCollector : MonoBehaviour
{
    private InventoryController inventoryController;

    private void Start()
    {
        inventoryController = FindObjectOfType<InventoryController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
{
    if (collision.CompareTag("Item"))
    {
        Item item = collision.GetComponent<Item>();
        if (item == null) return;

        if (item.uiItemPrefab == null)
        {
            Debug.LogError($"PlayerItemCollector: '{item.Name}' has no uiItemPrefab assigned.");
            return;
        }

        // Pass the prefab directly — AddItem will instantiate it
        bool itemAdded = inventoryController.AddItem(item.uiItemPrefab);

        if (itemAdded)
        {
            item.Pickup();
            Destroy(collision.gameObject);
        }
    }
}
}