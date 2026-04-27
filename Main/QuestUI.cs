using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    void Start()
    {
        UpdateQuestUI();
    }

    public void UpdateQuestUI()
    {
        if (QuestController.Instance == null) return;

        // Clear existing entries
        foreach (Transform child in questListContent)
            Destroy(child.gameObject);

        // Rebuild from active quests
        foreach (var quest in QuestController.Instance.activateQuest)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);

            TMP_Text questNameText = entry.transform
                .Find("QuestName")?.GetComponent<TMP_Text>();

            Transform objectiveList = entry.transform.Find("ObjectiveList");

            if (questNameText != null)
                questNameText.text = quest.quest.questName; // ← use questName not name

            if (objectiveList == null) continue;

            foreach (var objective in quest.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();

                if (objText != null)
                    objText.text = $"{objective.description} " +
                                   $"({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }
}