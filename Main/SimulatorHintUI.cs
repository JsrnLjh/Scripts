using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SimulatorHintUI : MonoBehaviour
{
    public static SimulatorHintUI Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public TMP_Text hintText;
    public TMP_Text feedbackText;
    public TMP_Text successPopup;
    public GameObject completionPanel;
    public Button returnButton;
    public TMP_Text completionTitleText;
    public TMP_Text completionMessageText;

    private const string CompletionMessage = "Quest Completed, you may now go back to the world";

    [Header("Feedback Colors")]
    public Color completeColor = new Color(0f, 0.85f, 0f, 1f);
    public Color incompleteColor = new Color(1f, 1f, 1f, 1f);
    public Color titleColor = new Color(1f, 1f, 1f, 1f);

    private bool completionShown;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        AutoAssignCompletionUI();

        if (completionPanel != null)
            completionPanel.SetActive(false);

        if (returnButton != null)
        {
            returnButton.gameObject.SetActive(false);
            returnButton.onClick.AddListener(ReturnToMainScene);
        }
    }

    private void AutoAssignCompletionUI()
    {
        if (completionPanel == null)
        {
            Transform panel = FindChildByPath("QuestPanel/CompletionPanel");
            if (panel == null)
                panel = FindChildByName("CompletionPanel");

            if (panel != null)
                completionPanel = panel.gameObject;
        }

        if (completionPanel == null)
            return;

        if (returnButton == null)
        {
            Transform button = completionPanel.transform.Find("Return to World");
            if (button == null)
                button = completionPanel.transform.Find("Back to World");

            if (button != null)
                returnButton = button.GetComponent<Button>();
        }

        if (returnButton == null)
            returnButton = FindReturnButton(completionPanel);

        TMP_Text[] texts = completionPanel.GetComponentsInChildren<TMP_Text>(true);
        foreach (TMP_Text text in texts)
        {
            if (text == null)
                continue;

            if (completionTitleText == null &&
                (text.name.Contains("CongratulationText") ||
                 text.name.Contains("Congratulations") ||
                 text.name.Contains("Congratulation")))
            {
                completionTitleText = text;
            }
            else if (completionMessageText == null && text.name.Contains("Text"))
            {
                completionMessageText = text;
            }
        }
    }

    private Transform FindChildByPath(string path)
    {
        Transform current = transform;

        while (current != null)
        {
            Transform found = current.Find(path);
            if (found != null)
                return found;

            current = current.parent;
        }

        return null;
    }

    private Transform FindChildByName(string childName)
    {
        Transform[] children = Resources.FindObjectsOfTypeAll<Transform>();

        foreach (Transform child in children)
        {
            if (child.gameObject.scene.IsValid() && child.name == childName)
                return child;
        }

        return null;
    }

    private Button FindReturnButton(GameObject panel)
    {
        Button[] buttons = panel.GetComponentsInChildren<Button>(true);

        foreach (Button button in buttons)
        {
            if (button == null)
                continue;

            if (button.name.Contains("Return") || button.name.Contains("Back"))
                return button;

            TMP_Text label = button.GetComponentInChildren<TMP_Text>(true);
            if (label != null &&
                (label.text.Contains("Return") || label.text.Contains("Back")))
            {
                return button;
            }
        }

        return buttons.Length > 0 ? buttons[0] : null;
    }

    private void Start()
    {
        PopulateFromActiveQuest();

        if (CircuitQuestValidator.Instance != null)
            UpdateHint(CircuitQuestValidator.Instance.GetHintText());

        CircuitManager.Instance?.EvaluateCircuit();
    }

    private void Update()
    {
        if (!completionShown && HasLitLED())
        {
            ShowCompletion();
        }
    }

    private bool HasLitLED()
    {
        LED[] leds = FindObjectsOfType<LED>();

        foreach (LED led in leds)
        {
            if (led != null && led.isPowered)
                return true;
        }

        return false;
    }

    private void PopulateFromActiveQuest()
    {
        if (QuestController.Instance == null ||
            QuestController.Instance.activateQuest.Count == 0)
        {
            SetTitle("Lit the LED");
            SetDescription("Connect a battery to an LED with wires.");
            return;
        }

        QuestProgress activeQuest = GetActiveCircuitQuest();

        if (activeQuest == null)
        {
            SetTitle("Lit the LED");
            SetDescription("Connect a battery to an LED with wires.");
            return;
        }

        SetTitle(activeQuest.quest.questName);
        SetDescription(activeQuest.quest.description);
    }

    private QuestProgress GetActiveCircuitQuest()
    {
        string[] circuitObjectiveIDs =
        {
            "LightLED",
            "SwitchLED",
            "ResistorLED",
            "SeriesLED",
            "ParallelLED",
            "MasterLED"
        };

        foreach (QuestProgress quest in QuestController.Instance.activateQuest)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                foreach (string id in circuitObjectiveIDs)
                {
                    if (string.Equals(
                        objective.objectiveID,
                        id,
                        System.StringComparison.OrdinalIgnoreCase) && !objective.IsCompleted)
                    {
                        return quest;
                    }
                }
            }
        }

        return null;
    }

    public void UpdateHint(string hint)
    {
        if (hintText != null)
            hintText.text = hint;
    }

    public void UpdateStatus(bool isValid)
    {
        UpdateFeedback(isValid);
        UpdateSuccessPopup(isValid);

        if (isValid)
            ShowCompletion();
    }

    private void ShowCompletion()
    {
        if (completionShown)
            return;

        AutoAssignCompletionUI();

        if (completionPanel == null && returnButton == null && completionTitleText == null)
            Debug.LogWarning("[SimulatorHintUI] LED is lit, but completion UI references are missing.");

        completionShown = true;

        if (completionPanel != null)
        {
            completionPanel.SetActive(true);
            SetChildrenActive(completionPanel.transform, true);
        }

        if (returnButton != null)
        {
            returnButton.gameObject.SetActive(true);
            returnButton.interactable = true;
        }

        Debug.Log(
            $"[SimulatorHintUI] Showing completion UI. " +
            $"Panel={(completionPanel != null ? completionPanel.name : "missing")}, " +
            $"Button={(returnButton != null ? returnButton.name : "missing")}"
        );

        if (successPopup != null)
        {
            successPopup.text = "Congratulations! Circuit complete. Return to the NPC to claim your reward.";
            successPopup.color = completeColor;
        }

        if (completionTitleText != null)
            completionTitleText.text = CompletionMessage;

        if (completionMessageText != null)
            completionMessageText.text = CompletionMessage;
    }

    private void SetChildrenActive(Transform parent, bool active)
    {
        foreach (Transform child in parent)
        {
            child.gameObject.SetActive(active);
            SetChildrenActive(child, active);
        }
    }

    public void ReturnToMainScene()
    {
        if (SceneTransitionManager.Instance == null)
        {
            GameObject sceneTransition = new GameObject("SceneTransitionManager");
            sceneTransition.AddComponent<SceneTransitionManager>();
        }

        SceneTransitionManager.Instance.ReturnToMain();
    }

    private void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
            titleText.color = titleColor;
        }
    }

    private void SetDescription(string description)
    {
        if (descriptionText != null)
            descriptionText.text = description;
    }

    private void UpdateFeedback(bool isValid)
    {
        if (feedbackText == null)
            return;

        feedbackText.text = isValid ? "COMPLETE" : "INCOMPLETE";
        feedbackText.color = isValid ? completeColor : incompleteColor;
    }

    private void UpdateSuccessPopup(bool isValid)
    {
        if (successPopup == null)
            return;

        if (isValid)
        {
            successPopup.text = "Congratulations! Circuit complete. Return to the NPC to claim your reward.";
            successPopup.color = completeColor;
        }
        else
        {
            successPopup.text = "Build the circuit shown in the hint above.";
            successPopup.color = incompleteColor;
        }
    }
}
