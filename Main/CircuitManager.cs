using System.Collections.Generic;
using UnityEngine;

public class CircuitManager : MonoBehaviour
{
    public static CircuitManager Instance;

    private CircuitComponent[] allComponents;
    private Battery battery;

    private void Awake()
    {
        Instance = this;
    }

    public void EvaluateCircuit()
    {
        Debug.Log("[CircuitManager] EvaluateCircuit called");

        RefreshComponents();
        ResetAllComponents();

        // ← These guards must be here
        if (battery == null)
        {
            Debug.LogWarning("[CircuitManager] No Battery in scene.");
            if (CircuitQuestValidator.Instance != null)
                CircuitQuestValidator.Instance.Validate();
            return;
        }

        if (battery.positiveTerminal == null || battery.negativeTerminal == null)
        {
            Debug.LogWarning("[CircuitManager] Battery terminals not assigned.");
            return;
        }

        Terminal positive = battery.positiveTerminal.GetComponent<Terminal>();
        Terminal negative = battery.negativeTerminal.GetComponent<Terminal>();

        if (positive == null || negative == null)
        {
            Debug.LogWarning("[CircuitManager] Terminal.cs missing on battery terminals.");
            return;
        }

        HashSet<Terminal> visited = new HashSet<Terminal>();
        List<CircuitComponent> pathComponents = new List<CircuitComponent>();

        bool closedLoop = TracePath(positive, negative, visited, pathComponents);
        Debug.Log($"[CircuitManager] Closed loop: {closedLoop}");

        // ← THIS IS THE MISSING CHECK
        if (!closedLoop)
        {
            Debug.Log("[CircuitManager] Circuit is OPEN.");
            if (CircuitQuestValidator.Instance != null)
                CircuitQuestValidator.Instance.Validate();
            return;
        }

        // Calculate total resistance
        float totalResistance = 0f;
        foreach (CircuitComponent comp in pathComponents)
            totalResistance += comp.GetResistance();

        if (totalResistance <= 0f)
            totalResistance = 0.01f;

        float current = battery.voltageOutput / totalResistance;
        float remainingVoltage = battery.voltageOutput;

        Debug.Log($"[CircuitManager] Closed. R={totalResistance}, I={current}A");

        // Evaluate each component with its own voltage drop
        foreach (CircuitComponent comp in pathComponents)
        {
            float voltageDrop = current * comp.GetResistance();
            comp.voltageDrop = voltageDrop;
            comp.isPassed = true;
            comp.Evaluate(voltageDrop, current);  // ← pass voltage DROP, not battery voltage
            remainingVoltage -= voltageDrop;
        }

        if (CircuitQuestValidator.Instance != null)
        CircuitQuestValidator.Instance.Validate();
    }

    private bool TracePath(
        Terminal current,
        Terminal target,
        HashSet<Terminal> visited,
        List<CircuitComponent> pathComponents)
    {
        if (current == null || visited.Contains(current)) return false;

        Debug.Log($"[TracePath] Visiting: {current.name}");
        visited.Add(current);

        if (current == target)
        {
            Debug.Log("[TracePath] Reached negative terminal — circuit closed!");
            return true;
        }

        Terminal connected = current.connectedTerminal;
        if (connected == null)
        {
            Debug.Log($"[TracePath] {current.name} has no connection.");
            return false;
        }

        CircuitComponent owner = connected.owner;
        if (owner == null)
        {
            Debug.Log($"[TracePath] Connected terminal {connected.name} has no owner.");
            return false;
        }

        // ✅ Check BEFORE adding to path — fixes the ordering bug
        if (!owner.CanPassCurrent())
        {
            Debug.Log($"[TracePath] Blocked by {owner.name} (CanPassCurrent = false)");
            return false;
        }

        if (owner != battery && !pathComponents.Contains(owner))
            pathComponents.Add(owner);

        if (owner.terminals == null || owner.terminals.Length == 0)
        {
            Debug.Log($"[TracePath] {owner.name} has no terminals.");
            return false;
        }

        foreach (Transform t in owner.terminals)
        {
            if (t == null) continue;

            Terminal next = t.GetComponent<Terminal>();
            if (next != null && next != connected)
            {
                if (TracePath(next, target, visited, pathComponents))
                    return true;
            }
        }

        return false;
    }

    private void RefreshComponents()
    {
        allComponents = FindObjectsOfType<CircuitComponent>();
        battery = FindObjectOfType<Battery>();
    }

    private void ResetAllComponents()
    {
        foreach (CircuitComponent comp in allComponents)
        {
            if (comp != null) comp.ResetState();
        }

        foreach (LED led in FindObjectsOfType<LED>())
        {
            if (led != null && !led.isBroken)
                led.FullReset();
        }
    }

    private void ValidateQuestProgress()
    {
        if (CircuitQuestValidator.Instance == null) return;

        // Only mark objective if circuit is fully valid for the quest type
        if (!CircuitQuestValidator.Instance.IsCircuitValid) return;

        CircuitQuestType questType = CircuitQuestValidator.Instance.requiredCircuitType;

        string objectiveID = questType switch
        {
            CircuitQuestType.SimpleLoop     => "LightLED",
            CircuitQuestType.SwitchLoop     => "SwitchLED",
            CircuitQuestType.ResistorLoop   => "ResistorLED",
            CircuitQuestType.SeriesCircuit  => "SeriesLED",
            CircuitQuestType.ParallelCircuit => "ParallelLED",
            CircuitQuestType.MasterCircuit  => "MasterLED",
            _ => null
        };

        if (objectiveID == null) return;

        QuestController.Instance?.UpdateObjective(objectiveID, ObjectiveType.Custom);
        Debug.Log($"[CircuitManager] Objective updated: {objectiveID}");
    }
}