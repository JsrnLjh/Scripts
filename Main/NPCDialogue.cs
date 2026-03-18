using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName ="NewNPCDialogue", menuName ="NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;
    public bool[] endDialogueLines;
    public float typingSpeed = 0.05f;
    // public AudioClip voiceSound;
    public float autoProgressDelay = 1.5f;

    public DialogueChoice[] choices;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex; // Dialogue line where choices appear
    public string[] choices;
    public int[] nextDialogueLines; 
}