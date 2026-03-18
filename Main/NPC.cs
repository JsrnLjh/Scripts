using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
    }

    public bool CanInteract()
    {
        return !isDialogueActive;   
    }

    public void Interact()
    {
        if(dialogueData == null || (PauseController.IsGamePaused && !isDialogueActive))
            return;  

        if(isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }

    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        DisplayCurrentLines();
    }

    void NextLine()
    {
        // If still typing, reveal the full line immediately on click
        if(isTyping)
        {
            StopAllCoroutines();
            dialogueUI.dialogueText.maxVisibleCharacters = dialogueData.dialogueLines[dialogueIndex].Length;
            isTyping = false;
            return; 
        }

        dialogueUI.ClearChoices();

        // Check for manual end points defined in the Inspector
        if(dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        // Check if this specific line should trigger branching choices
        foreach(DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if(dialogueChoice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoice);               
                return;
            }
        }

        // Advance to next line if available
        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLines();
        }
        else
        {
            EndDialogue(); 
        }
    }

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);
    }

    void DisplayChoices(DialogueChoice choice)
    {
        for(int i=0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueLines[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex));
        }
    }

    void ChooseOption(int nextIndex)
    {
        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        DisplayCurrentLines();
    }

    void DisplayCurrentLines()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        
        string fullText = dialogueData.dialogueLines[dialogueIndex];
        dialogueUI.SetDialogueTextImmediate(fullText);

        // Reveal character by character based on maxVisibleCharacters
        for (int i = 0; i <= fullText.Length; i++)
        {
            dialogueUI.dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }

        isTyping = false;

        // Handle auto-progression if enabled for this line
        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }
}