using TMPro;
using System.Collections.Generic;
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
        RebindSceneReferences();
        if (dialoguePanel == null)
            return;

        dialoguePanel.SetActive(show);
    }

    public void SetNPCInfo(string npcName, Sprite portrait)
    {
        RebindSceneReferences();

        if (nameText != null)
            nameText.text = npcName;

        if (portraitImage != null)
            portraitImage.sprite = portrait;
    }

    // Sets the full string immediately but hides characters for the typewriter effect
    public void SetDialogueTextImmediate(string text)
    {
        RebindSceneReferences();
        if (dialogueText == null)
            return;

        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = 0;
    }

    public void SetDialogueText(string text)
    {
        RebindSceneReferences();
        if (dialogueText == null)
            return;

        dialogueText.text = text;
        dialogueText.maxVisibleCharacters = text.Length;
    }

    public void ClearChoices()
    {
        RebindSceneReferences();
        if (choiceContainer == null)
            return;

        List<GameObject> choices = new List<GameObject>();
        foreach (Transform child in choiceContainer)
            choices.Add(child.gameObject);

        foreach (GameObject choice in choices)
            Destroy(choice);
    }

    public GameObject CreateChoiceButton(string choiceText, UnityAction onClick)
    {
        RebindSceneReferences();
        if (choiceButtonPrefab == null || choiceContainer == null)
            return null;

        GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceContainer);
        choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
        choiceButton.GetComponent<Button>().onClick.AddListener(onClick);
        return choiceButton;
    }

    public bool IsReady()
    {
        RebindSceneReferences();
        return dialoguePanel != null &&
               dialogueText != null &&
               nameText != null &&
               portraitImage != null &&
               choiceContainer != null;
    }

    private void RebindSceneReferences()
    {
        if (!IsSceneObjectValid(dialoguePanel))
            dialoguePanel = FindSceneObject("DialoguePanel");

        if (!IsSceneObjectValid(nameText))
            nameText = FindSceneComponent<TMP_Text>("NPCNameText");

        if (!IsSceneObjectValid(portraitImage))
            portraitImage = FindSceneComponent<Image>("NPCPortrait");

        if (!IsSceneObjectValid(dialogueText))
            dialogueText = FindDialogueText();

        if (!IsSceneObjectValid(choiceContainer))
            choiceContainer = FindSceneTransform("ChoicesPanel");
    }

    private bool IsSceneObjectValid(Object reference)
    {
        try
        {
            if (reference == null)
                return false;

            if (reference is GameObject gameObject)
                return gameObject.scene.IsValid();

            if (reference is Component component)
                return component.gameObject.scene.IsValid();

            return true;
        }
        catch (MissingReferenceException)
        {
            return false;
        }
    }

    private TMP_Text FindDialogueText()
    {
        TMP_Text[] texts = Resources.FindObjectsOfTypeAll<TMP_Text>();

        foreach (TMP_Text text in texts)
        {
            if (text == null || !text.gameObject.scene.IsValid())
                continue;

            if (text.name == "DialogueText" || text.name == "Dialogue Text")
                return text;
        }

        foreach (TMP_Text text in texts)
        {
            if (text == null || !text.gameObject.scene.IsValid())
                continue;

            if (dialoguePanel != null && text.transform.IsChildOf(dialoguePanel.transform) &&
                text != nameText)
            {
                return text;
            }
        }

        return null;
    }

    private T FindSceneComponent<T>(string objectName) where T : Component
    {
        GameObject sceneObject = FindSceneObject(objectName);
        return sceneObject != null ? sceneObject.GetComponent<T>() : null;
    }

    private Transform FindSceneTransform(string objectName)
    {
        GameObject sceneObject = FindSceneObject(objectName);
        return sceneObject != null ? sceneObject.transform : null;
    }

    private GameObject FindSceneObject(string objectName)
    {
        Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform transform in transforms)
        {
            if (transform == null || !transform.gameObject.scene.IsValid())
                continue;

            if (transform.name == objectName)
                return transform.gameObject;
        }

        return null;
    }
}
