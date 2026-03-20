using System.Collections.Generic;
using UnityEngine;

public enum CircuitQuestType
{
    None,
    SimpleLoop,         // Q1: Battery → Wire → LED → Wire → Battery
    SwitchLoop,         // Q2: Battery → Wire → Switch → Wire → LED → Wire → Battery
    ResistorLoop,       // Q3: Battery → Switch → Resistor → LED → Battery
    SeriesCircuit,      // Q4: Battery → Resistor → LED → LED → Battery
    ParallelCircuit,    // Q5: Two LEDs on separate branches
    MasterCircuit       // Q6: Battery → Switch → Resistor → LED + LED → Battery
}

public class CircuitQuestValidator : MonoBehaviour
{
    public static CircuitQuestValidator Instance { get; private set; }

    [Header("Circuit Type (auto-detected at runtime)")]
    public CircuitQuestType requiredCircuitType = CircuitQuestType.None;

    [Header("Hint Text (auto-set from quest type)")]
    [TextArea] public string hintText;

    private bool isCircuitValid = false;
    public bool IsCircuitValid => isCircuitValid;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Auto-detect which quest is active
        if (QuestController.Instance != null)
        {
            requiredCircuitType = DetectQuestType();
        }
        else
        {
            Debug.LogWarning("[CircuitQuestValidator] QuestController not found. " +
                             "Make sure it persists between scenes via DontDestroyOnLoad.");
        }

        // Auto-set hint text
        if (string.IsNullOrEmpty(hintText))
            hintText = GetDefaultHint(requiredCircuitType);

        Debug.Log($"[CircuitQuestValidator] Detected quest type: {requiredCircuitType}");
        Debug.Log($"[CircuitQuestValidator] Hint: {hintText}");

        // Push hint to UI
        if (SimulatorHintUI.Instance != null)
        {
            SimulatorHintUI.Instance.UpdateHint(hintText);
            SimulatorHintUI.Instance.UpdateStatus(false);
        }
    }

    // ─── Auto-Detect Quest Type ───────────────────────────────────────
    // Reads active objectives from QuestController to determine
    // which circuit the player needs to build right now.

    private CircuitQuestType DetectQuestType()
    {
        string[] objectiveIDs = new[]
        {
            "LightLED",
            "SwitchLED",
            "ResistorLED",
            "SeriesLED",
            "ParallelLED",
            "MasterLED"
        };

        CircuitQuestType[] types = new[]
        {
            CircuitQuestType.SimpleLoop,
            CircuitQuestType.SwitchLoop,
            CircuitQuestType.ResistorLoop,
            CircuitQuestType.SeriesCircuit,
            CircuitQuestType.ParallelCircuit,
            CircuitQuestType.MasterCircuit
        };

        for (int i = 0; i < objectiveIDs.Length; i++)
        {
            foreach (QuestProgress quest in QuestController.Instance.activateQuest)
            {
                foreach (QuestObjective obj in quest.objectives)
                {
                    if (string.Equals(obj.objectiveID, objectiveIDs[i],
                        System.StringComparison.OrdinalIgnoreCase) && !obj.IsCompleted)
                    {
                        Debug.Log($"[CircuitQuestValidator] Active objective: {objectiveIDs[i]} " +
                                  $"→ {types[i]}");
                        return types[i];
                    }
                }
            }
        }

        Debug.LogWarning("[CircuitQuestValidator] No matching active quest found. " +
                         "Defaulting to None.");
        return CircuitQuestType.None;
    }

    // ─── Main Validation Entry Point ──────────────────────────────────
    // Called by CircuitManager.EvaluateCircuit() every time
    // the circuit changes.

    public void Validate()
    {
        isCircuitValid = false;

        switch (requiredCircuitType)
        {
            case CircuitQuestType.SimpleLoop:
                isCircuitValid = ValidateSimpleLoop();
                break;
            case CircuitQuestType.SwitchLoop:
                isCircuitValid = ValidateSwitchLoop();
                break;
            case CircuitQuestType.ResistorLoop:
                isCircuitValid = ValidateResistorLoop();
                break;
            case CircuitQuestType.SeriesCircuit:
                isCircuitValid = ValidateSeriesCircuit();
                break;
            case CircuitQuestType.ParallelCircuit:
                isCircuitValid = ValidateParallelCircuit();
                break;
            case CircuitQuestType.MasterCircuit:
                isCircuitValid = ValidateMasterCircuit();
                break;
            case CircuitQuestType.None:
            default:
                isCircuitValid = false;
                break;
        }

        Debug.Log($"[CircuitQuestValidator] Validation result " +
                  $"({requiredCircuitType}): {isCircuitValid}");

        // Update UI status text
        if (SimulatorHintUI.Instance != null)
            SimulatorHintUI.Instance.UpdateStatus(isCircuitValid);

        // Update quest objective if valid
        if (isCircuitValid)
            UpdateQuestObjective();
    }

    // ─── Objective Update ─────────────────────────────────────────────
    // Pushes progress to QuestController when circuit is valid.

    private void UpdateQuestObjective()
    {
        if (QuestController.Instance == null) return;

        string objectiveID = requiredCircuitType switch
        {
            CircuitQuestType.SimpleLoop      => "LightLED",
            CircuitQuestType.SwitchLoop      => "SwitchLED",
            CircuitQuestType.ResistorLoop    => "ResistorLED",
            CircuitQuestType.SeriesCircuit   => "SeriesLED",
            CircuitQuestType.ParallelCircuit => "ParallelLED",
            CircuitQuestType.MasterCircuit   => "MasterLED",
            _ => null
        };

        if (objectiveID == null) return;

        QuestController.Instance.UpdateObjective(objectiveID, ObjectiveType.Custom);
        Debug.Log($"[CircuitQuestValidator] Objective updated: {objectiveID}");
    }

    // ─── Q1: Simple Loop ──────────────────────────────────────────────

    private bool ValidateSimpleLoop()
    {
        if (!HasClosedLoop()) return false;

        foreach (LED led in FindObjectsOfType<LED>())
        {
            if (led.isLit) return true;
        }

        Debug.Log("[Validator] Q1 failed: No LED is lit.");
        return false;
    }

    // ─── Q2: Switch Loop ──────────────────────────────────────────────

    private bool ValidateSwitchLoop()
    {
        if (!HasClosedLoop()) return false;

        if (FindObjectsOfType<Switch>().Length == 0)
        {
            Debug.Log("[Validator] Q2 failed: No switch in scene.");
            return false;
        }

        foreach (LED led in FindObjectsOfType<LED>())
        {
            if (led.isLit) return true;
        }

        Debug.Log("[Validator] Q2 failed: LED not lit.");
        return false;
    }

    // ─── Q3: Resistor Loop ────────────────────────────────────────────

    private bool ValidateResistorLoop()
    {
        if (!HasClosedLoop()) return false;

        if (FindObjectsOfType<Switch>().Length == 0)
        {
            Debug.Log("[Validator] Q3 failed: No switch in scene.");
            return false;
        }

        if (FindObjectsOfType<Resistor>().Length == 0)
        {
            Debug.Log("[Validator] Q3 failed: No resistor in scene.");
            return false;
        }

        foreach (LED led in FindObjectsOfType<LED>())
        {
            if (led.isLit && !led.isBroken) return true;
        }

        Debug.Log("[Validator] Q3 failed: LED not lit or is broken.");
        return false;
    }

    // ─── Q4: Series Circuit ───────────────────────────────────────────

    private bool ValidateSeriesCircuit()
    {
        if (!HasClosedLoop()) return false;

        if (FindObjectsOfType<Resistor>().Length == 0)
        {
            Debug.Log("[Validator] Q4 failed: No resistor in scene.");
            return false;
        }

        LED[] leds = FindObjectsOfType<LED>();
        if (leds.Length < 2)
        {
            Debug.Log("[Validator] Q4 failed: Need at least 2 LEDs.");
            return false;
        }

        int litCount = 0;
        foreach (LED led in leds)
        {
            if (led.isLit && led.isPassed) litCount++;
        }

        if (litCount < 2)
        {
            Debug.Log($"[Validator] Q4 failed: Only {litCount}/2 LEDs lit in series.");
            return false;
        }

        return true;
    }

    // ─── Q5: Parallel Circuit ─────────────────────────────────────────

    private bool ValidateParallelCircuit()
    {
        ParallelCircuitChecker checker = FindObjectOfType<ParallelCircuitChecker>();
        if (checker == null)
        {
            Debug.LogWarning("[Validator] Q5: ParallelCircuitChecker not found in scene.");
            return false;
        }

        return checker.IsParallelCircuitValid();
    }

    // ─── Q6: Master Circuit ───────────────────────────────────────────

    private bool ValidateMasterCircuit()
    {
        if (!HasClosedLoop()) return false;

        if (FindObjectsOfType<Switch>().Length == 0)
        {
            Debug.Log("[Validator] Q6 failed: No switch.");
            return false;
        }

        if (FindObjectsOfType<Resistor>().Length == 0)
        {
            Debug.Log("[Validator] Q6 failed: No resistor.");
            return false;
        }

        LED[] leds = FindObjectsOfType<LED>();
        int litCount = 0;
        foreach (LED led in leds)
        {
            if (led.isLit && !led.isBroken) litCount++;
        }

        if (litCount < 2)
        {
            Debug.Log($"[Validator] Q6 failed: Only {litCount}/2 LEDs lit.");
            return false;
        }

        return true;
    }

    // ─── Shared Helper ────────────────────────────────────────────────

    private bool HasClosedLoop()
    {
        if (FindObjectOfType<Battery>() == null)
        {
            Debug.Log("[Validator] No battery in scene.");
            return false;
        }

        foreach (CircuitComponent comp in FindObjectsOfType<CircuitComponent>())
        {
            if (comp.isPassed) return true;
        }

        Debug.Log("[Validator] No closed loop detected.");
        return false;
    }

    // ─── Hint Text ────────────────────────────────────────────────────

    public string GetDefaultHint(CircuitQuestType type)
    {
        return type switch
        {
            CircuitQuestType.SimpleLoop =>
                "Battery  →  Wire  →  LED  →  Wire  →  Battery",
            CircuitQuestType.SwitchLoop =>
                "Battery  →  Wire  →  Switch  →  Wire  →  LED  →  Wire  →  Battery",
            CircuitQuestType.ResistorLoop =>
                "Battery  →  Switch  →  Resistor  →  LED  →  Battery",
            CircuitQuestType.SeriesCircuit =>
                "Battery  →  Resistor  →  LED  →  LED  →  Battery  (series)",
            CircuitQuestType.ParallelCircuit =>
                "Battery  →  [ LED | LED ]  →  Battery  (parallel — two separate branches)",
            CircuitQuestType.MasterCircuit =>
                "Battery  →  Switch  →  Resistor  →  LED + LED  →  Battery",
            _ => "No active quest found. Return to the main scene."
        };
    }

    // ─── Public Accessors ─────────────────────────────────────────────

    public string GetHintText() => hintText;

    // ─── Debug ────────────────────────────────────────────────────────

    [ContextMenu("Debug: Force Redetect Quest Type")]
    private void Debug_RedetectQuestType()
    {
        if (QuestController.Instance != null)
        {
            requiredCircuitType = DetectQuestType();
            hintText = GetDefaultHint(requiredCircuitType);
            SimulatorHintUI.Instance?.UpdateHint(hintText);
            Debug.Log($"[CircuitQuestValidator] Redetected: {requiredCircuitType}");
        }
    }

    [ContextMenu("Debug: Force Validate")]
    private void Debug_ForceValidate()
    {
        Validate();
    }
}