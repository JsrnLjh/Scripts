using System.Collections.Generic;
using UnityEngine;

public class BadgeController : MonoBehaviour
{
    public static BadgeController Instance { get; private set; }

    private HashSet<int> earnedBadges = new HashSet<int>();

    public const int BADGE_Q1 = 101;
    public const int BADGE_Q2 = 102;
    public const int BADGE_Q3 = 103;
    public const int BADGE_Q4 = 104;
    public const int BADGE_Q5 = 105;
    public const int BADGE_Q6 = 106;

    [Header("Badge Sprites (assign in Inspector)")]
    public Sprite badgeQ1Sprite;
    public Sprite badgeQ2Sprite;
    public Sprite badgeQ3Sprite;
    public Sprite badgeQ4Sprite;
    public Sprite badgeQ5Sprite;
    public Sprite badgeQ6Sprite;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ─── Core API ─────────────────────────────────────────────────────

    public void GiveBadge(int badgeID)
    {
        if (earnedBadges.Contains(badgeID))
        {
            Debug.Log($"[BadgeController] Badge {badgeID} already earned.");
            return;
        }

        earnedBadges.Add(badgeID);
        Debug.Log($"[BadgeController] Badge {badgeID} earned!");

        ShowBadgePopup(badgeID);

        // Auto-save whenever a badge is earned
        SaveController saveController = FindObjectOfType<SaveController>();
        if (saveController != null)
            saveController.SaveGame();
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

    // ─── Badge Gate Helpers ───────────────────────────────────────────

    public bool CanAccessQ2() => HasBadge(BADGE_Q1);
    public bool CanAccessQ3() => HasBadge(BADGE_Q2);
    public bool CanAccessQ4() => HasBadge(BADGE_Q3);
    public bool CanAccessQ5() => HasBadge(BADGE_Q4);
    public bool CanAccessQ6() => HasBadge(BADGE_Q5);

    // ─── Badge Popup ──────────────────────────────────────────────────

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
            Debug.LogWarning($"[BadgeController] No sprite assigned for badge {badgeID}.");
            return;
        }

        // Empty string = no name text shown, icon only — matches your design decision
        ItemPickupUIController.Instance.ShowItemPickup("", sprite);
    }

    private Sprite GetBadgeSprite(int badgeID)
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

    // ─── Save / Load ──────────────────────────────────────────────────

    public List<int> GetSaveData()
    {
        return GetAllBadges();
    }

    public void LoadSaveData(List<int> savedBadges)
    {
        if (savedBadges == null)
        {
            Debug.Log("[BadgeController] No badge save data — starting fresh.");
            return;
        }

        earnedBadges.Clear();

        foreach (int id in savedBadges)
            earnedBadges.Add(id);

        Debug.Log($"[BadgeController] Loaded {earnedBadges.Count} badge(s): [{string.Join(", ", savedBadges)}]");
    }

    // ─── Debug Helpers ────────────────────────────────────────────────

    [ContextMenu("Debug: Give All Badges")]
    private void Debug_GiveAllBadges()
    {
        GiveBadge(BADGE_Q1);
        GiveBadge(BADGE_Q2);
        GiveBadge(BADGE_Q3);
        GiveBadge(BADGE_Q4);
        GiveBadge(BADGE_Q5);
        GiveBadge(BADGE_Q6);
    }

    [ContextMenu("Debug: Clear All Badges")]
    private void Debug_ClearAllBadges()
    {
        earnedBadges.Clear();
        Debug.Log("[BadgeController] All badges cleared.");
    }

    [ContextMenu("Debug: Print Earned Badges")]
    private void Debug_PrintBadges()
    {
        Debug.Log($"[BadgeController] Earned: [{string.Join(", ", earnedBadges)}]");
    }
}