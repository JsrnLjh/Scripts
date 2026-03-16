using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    public static HotbarController Instance { get; private set; }

    public GameObject hotbarPanel;
    public GameObject slotPrefab;
    public int slotCount = 4;

    private ItemDictionary itemDictionary;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Debug.LogError("Multiple HotbarController instances detected! Destroying the extra one.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        itemDictionary = FindObjectOfType<ItemDictionary>();
        SetupSlotButtons();
    }

    private void SetupSlotButtons()
    {
        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            int index = i; // Capture for lambda
            Button btn = hotbarPanel.transform.GetChild(i).GetComponent<Button>();
            if (btn != null)
                btn.onClick.AddListener(() => UseItemInSlot(index));
        }
    }

    public void UseItemInSlot(int index)
    {
        Slot slot = hotbarPanel.transform.GetChild(index).GetComponent<Slot>();
        if (slot.currentItem != null)
        {
            Item item = slot.currentItem.GetComponent<Item>();
            item.UseItem();
        }
    }

    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach(Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if(slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData {itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex() });
            }
        }
        return invData;    
    }

    public void SetHotbarItems(List<InventorySaveData>inventorySaveData)
    {
        //Clears inventory panel to avoid duplicates
        foreach(Transform child in hotbarPanel.transform)
        {
            Destroy(child.gameObject);
        }
        // Creates new slots
        for(int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }

        // Populate slots with saved items
        foreach(InventorySaveData data in inventorySaveData)
        {
            if(data.slotIndex < slotCount)
            {
                Slot slot = hotbarPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if(itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    slot.currentItem = item;
                }
            }
        }
    }


}