using UnityEngine;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int ID;
    public string Name;
    public GameObject itemPrefab;    // World-space prefab (SpriteRenderer)
    public GameObject uiItemPrefab;  // UI prefab (Image + ItemDragHandler)

    public virtual void UseItem()
    {
        Debug.Log($"Using item {Name}");
    }

    public virtual void Pickup()
    {
        Sprite itemIcon = GetComponent<Image>()?.sprite
                       ?? GetComponent<SpriteRenderer>()?.sprite;

        if (ItemPickupUIController.Instance != null && itemIcon != null)
            ItemPickupUIController.Instance.ShowItemPickup(Name, itemIcon);
    }
}