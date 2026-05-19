using System.Collections;
using UnityEngine;

public class NPC_Circuit : MonoBehaviour, IInteractable
{
    [Header("Dialogue Data")]
    public NPCDialogue dialogueData;

    [Header("Badge Gate")]
    [Tooltip("Badge ID required to talk to this NPC. Set to 0 for Q1 (no badge needed).")]
    public int requiredBadgeID = 0;

    [Header("Locked Dialogue")]
    [Tooltip("Message shown when player doesn't have the required badge.")]
    [TextArea] public string lockedMessage = "You need to complete the previous quest first.";

    [Header("Quest Transition")]
    [SerializeField] private bool loadSimulatorAfterAcceptingQuest = true;
    [SerializeField] private bool loadSimulatorFromInProgressDialogue;
    [SerializeField] private float questAcceptedLoadDelay = 1.5f;

    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping;
    private bool isDialogueActive;
    private bool closeDialogueAfterCurrentLine;

    private Coroutine currentTypingCoroutine;
    private Coroutine simulatorLoadCoroutine;

    private enum QuestState { NotStarted, InProgress, Completed }
    private QuestState questState = QuestState.NotStarted;

    private void Start()
    {
        dialogueUI = DialogueController.Instance;
    }

    // ─── IInteractable ────────────────────────────────────────────────

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueData == null) return;
        if (!EnsureDialogueUI()) return;
        if (PauseController.IsGamePaused && !isDialogueActive) return;

        if (isDialogueActive)
        {
            NextLine();
            return;
        }

        // Badge gate check
        BadgeController badgeController = BadgeController.Instance;
        if (requiredBadgeID != 0 && (badgeController == null || !badgeController.HasBadge(requiredBadgeID)))
        {
            ShowLockedMessage();
            return;
        }

        StartDialogue();
    }

    // ─── Locked Message ───────────────────────────────────────────────

    private void ShowLockedMessage()
    {
        if (!EnsureDialogueUI()) return;

        isDialogueActive = true;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        dialogueUI.SetDialogueText(lockedMessage);
        PauseController.SetPause(true);

        StartCoroutine(AutoCloseLockedMessage());
    }

    private IEnumerator AutoCloseLockedMessage()
    {
        float timer = 0f;
        while (timer < 2f)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space)
                || Input.GetKeyDown(KeyCode.Return))
                break;

            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);
    }

    // ─── Dialogue ─────────────────────────────────────────────────────

    private void StartDialogue()
    {
        if (!EnsureDialogueUI()) return;

        closeDialogueAfterCurrentLine = false;
        SyncQuestState();

        if (questState == QuestState.NotStarted)
            dialogueIndex = 0;
        else if (questState == QuestState.InProgress)
            dialogueIndex = dialogueData.questInProgressIndex;
        else if (questState == QuestState.Completed)
            dialogueIndex = dialogueData.questCompletedIndex;

        isDialogueActive = true;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        if (questState == QuestState.InProgress && loadSimulatorFromInProgressDialogue)
        {
            simulatorLoadCoroutine = StartCoroutine(DisplayThenLoadSimulator(dialogueIndex, true));
            return;
        }

        DisplayCurrentLines();
    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string questID = dialogueData.quest.questID;

        if (QuestController.Instance.IsQuestCompleted(questID) ||
            QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.Completed;
        }
        else if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }

    private void NextLine()
    {
        // If simulator is loading — ignore all input
        if (simulatorLoadCoroutine != null) return;

        // If still typing — reveal full line but DON'T kill simulator coroutine
        if (isTyping)
        {
            if (currentTypingCoroutine != null)
                StopCoroutine(currentTypingCoroutine);

            dialogueUI.dialogueText.maxVisibleCharacters =
                dialogueData.dialogueLines[dialogueIndex].Length;
            isTyping = false;

            if (closeDialogueAfterCurrentLine)
                currentTypingCoroutine = StartCoroutine(AutoCloseQuestAcceptedDialogue());

            return;
        }

        dialogueUI.ClearChoices();

        // Check for manual end points
        if (dialogueData.endDialogueLines.Length > dialogueIndex &&
            dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        // Check for branching choices at this line
        foreach (DialogueChoice choice in dialogueData.choices)
        {
            if (choice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(choice);
                return;
            }
        }

        // Advance to next line
        if (++dialogueIndex < dialogueData.dialogueLines.Length)
            DisplayCurrentLines();
        else
            EndDialogue();
    }

    public void EndDialogue()
    {
        // Handle quest hand-in on dialogue end
        if (questState == QuestState.Completed &&
            dialogueData.quest != null &&
            !QuestController.Instance.IsQuestHandedIn(dialogueData.quest.questID))
        {
            HandleQuestCompletion(dialogueData.quest);
        }

        // Only stop typing — never stop simulator load coroutine
        if (currentTypingCoroutine != null)
            StopCoroutine(currentTypingCoroutine);

        isDialogueActive = false;
        dialogueUI.SetDialogueText("");
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);
    }

    private void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueLines[i];

            bool givesQuest = (choice.givesQuest != null &&
                               i < choice.givesQuest.Length)
                               && choice.givesQuest[i];

            dialogueUI.CreateChoiceButton(choice.choices[i],
                () => ChooseOption(nextIndex, givesQuest));
        }
    }

    private void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest)
        {
            if (!GiveQuestBeforeSimulatorLoad())
                return;

            questState = QuestState.InProgress;
        }

        dialogueUI.ClearChoices();
        dialogueIndex = nextIndex;
        closeDialogueAfterCurrentLine = givesQuest;
        DisplayCurrentLines();
    }
    // ─── Simulator Load ───────────────────────────────────────────────

    private IEnumerator DisplayThenLoadSimulator(int lineIndex, bool forceLoad = false)
    {
        // Debug.Log($"[NPC_Circuit] DisplayThenLoadSimulator started — lineIndex: {lineIndex}");

        // ← Guard against out of bounds
        if (lineIndex >= dialogueData.dialogueLines.Length)
        {
            Debug.LogError($"[NPC_Circuit] lineIndex {lineIndex} is out of bounds! " +
                        $"Dialogue has {dialogueData.dialogueLines.Length} lines.");

            // Skip typing and load the simulator only after the quest is already active.
            yield return new WaitForSecondsRealtime(0.5f);
            CloseDialogueBeforeSimulatorLoad();
            simulatorLoadCoroutine = null;
            LoadSimulatorIfReady(forceLoad);
            yield break;
        }

        isTyping = true;
        dialogueIndex = lineIndex;

        string fullText = dialogueData.dialogueLines[lineIndex];
        // Debug.Log($"[NPC_Circuit] Typing line: '{fullText}'");

        dialogueUI.SetDialogueTextImmediate(fullText);

        for (int i = 0; i <= fullText.Length; i++)
        {
            dialogueUI.dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        isTyping = false;
        yield return new WaitForSecondsRealtime(questAcceptedLoadDelay);

        // Debug.Log("[NPC_Circuit] Closing dialogue and loading simulator...");

        CloseDialogueBeforeSimulatorLoad();
        simulatorLoadCoroutine = null;

        // Debug.Log($"[NPC_Circuit] SceneTransitionManager = {SceneTransitionManager.Instance}");

        LoadSimulatorIfReady(forceLoad);
    }

    // ─── Typing ───────────────────────────────────────────────────────

    private void DisplayCurrentLines()
    {
        if (!EnsureDialogueUI()) return;

        if (currentTypingCoroutine != null)
            StopCoroutine(currentTypingCoroutine);

        currentTypingCoroutine = StartCoroutine(TypeLine());
    }

    private IEnumerator TypeLine()
    {
        isTyping = true;

        string fullText = dialogueData.dialogueLines[dialogueIndex];
        dialogueUI.SetDialogueTextImmediate(fullText);

        for (int i = 0; i <= fullText.Length; i++)
        {
            dialogueUI.dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSecondsRealtime(dialogueData.typingSpeed);
        }

        isTyping = false;

        if (closeDialogueAfterCurrentLine)
        {
            yield return AutoCloseQuestAcceptedDialogue();
            yield break;
        }

        // Auto-progress if enabled for this line
        if (dialogueData.autoProgressLines.Length > dialogueIndex &&
            dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    // ─── Quest Completion ─────────────────────────────────────────────

    private IEnumerator AutoCloseQuestAcceptedDialogue()
    {
        closeDialogueAfterCurrentLine = false;
        yield return new WaitForSecondsRealtime(questAcceptedLoadDelay);
        currentTypingCoroutine = null;
        CloseDialogueBeforeSimulatorLoad();

        if (loadSimulatorAfterAcceptingQuest)
            LoadSimulatorIfReady(true);
    }

    private void HandleQuestCompletion(Quest quest)
    {
        if (!ValidateCircuitForQuest(quest))
        {
            ShowCircuitIncompleteMessage();
            return;
        }

        if (RewardsController.Instance == null)
        {
            Debug.LogError("[NPC_Circuit] Cannot give quest reward because RewardsController is missing.");
            return;
        }

        if (QuestController.Instance == null)
        {
            Debug.LogError("[NPC_Circuit] Cannot hand in quest because QuestController is missing.");
            return;
        }

        RewardsController.Instance.GiveQuestReward(quest);
        QuestController.Instance.HandInQuest(quest.questID);
    }

    private bool ValidateCircuitForQuest(Quest quest)
    {
        if (CircuitQuestValidator.Instance == null)
        {
            // Validator not in this scene — rely on objective completion
            return QuestController.Instance.IsQuestCompleted(quest.questID);
        }

        return CircuitQuestValidator.Instance.IsCircuitValid;
    }

    private void ShowCircuitIncompleteMessage()
    {
        if (!EnsureDialogueUI()) return;

        if (currentTypingCoroutine != null)
            StopCoroutine(currentTypingCoroutine);

        dialogueUI.SetDialogueText(
            "Hmm, it doesn't look like your circuit is working yet. " +
            "Head back to the simulator and try again!");

        StartCoroutine(AutoCloseLockedMessage());
    }

    private bool EnsureDialogueUI()
    {
        dialogueUI = DialogueController.Instance;

        if (dialogueUI == null || !dialogueUI.IsReady())
        {
            Debug.LogWarning("[NPC_Circuit] DialogueController or dialogue UI references are missing.");
            return false;
        }

        return true;
    }

    private bool GiveQuestBeforeSimulatorLoad()
    {
        if (QuestController.Instance == null)
        {
            Debug.LogError("[NPC_Circuit] Cannot give quest because QuestController is missing.");
            return false;
        }

        if (dialogueData.quest == null)
        {
            Debug.LogError("[NPC_Circuit] Cannot give quest because dialogueData.quest is missing.");
            return false;
        }

        bool accepted = QuestController.Instance.AcceptQuest(dialogueData.quest);
        if (!accepted)
            return false;

        QuestController.Instance.RefreshUI();

        SaveController saveController = FindObjectOfType<SaveController>();
        saveController?.SaveGame();

        return true;
    }

    private void CloseDialogueBeforeSimulatorLoad()
    {
        isDialogueActive = false;
        isTyping = false;

        if (dialogueUI != null)
        {
            dialogueUI.ClearChoices();
            dialogueUI.SetDialogueText("");
            dialogueUI.ShowDialogueUI(false);
        }

        PauseController.SetPause(false);
    }

    private void LoadSimulatorIfReady(bool forceLoad = false)
    {
        if (!forceLoad)
            return;

        if (SceneTransitionManager.Instance == null)
        {
            Debug.LogError("[NPC_Circuit] SceneTransitionManager is NULL!");
            return;
        }

        SceneTransitionManager.Instance.LoadSimulator();
    }
}
