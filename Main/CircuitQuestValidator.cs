using UnityEngine;

public enum CircuitQuestType
{
    None,
    SimpleLoop,
    SwitchLoop,
    ResistorLoop,
    SeriesCircuit,
    ParallelCircuit,
    MasterCircuit
}

public class CircuitQuestValidator : MonoBehaviour
{
    public static CircuitQuestValidator Instance { get; private set; }

    [Header("Circuit Type")]
    public CircuitQuestType requiredCircuitType = CircuitQuestType.None;

    [Header("Hint Text")]
    [TextArea] public string hintText;

    private bool isCircuitValid;
    public bool IsCircuitValid => isCircuitValid;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (QuestController.Instance != null)
            requiredCircuitType = DetectQuestType();

        if (requiredCircuitType == CircuitQuestType.None)
            requiredCircuitType = CircuitQuestType.SimpleLoop;

        if (string.IsNullOrEmpty(hintText))
            hintText = GetDefaultHint(requiredCircuitType);

        GetSimulatorHintUI()?.UpdateHint(hintText);
        CircuitManager.Instance?.EvaluateCircuit();
    }

    private CircuitQuestType DetectQuestType()
    {
        if (QuestController.Instance == null)
            return CircuitQuestType.None;

        string[] objectiveIDs =
        {
            "LightLED",
            "SwitchLED",
            "ResistorLED",
            "SeriesLED",
            "ParallelLED",
            "MasterLED"
        };

        CircuitQuestType[] types =
        {
            CircuitQuestType.SimpleLoop,
            CircuitQuestType.SwitchLoop,
            CircuitQuestType.ResistorLoop,
            CircuitQuestType.SeriesCircuit,
            CircuitQuestType.ParallelCircuit,
            CircuitQuestType.MasterCircuit
        };

        foreach (QuestProgress quest in QuestController.Instance.activateQuest)
        {
            foreach (QuestObjective objective in quest.objectives)
            {
                for (int i = 0; i < objectiveIDs.Length; i++)
                {
                    if (objective.IsCompleted)
                        continue;

                    if (string.Equals(
                        objective.objectiveID,
                        objectiveIDs[i],
                        System.StringComparison.OrdinalIgnoreCase))
                    {
                        return types[i];
                    }
                }
            }
        }

        return CircuitQuestType.None;
    }

    public void Validate()
    {
        CircuitQuestType detectedQuestType = DetectQuestType();
        if (detectedQuestType != CircuitQuestType.None)
            requiredCircuitType = detectedQuestType;

        isCircuitValid = requiredCircuitType switch
        {
            CircuitQuestType.SimpleLoop => ValidateSimpleLoop(),
            CircuitQuestType.SwitchLoop => ValidateSwitchLoop(),
            CircuitQuestType.ResistorLoop => ValidateResistorLoop(),
            CircuitQuestType.SeriesCircuit => ValidateSeriesCircuit(),
            CircuitQuestType.ParallelCircuit => ValidateParallelCircuit(),
            CircuitQuestType.MasterCircuit => ValidateMasterCircuit(),
            CircuitQuestType.None => ValidateSimpleLoop(),
            _ => ValidateSimpleLoop()
        };

        Debug.Log($"[CircuitQuestValidator] {requiredCircuitType} valid = {isCircuitValid}");

        bool questObjectiveCompleted = true;
        if (isCircuitValid)
            questObjectiveCompleted = UpdateQuestObjective();

        GetSimulatorHintUI()?.UpdateStatus(isCircuitValid && questObjectiveCompleted);
    }

    private bool ValidateSimpleLoop()
    {
        return HasBattery() && HasPoweredLED();
    }

    private bool ValidateSwitchLoop()
    {
        return HasBattery()
            && HasComponent<Switch>()
            && HasPoweredLED();
    }

    private bool ValidateResistorLoop()
    {
        return HasBattery()
            && HasComponent<Switch>()
            && HasComponent<Resistor>()
            && HasPoweredLED();
    }

    private bool ValidateSeriesCircuit()
    {
        return HasBattery()
            && HasComponent<Resistor>()
            && CountPoweredLEDs() >= 2;
    }

    private bool ValidateParallelCircuit()
    {
        ParallelCircuitChecker checker = FindObjectOfType<ParallelCircuitChecker>();
        if (checker != null)
            return checker.IsParallelCircuitValid();

        return HasBattery() && CountPoweredLEDs() >= 2;
    }

    private bool ValidateMasterCircuit()
    {
        return HasBattery()
            && HasComponent<Switch>()
            && HasComponent<Resistor>()
            && CountPoweredLEDs() >= 2;
    }

    private bool HasBattery()
    {
        return FindObjectOfType<Battery>() != null;
    }

    private bool HasComponent<T>() where T : Component
    {
        return FindObjectOfType<T>() != null;
    }

    private bool HasPoweredLED()
    {
        foreach (LED led in FindObjectsOfType<LED>())
        {
            if (led != null && led.isPowered)
                return true;
        }

        return false;
    }

    private int CountPoweredLEDs()
    {
        int count = 0;

        foreach (LED led in FindObjectsOfType<LED>())
        {
            if (led != null && led.isPowered)
                count++;
        }

        return count;
    }

    private bool UpdateQuestObjective()
    {
        if (QuestController.Instance == null)
        {
            Debug.LogWarning("[CircuitQuestValidator] Cannot complete quest because QuestController is missing. Start from the main scene/NPC quest flow.");
            return false;
        }

        string objectiveID = GetObjectiveID(requiredCircuitType);

        if (string.IsNullOrEmpty(objectiveID))
        {
            Debug.LogWarning($"[CircuitQuestValidator] Cannot complete quest because no objective ID maps to circuit type '{requiredCircuitType}'.");
            return false;
        }

        if (IsQuestObjectiveAlreadyCompleted(objectiveID))
            return true;

        bool objectiveUpdated = QuestController.Instance.TryCompleteObjective(
            objectiveID,
            ObjectiveType.Custom,
            out Quest completedQuest);

        if (objectiveUpdated)
        {
            SaveController saveController = FindObjectOfType<SaveController>();
            saveController?.SaveGame();
        }

        return objectiveUpdated;
    }

    private string GetObjectiveID(CircuitQuestType questType)
    {
        return questType switch
        {
            CircuitQuestType.SimpleLoop => "LightLED",
            CircuitQuestType.SwitchLoop => "SwitchLED",
            CircuitQuestType.ResistorLoop => "ResistorLED",
            CircuitQuestType.SeriesCircuit => "SeriesLED",
            CircuitQuestType.ParallelCircuit => "ParallelLED",
            CircuitQuestType.MasterCircuit => "MasterLED",
            _ => null
        };
    }

    private bool IsQuestObjectiveAlreadyCompleted(string objectiveID)
    {
        foreach (QuestProgress questProgress in QuestController.Instance.activateQuest)
        {
            if (QuestHasCompletedObjective(questProgress, objectiveID))
                return true;
        }

        return false;
    }

    private bool QuestHasCompletedObjective(QuestProgress questProgress, string objectiveID)
    {
        if (questProgress == null || questProgress.objectives == null)
            return false;

        foreach (QuestObjective objective in questProgress.objectives)
        {
            if (objective == null || !objective.IsCompleted)
                continue;

            if (string.Equals(
                objective.objectiveID,
                objectiveID,
                System.StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    public string GetDefaultHint(CircuitQuestType type)
    {
        return type switch
        {
            CircuitQuestType.SimpleLoop =>
                "Connect a battery to an LED with wires.",
            CircuitQuestType.SwitchLoop =>
                "Connect a battery, switch, LED, and wires. Close the switch to light the LED.",
            CircuitQuestType.ResistorLoop =>
                "Connect a battery, switch, resistor, LED, and wires. Close the switch to light the LED.",
            CircuitQuestType.SeriesCircuit =>
                "Connect a battery, resistor, and at least two LEDs so both LEDs light.",
            CircuitQuestType.ParallelCircuit =>
                "Connect a battery and at least two LEDs so both LEDs light.",
            CircuitQuestType.MasterCircuit =>
                "Connect a battery, switch, resistor, and at least two LEDs. Close the switch so both LEDs light.",
            _ => "No active circuit quest found."
        };
    }

    public string GetHintText()
    {
        return hintText;
    }

    // [ContextMenu("Debug: Force Redetect Quest Type")]
    private void Debug_RedetectQuestType()
    {
        if (QuestController.Instance == null)
            return;

        requiredCircuitType = DetectQuestType();
        if (requiredCircuitType == CircuitQuestType.None)
            requiredCircuitType = CircuitQuestType.SimpleLoop;

        hintText = GetDefaultHint(requiredCircuitType);
        GetSimulatorHintUI()?.UpdateHint(hintText);
    }

    // [ContextMenu("Debug: Force Validate")]
    private void Debug_ForceValidate()
    {
        Validate();
    }

    private SimulatorHintUI GetSimulatorHintUI()
    {
        if (SimulatorHintUI.Instance != null)
            return SimulatorHintUI.Instance;

        SimulatorHintUI[] uiObjects = Resources.FindObjectsOfTypeAll<SimulatorHintUI>();
        foreach (SimulatorHintUI ui in uiObjects)
        {
            if (ui != null && ui.gameObject.scene.IsValid())
                return ui;
        }

        return null;
    }
}
