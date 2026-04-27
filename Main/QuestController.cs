using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activateQuest = new();
    private QuestUI questUI;
    public List<string> handinQuestIDs = new();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // ← ADD THIS
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        questUI = FindObjectOfType<QuestUI>();
    }

    private void Start()
    {
        if (InventoryController.Instance != null)
            InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;
    }

    public void AcceptQuest(Quest quest)
    {
        if (IsQuestActive(quest.questID)) return;

        activateQuest.Add(new QuestProgress(quest));
        CheckInventoryForQuests();
        RefreshQuestUI();

        Debug.Log($"[QuestController] Quest accepted: {quest.questName}");
    }

    public bool IsQuestActive(string questID) =>
        activateQuest.Exists(q => q.QuestID == questID);

    public void CheckInventoryForQuests()
    {
        if (InventoryController.Instance == null) return;

        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();

        foreach (QuestProgress quest in activateQuest)
        {
            foreach (QuestObjective questObjective in quest.objectives)
            {
                if (questObjective.type != ObjectiveType.CollectItem) continue;
                if (!int.TryParse(questObjective.objectiveID, out int itemID)) continue;

                int newAmount = itemCounts.TryGetValue(itemID, out int count)
                    ? Mathf.Min(count, questObjective.requiredAmount)
                    : 0;

                if (questObjective.currentAmount != newAmount)
                    questObjective.currentAmount = newAmount;
            }
        }

        RefreshQuestUI();
    }

    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activateQuest.Find(q => q.QuestID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }

    public void HandInQuest(string questID)
    {
        if (!RemoveRequiredItemsFromInventory(questID)) return;

        QuestProgress quest = activateQuest.Find(q => q.QuestID == questID);
        if (quest != null)
        {
            handinQuestIDs.Add(questID);
            activateQuest.Remove(quest);
            RefreshQuestUI();
            Debug.Log($"[QuestController] Quest handed in: {questID}");
        }
    }

    public bool IsQuestHandedIn(string questID) =>
        handinQuestIDs.Contains(questID);

    public bool RemoveRequiredItemsFromInventory(string questID)
    {
        QuestProgress quest = activateQuest.Find(q => q.QuestID == questID);
        if (quest == null) return false;

        Dictionary<int, int> requiredItems = new();

        foreach (QuestObjective objective in quest.objectives)
        {
            if (objective.type == ObjectiveType.CollectItem &&
                int.TryParse(objective.objectiveID, out int itemID))
            {
                requiredItems[itemID] = objective.requiredAmount;
            }
        }

        // Circuit quests have no CollectItem objectives — always return true
        if (requiredItems.Count == 0) return true;

        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();
        foreach (var item in requiredItems)
        {
            if (itemCounts.GetValueOrDefault(item.Key) < item.Value)
                return false;
        }

        foreach (var itemRequirement in requiredItems)
            InventoryController.Instance.RemoveItemsFromInventory(
                itemRequirement.Key, itemRequirement.Value);

        return true;
    }

    public void LoadQuestProgress(List<QuestProgress> savedQuests)
    {
        activateQuest = savedQuests ?? new();
        CheckInventoryForQuests();
        RefreshQuestUI();
    }

    public void UpdateObjective(string objectiveID, ObjectiveType type, int amount = 1)
    {
        bool updated = false;

        foreach (QuestProgress quest in activateQuest)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                if (objective.type != type) continue;
                if (!string.Equals(objective.objectiveID, objectiveID,
                    System.StringComparison.OrdinalIgnoreCase)) continue;
                if (objective.IsCompleted) continue;

                objective.currentAmount += amount;
                if (objective.currentAmount > objective.requiredAmount)
                    objective.currentAmount = objective.requiredAmount;

                updated = true;
                Debug.Log($"[QuestController] Objective updated: {objectiveID} " +
                          $"({objective.currentAmount}/{objective.requiredAmount})");
            }
        }

        if (updated) RefreshQuestUI();
    }

    // ─── UI Refresh ───────────────────────────────────────────────────
    // Finds QuestUI fresh each time in case scene changed

    private void RefreshQuestUI()
    {
        // Re-find QuestUI in case we've changed scenes
        if (questUI == null)
            questUI = FindObjectOfType<QuestUI>();

        if (questUI != null)
            questUI.UpdateQuestUI();
    }
}