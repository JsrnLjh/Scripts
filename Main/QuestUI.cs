using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        RebindSceneReferences();

        if (questListContent == null || questEntryPrefab == null || objectiveTextPrefab == null)
            return;

        // Clear existing entries
        List<GameObject> existingEntries = new List<GameObject>();
        foreach (Transform child in questListContent)
            existingEntries.Add(child.gameObject);

        foreach (GameObject entry in existingEntries)
            Destroy(entry);

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

    private void RebindSceneReferences()
    {
        if (IsSceneReferenceValid(questListContent))
            return;

        questListContent = FindQuestListContent();
    }

    private bool IsSceneReferenceValid(Transform reference)
    {
        try
        {
            return reference != null && reference.gameObject.scene.IsValid();
        }
        catch (MissingReferenceException)
        {
            return false;
        }
    }

    private Transform FindQuestListContent()
    {
        ScrollRect[] scrollRects = Resources.FindObjectsOfTypeAll<ScrollRect>();

        foreach (ScrollRect scrollRect in scrollRects)
        {
            if (scrollRect == null || !scrollRect.gameObject.scene.IsValid())
                continue;

            if (scrollRect.name != "QuestScrollView")
                continue;

            if (scrollRect.content != null)
                return scrollRect.content;
        }

        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform == null || !transform.gameObject.scene.IsValid())
                continue;

            if (transform.name == "Content" &&
                transform.GetComponentInParent<ScrollRect>(true)?.name == "QuestScrollView")
            {
                return transform;
            }
        }

        return null;
    }
}
