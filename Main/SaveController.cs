using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    private Chest[] chests;

    void Awake()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = FindObjectOfType<InventoryController>();
        chests = FindObjectsOfType<Chest>();
    }

    // ← Move LoadGame to Start so all other Awake() singletons
    //   (BadgeController, QuestController) are guaranteed to be ready
    void Start()
    {
        LoadGame();
    }

    public void SaveGame()
    {
        if (BadgeController.Instance == null)
        {
            Debug.LogWarning("[SaveController] BadgeController not ready — skipping save.");
            return;
        }

        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            inventorySaveData = inventoryController.GetInventoryItems(),
            chestSaveData = GetChestsState(),
            questProgressData = QuestController.Instance.activateQuest,
            handInQuestIDs = QuestController.Instance.handinQuestIDs,
            earnedBadgeIDs = BadgeController.Instance.GetSaveData()
        };

        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
        
        Debug.Log("[SaveController] Game saved.");
    }

    private List<ChestSaveData> GetChestsState()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();

        foreach (Chest chest in chests)
        {
            chestStates.Add(new ChestSaveData
            {
                chestID = chest.ChestID,
                isOpened = chest.IsOpened
            });
        }

        return chestStates;
    }

    public void LoadGame()
    {
        if (!File.Exists(saveLocation))
        {
            // First time — create fresh save
            inventoryController.SetInventoryItems(new List<InventorySaveData>());
            SaveGame();
            return;
        }

        SaveData saveData = JsonUtility.FromJson<SaveData>(
            File.ReadAllText(saveLocation));

        // Player position
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            player.transform.position = saveData.playerPosition;

        // Inventory
        inventoryController.SetInventoryItems(saveData.inventorySaveData);

        // Chests
        if (saveData.chestSaveData != null)
            LoadChestStates(saveData.chestSaveData);

        // Quests
        if (QuestController.Instance != null)
        {
            QuestController.Instance.LoadQuestProgress(saveData.questProgressData);
            QuestController.Instance.handinQuestIDs = saveData.handInQuestIDs
                ?? new List<string>();
        }
        else
        {
            Debug.LogWarning("[SaveController] QuestController not ready during LoadGame.");
        }

        // Badges
        if (BadgeController.Instance != null)
        {
            BadgeController.Instance.LoadSaveData(saveData.earnedBadgeIDs);
        }
        else
        {
            Debug.LogWarning("[SaveController] BadgeController not ready during LoadGame.");
        }

        Debug.Log("[SaveController] Game loaded.");
    }

    private void LoadChestStates(List<ChestSaveData> chestStates)
    {
        foreach (Chest chest in chests)
        {
            ChestSaveData data = chestStates.FirstOrDefault(
                c => c.chestID == chest.ChestID);

            if (data != null)
                chest.SetOpened(data.isOpened);
        }
    }
}