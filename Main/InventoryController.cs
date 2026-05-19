using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;
    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs;

    public static InventoryController Instance {get; private set;}
    Dictionary<int, int> itemsCountCache = new();
    private List<InventorySaveData> cachedInventoryItems = new();
    public event Action OnInventoryChanged;

    public void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        itemDictionary = FindObjectOfType<ItemDictionary>();
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.sceneUnloaded += OnSceneUnloaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }
    }

    void Start()
    {
        RebindSceneReferences();
        EnsureSlotsExist();
        RebuildItemCounts();
    }

    public void RebuildItemCounts()
    {
        RebindSceneReferences();
        EnsureSlotsExist();

        if (inventoryPanel == null)
            return;

        itemsCountCache.Clear();

        foreach(Transform slotTranform in inventoryPanel.transform)
        {
            Slot slot = slotTranform.GetComponent<Slot>();
            if(slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if(item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts()
    {
        return itemsCountCache;
    }

    public bool HasItem(int itemID)
    {
        RebuildItemCounts();
        foreach (InventorySaveData data in cachedInventoryItems)
        {
            if (data != null && data.itemID == itemID && data.quantity > 0)
                return true;
        }

        return itemsCountCache.GetValueOrDefault(itemID) > 0;
    }

    public bool AddItem(GameObject itemPrefab)
    {
        RebindSceneReferences();
        EnsureSlotsExist();

        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null) return false;

        if (inventoryPanel == null)
            return AddItemToCache(itemToAdd);

        //check if there's item type in inventory
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item slotItem = slot.currentItem.GetComponent<Item>();
                if(slotItem != null &&
                slotItem.ID == itemToAdd.ID &&
                slotItem.stackable)
                {
                    slotItem.AddToStack();
                    RebuildItemCounts();
                    return true;
                }
            }
        }

        //look for empty slots
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slotTransform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                slot.currentItem = newItem;
                RebuildItemCounts();
                return true; // Item added successfully
            }
        }
        // Debug.Log("Inventory Full");
        return false; // Inventory full
    }

    public void CacheCurrentInventory()
    {
        if (inventoryPanel == null)
            return;

        cachedInventoryItems = GetInventoryItems();
    }

    public List<InventorySaveData> GetInventoryItems()
    {
        RebindSceneReferences();
        EnsureSlotsExist();

        List<InventorySaveData> invData = new List<InventorySaveData>();
        if (inventoryPanel == null)
            return CopyInventoryData(cachedInventoryItems);

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex(),
                    quantity = item.quantity
                });
            }
        }
        cachedInventoryItems = invData;
        return invData;
    }
    
    public void SetInventoryItems(List<InventorySaveData> inventorySaveData)
    {
        RebindSceneReferences();

        if (inventoryPanel == null || slotPrefab == null)
        {
            cachedInventoryItems = CopyInventoryData(inventorySaveData);
            return;
        }

        //Clear inventory panel and recreate slots
        List<GameObject> children = new List<GameObject>();
        foreach (Transform child in inventoryPanel.transform)
        {
            children.Add(child.gameObject);
        }

        foreach (GameObject child in children)
        {
            Destroy(child);
        }

        //create new slots
        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        if (inventorySaveData == null)
        {
            cachedInventoryItems = new List<InventorySaveData>();
            RebuildItemCounts();
            return;
        }

        cachedInventoryItems = CopyInventoryData(inventorySaveData);

        //Populate slots with isaved items
        foreach (InventorySaveData data in inventorySaveData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    Item itemComponent = item.GetComponent<Item>();
                    if(itemComponent != null && data.quantity > 1)
                    {
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }
                    slot.currentItem = item;
                }
            }
        } 

        RebuildItemCounts();
    }

    public void RemoveItemsFromInventory(int itemID, int amountToRemove)
    {
        RebindSceneReferences();
        EnsureSlotsExist();

        foreach(Transform slotTranform in inventoryPanel.transform)
        {
            if(amountToRemove <= 0) break;

            Slot slot = slotTranform.GetComponent<Slot>();
            if(slot?.currentItem?.GetComponent<Item>() is Item item && item.ID == itemID)
            {
                int removed = Mathf.Min(amountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                amountToRemove -= removed;

                if(item.quantity == 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }

        RebuildItemCounts();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindSceneReferences();

        if (inventoryPanel != null && cachedInventoryItems.Count > 0)
            SetInventoryItems(cachedInventoryItems);
        else
            EnsureSlotsExist();

        RebuildItemCounts();
        BadgeController.Instance?.RestoreEarnedBadgesToInventory();
    }

    private void OnSceneUnloaded(Scene scene)
    {
        if (inventoryPanel == null || inventoryPanel.transform.childCount == 0)
            return;

        cachedInventoryItems = GetInventoryItems();
    }

    private bool AddItemToCache(Item itemToAdd)
    {
        foreach (InventorySaveData data in cachedInventoryItems)
        {
            if (data.itemID == itemToAdd.ID && itemToAdd.stackable)
            {
                data.quantity += itemToAdd.quantity;
                RebuildCachedItemCounts();
                return true;
            }
        }

        if (cachedInventoryItems.Count >= slotCount)
            return false;

        HashSet<int> occupiedSlots = new HashSet<int>();
        foreach (InventorySaveData data in cachedInventoryItems)
        {
            occupiedSlots.Add(data.slotIndex);
        }

        int slotIndex = 0;
        while (occupiedSlots.Contains(slotIndex) && slotIndex < slotCount)
        {
            slotIndex++;
        }

        if (slotIndex >= slotCount)
            return false;

        cachedInventoryItems.Add(new InventorySaveData
        {
            itemID = itemToAdd.ID,
            slotIndex = slotIndex,
            quantity = itemToAdd.quantity
        });

        RebuildCachedItemCounts();
        return true;
    }

    private void RebuildCachedItemCounts()
    {
        itemsCountCache.Clear();

        foreach (InventorySaveData data in cachedInventoryItems)
        {
            itemsCountCache[data.itemID] = itemsCountCache.GetValueOrDefault(data.itemID, 0) + data.quantity;
        }

        OnInventoryChanged?.Invoke();
    }

    private void RebindSceneReferences()
    {
        if (!IsSceneObjectValid(inventoryPanel))
            inventoryPanel = FindSceneObject("InventoryPage");

        if (itemDictionary == null)
            itemDictionary = FindObjectOfType<ItemDictionary>();
    }

    private bool IsSceneObjectValid(GameObject sceneObject)
    {
        try
        {
            return sceneObject != null && sceneObject.scene.IsValid();
        }
        catch (MissingReferenceException)
        {
            return false;
        }
    }

    private void EnsureSlotsExist()
    {
        if (inventoryPanel == null || slotPrefab == null)
            return;

        if (slotCount <= 0)
            slotCount = 27;

        if (inventoryPanel.transform.childCount > 0)
            return;

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }
    }

    private GameObject FindSceneObject(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform == null || !transform.gameObject.scene.IsValid())
                continue;

            if (transform.name == objectName)
                return transform.gameObject;
        }

        return null;
    }

    private List<InventorySaveData> CopyInventoryData(List<InventorySaveData> source)
    {
        List<InventorySaveData> copy = new List<InventorySaveData>();
        if (source == null)
            return copy;

        foreach (InventorySaveData data in source)
        {
            if (data == null)
                continue;

            copy.Add(new InventorySaveData
            {
                itemID = data.itemID,
                slotIndex = data.slotIndex,
                quantity = data.quantity
            });
        }

        return copy;
    }
}
