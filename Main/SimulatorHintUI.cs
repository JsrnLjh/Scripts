using TMPro;
using UnityEngine;

public class SimulatorHintUI : MonoBehaviour
{
    public static SimulatorHintUI Instance { get; private set; }

    [Header("UI References")]
    public TMP_Text titleText;        // Quest name
    public TMP_Text descriptionText;  // Quest description
    public TMP_Text hintText;         // Battery → Wire → LED chain
    public TMP_Text feedbackText;     // COMPLETE / INCOMPLETE
    public TMP_Text successPopup;     // changes color to green when valid

    [Header("Feedback Colors")]
    public Color completeColor   = new Color(0f, 0.85f, 0f, 1f);    // green
    public Color incompleteColor = new Color(1f, 1f, 1f, 1f);        // white
    public Color titleColor      = new Color(1f, 1f, 1f, 1f);        // white

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        PopulateFromActiveQuest();

        if (CircuitQuestValidator.Instance != null)
            UpdateHint(CircuitQuestValidator.Instance.GetHintText());

        UpdateStatus(false);
    }

    // ─── Populate from QuestController ───────────────────────────────
    // Reads the active quest and fills Title + Description automatically

    private void PopulateFromActiveQuest()
    {
        if (QuestController.Instance == null ||
            QuestController.Instance.activateQuest.Count == 0)
        {
            SetTitle("No Active Quest");
            SetDescription("Return to the main scene and talk to an NPC.");
            return;
        }

        // Get the first active quest that matches a circuit objective
        QuestProgress activeQuest = GetActiveCircuitQuest();

        if (activeQuest == null)
        {
            SetTitle("No Circuit Quest");
            SetDescription("Talk to an NPC in the main scene to get a quest.");
            return;
        }

        SetTitle(activeQuest.quest.questName);
        SetDescription(activeQuest.quest.description);
    }

    private QuestProgress GetActiveCircuitQuest()
    {
        string[] circuitObjectiveIDs = new[]
        {
            "LightLED", "SwitchLED", "ResistorLED",
            "SeriesLED", "ParallelLED", "MasterLED"
        };

        foreach (QuestProgress quest in QuestController.Instance.activateQuest)
        {
            foreach (QuestObjective obj in quest.objectives)
            {
                foreach (string id in circuitObjectiveIDs)
                {
                    if (string.Equals(obj.objectiveID, id,
                        System.StringComparison.OrdinalIgnoreCase) && !obj.IsCompleted)
                    {
                        return quest;
                    }
                }
            }
        }

        return null;
    }

    // ─── Public Update Methods ────────────────────────────────────────
    // Called by CircuitQuestValidator

    public void UpdateHint(string hint)
    {
        if (hintText != null)
            hintText.text = hint;
    }

    public void UpdateStatus(bool isValid)
    {
        UpdateFeedback(isValid);
        UpdateSuccessPopup(isValid);
    }

    // ─── Internal Setters ─────────────────────────────────────────────

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
        if (feedbackText == null) return;

        if (isValid)
        {
            feedbackText.text = "COMPLETE";
            feedbackText.color = completeColor;
        }
        else
        {
            feedbackText.text = "INCOMPLETE";
            feedbackText.color = incompleteColor;
        }
    }

    private void UpdateSuccessPopup(bool isValid)
    {
        if (successPopup == null) return;

        if (isValid)
        {
            successPopup.text = "Circuit complete! Return to the NPC to claim your reward.";
            successPopup.color = completeColor;
        }
        else
        {
            successPopup.text = "Build the circuit shown in the hint above.";
            successPopup.color = incompleteColor;
        }
    }
}