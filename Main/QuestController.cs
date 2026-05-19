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

    public bool AcceptQuest(Quest quest)
    {
        if (quest == null)
            return false;

        if (IsQuestActive(quest.questID))
        {
            RefreshQuestUI();
            return true;
        }

        activateQuest.Add(new QuestProgress(quest));
        CheckInventoryForQuests();
        RefreshQuestUI();

        // Debug.Log($"[QuestController] Quest accepted: {quest.questName}");
        return true;
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

    public bool TryCompleteObjective(
        string objectiveID,
        ObjectiveType type,
        out Quest completedQuest,
        int amount = 1)
    {
        completedQuest = null;

        if (string.IsNullOrEmpty(objectiveID))
            return false;

        foreach (QuestProgress questProgress in activateQuest)
        {
            if (questProgress?.quest == null || questProgress.objectives == null)
                continue;

            foreach (QuestObjective objective in questProgress.objectives)
            {
                if (objective == null) continue;
                if (objective.type != type) continue;
                if (!string.Equals(
                    objective.objectiveID,
                    objectiveID,
                    System.StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!objective.IsCompleted)
                {
                    objective.currentAmount += amount;
                    if (objective.currentAmount > objective.requiredAmount)
                        objective.currentAmount = objective.requiredAmount;
                }

                if (questProgress.IsCompleted)
                    completedQuest = questProgress.quest;

                RefreshQuestUI();
                Debug.Log(
                    $"[QuestController] Objective '{objectiveID}' is now " +
                    $"{objective.currentAmount}/{objective.requiredAmount} " +
                    $"for quest '{questProgress.quest.questName}'.");

                return true;
            }
        }

        Debug.LogWarning(
            $"[QuestController] No active quest objective matched '{objectiveID}' " +
            $"with type '{type}'.");
        return false;
    }

    public void HandInQuest(string questID)
    {
        if (!RemoveRequiredItemsFromInventory(questID)) return;

        QuestProgress quest = activateQuest.Find(q => q.QuestID == questID);
        if (quest != null)
        {
            if (!handinQuestIDs.Contains(questID))
                handinQuestIDs.Add(questID);

            activateQuest.Remove(quest);
            RefreshQuestUI();
            // Debug.Log($"[QuestController] Quest handed in: {questID}");
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
        if (HasRuntimeQuestState())
            activateQuest = MergeQuestProgress(savedQuests, activateQuest);
        else
            activateQuest = savedQuests ?? new();

        CheckInventoryForQuests();
        RefreshQuestUI();
    }

    public void LoadHandInQuestIDs(List<string> savedHandInQuestIDs)
    {
        if (savedHandInQuestIDs == null)
            return;

        foreach (string questID in savedHandInQuestIDs)
        {
            if (string.IsNullOrEmpty(questID) || handinQuestIDs.Contains(questID))
                continue;

            handinQuestIDs.Add(questID);
        }

        activateQuest.RemoveAll(q =>
            q?.quest != null && handinQuestIDs.Contains(q.QuestID));

        RefreshQuestUI();
    }

    private bool HasRuntimeQuestState()
    {
        return (activateQuest != null && activateQuest.Count > 0) ||
               (handinQuestIDs != null && handinQuestIDs.Count > 0);
    }

    private List<QuestProgress> MergeQuestProgress(
        List<QuestProgress> savedQuests,
        List<QuestProgress> runtimeQuests)
    {
        List<QuestProgress> merged = savedQuests != null
            ? new List<QuestProgress>(savedQuests)
            : new List<QuestProgress>();

        if (runtimeQuests == null)
            return merged;

        foreach (QuestProgress runtimeQuest in runtimeQuests)
        {
            if (runtimeQuest?.quest == null)
                continue;

            QuestProgress existingQuest = merged.Find(q =>
                q?.quest != null && q.QuestID == runtimeQuest.QuestID);

            if (existingQuest == null)
            {
                merged.Add(runtimeQuest);
                continue;
            }

            MergeObjectives(existingQuest, runtimeQuest);
        }

        return merged;
    }

    private void MergeObjectives(QuestProgress savedQuest, QuestProgress runtimeQuest)
    {
        if (savedQuest.objectives == null)
            savedQuest.objectives = new List<QuestObjective>();

        if (runtimeQuest.objectives == null)
            return;

        foreach (QuestObjective runtimeObjective in runtimeQuest.objectives)
        {
            if (runtimeObjective == null)
                continue;

            QuestObjective savedObjective = savedQuest.objectives.Find(o =>
                o != null &&
                o.type == runtimeObjective.type &&
                string.Equals(
                    o.objectiveID,
                    runtimeObjective.objectiveID,
                    System.StringComparison.OrdinalIgnoreCase));

            if (savedObjective == null)
            {
                savedQuest.objectives.Add(runtimeObjective);
                continue;
            }

            savedObjective.currentAmount = Mathf.Max(
                savedObjective.currentAmount,
                runtimeObjective.currentAmount);
        }
    }

    public bool UpdateObjective(string objectiveID, ObjectiveType type, int amount = 1)
    {
        return TryCompleteObjective(objectiveID, type, out _, amount);
    }

    public void RefreshUI()
    {
        RefreshQuestUI();
    }

    // ─── UI Refresh ───────────────────────────────────────────────────
    // Finds QuestUI fresh each time in case scene changed

    private void RefreshQuestUI()
    {
        // Re-find QuestUI in case we've changed scenes or the old UI was destroyed.
        if (!IsQuestUIValid(questUI))
            questUI = FindLiveQuestUI();

        if (questUI != null)
            questUI.UpdateQuestUI();
    }

    private bool IsQuestUIValid(QuestUI ui)
    {
        try
        {
            return ui != null && ui.gameObject.scene.IsValid();
        }
        catch (MissingReferenceException)
        {
            return false;
        }
    }

    private QuestUI FindLiveQuestUI()
    {
        QuestUI[] questUIs = Resources.FindObjectsOfTypeAll<QuestUI>();

        foreach (QuestUI ui in questUIs)
        {
            if (IsQuestUIValid(ui))
                return ui;
        }

        return null;
    }
}
