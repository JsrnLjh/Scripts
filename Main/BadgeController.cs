using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BadgeController : MonoBehaviour
{
    private static BadgeController instance;

    public static BadgeController Instance
    {
        get
        {
            if (instance == null)
                instance = FindAnyObjectByType<BadgeController>();

            return instance;
        }
        private set => instance = value;
    }

    // =========================================================
    // BADGE STORAGE
    // =========================================================

    private HashSet<int> earnedBadges = new HashSet<int>();

    // =========================================================
    // BADGE IDS
    // =========================================================

    public const int BADGE_Q1 = 101;
    public const int BADGE_Q2 = 102;
    public const int BADGE_Q3 = 103;
    public const int BADGE_Q4 = 104;
    public const int BADGE_Q5 = 105;
    public const int BADGE_Q6 = 106;

    // =========================================================
    // BADGE SPRITES
    // =========================================================

    [Header("Badge Sprites")]
    public Sprite badgeQ1Sprite;
    public Sprite badgeQ2Sprite;
    public Sprite badgeQ3Sprite;
    public Sprite badgeQ4Sprite;
    public Sprite badgeQ5Sprite;
    public Sprite badgeQ6Sprite;

    // =========================================================
    // BADGE INVENTORY PREFABS
    // These prefabs MUST contain:
    // - Item.cs
    // - Image
    // - UI item setup
    // =========================================================

    [Header("Badge Inventory Prefabs")]
    public GameObject badgeQ1Prefab;
    public GameObject badgeQ2Prefab;
    public GameObject badgeQ3Prefab;
    public GameObject badgeQ4Prefab;
    public GameObject badgeQ5Prefab;
    public GameObject badgeQ6Prefab;

    // =========================================================
    // UNITY METHODS
    // =========================================================

    private void Awake()
    {
        if (instance == null || instance == this)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RestoreEarnedBadgesToInventory();
    }

    // =========================================================
    // CORE API
    // =========================================================

    public void GiveBadge(int badgeID)
    {
        // Prevent duplicate badges
        if (earnedBadges.Contains(badgeID))
        {
            Debug.Log($"[BadgeController] Badge {badgeID} already earned.");
            EnsureBadgeInInventory(badgeID);
            return;
        }

        // Add badge to earned list
        earnedBadges.Add(badgeID);

        Debug.Log($"[BadgeController] Badge {badgeID} earned!");

        // =====================================================
        // ADD BADGE TO INVENTORY
        // =====================================================

        EnsureBadgeInInventory(badgeID);

        // =====================================================
        // SHOW BADGE POPUP
        // =====================================================

        ShowBadgePopup(badgeID);

        // =====================================================
        // AUTO SAVE
        // =====================================================

        SaveController saveController = FindObjectOfType<SaveController>();

        if (saveController != null)
        {
            saveController.SaveGame();
        }
    }

    public bool HasBadge(int badgeID)
    {
        return earnedBadges.Contains(badgeID);
    }

    public List<int> GetAllBadges()
    {
        return new List<int>(earnedBadges);
    }

    public int BadgeCount => earnedBadges.Count;

    // =========================================================
    // BADGE GATE HELPERS
    // =========================================================

    public bool CanAccessQ2() => HasBadge(BADGE_Q1);
    public bool CanAccessQ3() => HasBadge(BADGE_Q2);
    public bool CanAccessQ4() => HasBadge(BADGE_Q3);
    public bool CanAccessQ5() => HasBadge(BADGE_Q4);
    public bool CanAccessQ6() => HasBadge(BADGE_Q5);

    // =========================================================
    // BADGE POPUP
    // =========================================================

    private void ShowBadgePopup(int badgeID)
    {
        if (ItemPickupUIController.Instance == null)
        {
            Debug.LogWarning("[BadgeController] ItemPickupUIController not found.");
            return;
        }

        Sprite sprite = GetBadgeSprite(badgeID);

        if (sprite == null)
        {
            Debug.LogWarning($"[BadgeController] No sprite assigned for badge {badgeID}");
            return;
        }

        // Empty string = icon only popup
        ItemPickupUIController.Instance.ShowItemPickup("", sprite);
    }

    // =========================================================
    // BADGE SPRITE GETTER
    // =========================================================

    public Sprite GetBadgeSprite(int badgeID)
    {
        return badgeID switch
        {
            BADGE_Q1 => badgeQ1Sprite,
            BADGE_Q2 => badgeQ2Sprite,
            BADGE_Q3 => badgeQ3Sprite,
            BADGE_Q4 => badgeQ4Sprite,
            BADGE_Q5 => badgeQ5Sprite,
            BADGE_Q6 => badgeQ6Sprite,
            _ => null
        };
    }

    // =========================================================
    // BADGE PREFAB GETTER
    // =========================================================

    private GameObject GetBadgePrefab(int badgeID)
    {
        return badgeID switch
        {
            BADGE_Q1 => badgeQ1Prefab,
            BADGE_Q2 => badgeQ2Prefab,
            BADGE_Q3 => badgeQ3Prefab,
            BADGE_Q4 => badgeQ4Prefab,
            BADGE_Q5 => badgeQ5Prefab,
            BADGE_Q6 => badgeQ6Prefab,
            _ => null
        };
    }

    // =========================================================
    // SAVE / LOAD
    // =========================================================

    public List<int> GetSaveData()
    {
        return GetAllBadges();
    }

    public void RestoreEarnedBadgesToInventory()
    {
        foreach (int id in earnedBadges)
            EnsureBadgeInInventory(id);
    }

    public void LoadSaveData(List<int> savedBadges)
    {
        if (savedBadges == null)
        {
            Debug.Log("[BadgeController] No badge save data found.");
            return;
        }

        foreach (int id in savedBadges)
        {
            earnedBadges.Add(id);
            EnsureBadgeInInventory(id);
        }

        Debug.Log($"[BadgeController] Loaded {earnedBadges.Count} badge(s).");
    }

    private void EnsureBadgeInInventory(int badgeID)
    {
        if (InventoryController.Instance == null)
        {
            Debug.LogWarning("[BadgeController] InventoryController not found.");
            return;
        }

        GameObject badgePrefab = GetBadgePrefab(badgeID);

        if (badgePrefab == null)
            badgePrefab = FindAnyObjectByType<ItemDictionary>()?.GetItemPrefab(badgeID);

        if (badgePrefab == null)
        {
            Debug.LogWarning($"[BadgeController] No badge prefab assigned for badge ID {badgeID}");
            return;
        }

        Item badgeItem = badgePrefab.GetComponent<Item>();
        int inventoryItemID = badgeItem != null ? badgeItem.ID : badgeID;

        if (InventoryController.Instance.HasItem(inventoryItemID))
            return;

        bool added = InventoryController.Instance.AddItem(badgePrefab);

        if (!added)
            Debug.LogWarning("[BadgeController] Inventory full. Badge could not be added.");
    }

    // =========================================================
    // DEBUG HELPERS
    // =========================================================

    [ContextMenu("Debug Give All Badges")]
    private void Debug_GiveAllBadges()
    {
        GiveBadge(BADGE_Q1);
        GiveBadge(BADGE_Q2);
        GiveBadge(BADGE_Q3);
        GiveBadge(BADGE_Q4);
        GiveBadge(BADGE_Q5);
        GiveBadge(BADGE_Q6);
    }

    [ContextMenu("Debug Clear Badges")]
    private void Debug_ClearBadges()
    {
        earnedBadges.Clear();

        Debug.Log("[BadgeController] All badges cleared.");
    }

    [ContextMenu("Debug Print Badges")]
    private void Debug_PrintBadges()
    {
        Debug.Log($"Earned Badges: [{string.Join(", ", earnedBadges)}]");
    }
}
