using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RewardsController : MonoBehaviour
{
    public static RewardsController Instance {get; private set;}

    public void Awake()
    {
        if(Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void GiveQuestReward(Quest quest)
    {
        if(quest?.questRewards == null) return;

        foreach(var reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Gold:
                    break;
                case RewardType.Experience:
                    break;
                case RewardType.Badge:
                    break;
                case RewardType.Custom:
                    break;
            }
        }
    }

   public void GiveItemReward(int itemID, int amount)
    {
        var itemPrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefab(itemID);

        if (itemPrefab == null) return;

        for (int i = 0; i < amount; i++)
        {
            // AddItem returns true if it successfully put the item in a slot
            if (InventoryController.Instance.AddItem(itemPrefab))
            {
                // SUCCESS: The item is already in the inventory panel. 
                // Just show the notification.
                itemPrefab.GetComponent<Item>().ShowPopUp();
            }
            else
            {
                // FAILURE: Inventory is full. Drop it on the ground instead.
                GameObject dropItem = Instantiate(itemPrefab, transform.position + Vector3.down, Quaternion.identity);
                if (dropItem.TryGetComponent<BounceEffect>(out var bounce))
                {
                    bounce.StartBounce();
                }
            }
        }
    }
}
