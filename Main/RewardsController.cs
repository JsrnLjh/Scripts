using System.Collections.Generic;
using UnityEngine;

public class RewardsController : MonoBehaviour
{
    public static RewardsController Instance { get; private set; }

    public void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ← ADD THIS
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void GiveQuestReward(Quest quest)
    {
        if (quest?.questRewards == null) return;

        foreach (var reward in quest.questRewards)
        {
            switch (reward.type)
            {
                case RewardType.Item:
                    GiveItemReward(reward.rewardID, reward.amount);
                    break;
                case RewardType.Badge:
                    GiveBadgeReward(reward.rewardID); // ← NOW IMPLEMENTED
                    break;
                case RewardType.Gold:
                    break;
                case RewardType.Experience:
                    break;
                case RewardType.Custom:
                    break;
            }
        }
    }

    public void GiveBadgeReward(int badgeID)
    {
        if (BadgeController.Instance == null)
        {
            // Debug.LogWarning("[RewardsController] BadgeController not found.");
            return;
        }

        BadgeController.Instance.GiveBadge(badgeID);
        // Debug.Log($"[RewardsController] Badge {badgeID} given to player.");
    }

    public void GiveItemReward(int itemID, int amount)
    {
        var itemPrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefab(itemID);
        if (itemPrefab == null) return;

        for (int i = 0; i < amount; i++)
        {
            if (InventoryController.Instance.AddItem(itemPrefab))
            {
                itemPrefab.GetComponent<Item>().ShowPopUp();
            }
            else
            {
                GameObject dropItem = Instantiate(
                    itemPrefab,
                    transform.position + Vector3.down,
                    Quaternion.identity);

                if (dropItem.TryGetComponent<BounceEffect>(out var bounce))
                    bounce.StartBounce();
            }
        }
    }
}