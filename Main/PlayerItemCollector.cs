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

            // Use the UI prefab for inventory, not the world prefab
            if (item.uiItemPrefab == null)
            {
                Debug.LogError($"PlayerItemCollector: '{item.Name}' has no uiItemPrefab assigned.");
                return;
            }

            // Instantiate the correct UI prefab and add that to inventory
            GameObject uiItem = Instantiate(item.uiItemPrefab);
            bool itemAdded = inventoryController.AddItem(uiItem);

            if (itemAdded)
            {
                item.Pickup();
                Destroy(collision.gameObject);
            }
            else
            {
                // Inventory full — destroy the temporary UI item
                Destroy(uiItem);
            }
        }
    }
}