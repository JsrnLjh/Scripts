using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public List<InventorySaveData> inventorySaveData;
    // public List<InventorySaveData> hotbarSaveData;
    public List<ChestSaveData> chestSaveData;
    public List<QuestProgress> questProgressData;
    public List<string> handInQuestIDs;
    public List<int> earnedBadgeIDs;
}

[System.Serializable]
public class ChestSaveData
{
    public string chestID;
    public bool isOpened;
}