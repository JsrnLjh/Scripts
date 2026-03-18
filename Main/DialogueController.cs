using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DialogueController : MonoBehaviour
{
    public static DialogueController Instance { get; private set; }

    public GameObject dialoguePanel;
    public TMP_Text dialogueText;
    public TMP_Text nameText;
    public Image portraitImage;
    public Transform choiceContainer;
    public GameObject choiceButtonPrefab;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject); 
    }

    public void ShowDialogueUI(bool show)
    {
        dialoguePanel.SetActive(show);
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        nameText.text = npcName;
        portraitImage.sprite = portrait;
    }

    // Sets the full string immediately but hides characters for the typewriter effect
    public void SetDialogueTextImmediate(string text)
    {
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = text.Length;
    }

    public void ClearChoices()
    {
        foreach (Transform child in choiceContainer)
        {
            Destroy(child.gameObject);
        }
    }

    public GameObject CreateChoiceButton(string choiceText, UnityAction onClick)
    {
        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choiceButton.GetComponent<Button>().onClick.AddListener(onClick);
        return choiceButton;
    }
}