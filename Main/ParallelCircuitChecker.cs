using System.Collections.Generic;
using UnityEngine;

public class ParallelCircuitChecker : MonoBehaviour
{
    // ─── Main Entry Point ─────────────────────────────────────────────
    // Called by CircuitQuestValidator.ValidateParallelCircuit()

    public bool IsParallelCircuitValid()
    {
        Battery battery = FindObjectOfType<Battery>();
        if (battery == null)
        {
            Debug.Log("[ParallelChecker] No battery found.");
            return false;
        }

        LED[] leds = FindObjectsOfType<LED>();
        if (leds.Length < 2)
        {
            Debug.Log("[ParallelChecker] Need at least 2 LEDs.");
            return false;
        }

        // Step 1 — Both LEDs must be lit
        if (!AllLEDsLit(leds))
        {
            Debug.Log("[ParallelChecker] Not all LEDs are lit.");
            return false;
        }

        // Step 2 — LEDs must be on separate branches (not in series)
        if (!LEDsAreOnSeparateBranches(leds, battery))
        {
            Debug.Log("[ParallelChecker] LEDs are not on separate branches.");
            return false;
        }

        Debug.Log("[ParallelChecker] Valid parallel circuit detected!");
        return true;
    }

    // ─── Check 1: All LEDs Lit ────────────────────────────────────────

    private bool AllLEDsLit(LED[] leds)
    {
        foreach (LED led in leds)
        {
            if (!led.isLit || led.isBroken)
                return false;
        }
        return true;
    }

    // ─── Check 2: LEDs on Separate Branches ──────────────────────────
    // Core parallel detection logic.
    // In a series circuit, both LEDs share the SAME terminal path.
    // In a parallel circuit, each LED has its OWN independent path
    // from the battery positive back to the battery negative.
    // We verify this by tracing from each LED's anode terminal
    // back to the battery positive WITHOUT passing through the other LED.

    private bool LEDsAreOnSeparateBranches(LED[] leds, Battery battery)
    {
        Terminal positive = battery.positiveTerminal.GetComponent<Terminal>();
        Terminal negative = battery.negativeTerminal.GetComponent<Terminal>();

        if (positive == null || negative == null)
        {
            Debug.Log("[ParallelChecker] Battery terminals missing Terminal component.");
            return false;
        }

        // For each LED, check it has its own independent path to battery
        for (int i = 0; i < leds.Length; i++)
        {
            LED currentLED = leds[i];

            // Build exclusion set — all OTHER LEDs' terminals
            HashSet<Terminal> excludedTerminals = GetOtherLEDTerminals(leds, i);

            // Try to trace a path from battery positive to battery negative
            // that passes through this LED but NOT through any other LED
            HashSet<Terminal> visited = new HashSet<Terminal>();
            bool hasOwnPath = TraceIndependentPath(
                positive,
                negative,
                currentLED,
                excludedTerminals,
                visited
            );

            if (!hasOwnPath)
            {
                Debug.Log($"[ParallelChecker] LED {currentLED.name} has no independent path.");
                return false;
            }

            Debug.Log($"[ParallelChecker] LED {currentLED.name} has its own branch.");
        }

        return true;
    }

    // ─── Independent Path Tracer ──────────────────────────────────────
    // Traces from 'current' terminal to 'target' terminal,
    // must pass through 'requiredLED' and must NOT pass through
    // any terminal in 'excludedTerminals'.

    private bool TraceIndependentPath(
        Terminal current,
        Terminal target,
        LED requiredLED,
        HashSet<Terminal> excludedTerminals,
        HashSet<Terminal> visited)
    {
        if (current == null) return false;
        if (visited.Contains(current)) return false;

        // Block paths through other LEDs' terminals
        if (excludedTerminals.Contains(current)) return false;

        visited.Add(current);

        if (current == target)
        {
            // Only valid if we passed through the required LED
            return PathContainsLED(visited, requiredLED);
        }

        Terminal connected = current.connectedTerminal;
        if (connected == null) return false;

        CircuitComponent owner = connected.owner;
        if (owner == null) return false;

        // Block paths through other LEDs
        if (owner is LED otherLED && otherLED != requiredLED)
            return false;

        if (!owner.CanPassCurrent()) return false;

        if (owner.terminals == null || owner.terminals.Length == 0)
            return false;

        foreach (Transform t in owner.terminals)
        {
            if (t == null) continue;

            Terminal next = t.GetComponent<Terminal>();
            if (next != null && next != connected)
            {
                if (TraceIndependentPath(next, target, requiredLED, excludedTerminals, visited))
                    return true;
            }
        }

        return false;
    }

    // ─── Helpers ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns all terminals belonging to every LED except the one at 'excludeIndex'.
    /// Used to block the tracer from passing through sibling LEDs.
    /// </summary>
    private HashSet<Terminal> GetOtherLEDTerminals(LED[] leds, int excludeIndex)
    {
        HashSet<Terminal> excluded = new HashSet<Terminal>();

        for (int i = 0; i < leds.Length; i++)
        {
            if (i == excludeIndex) continue;

            if (leds[i].terminals == null) continue;

            foreach (Transform t in leds[i].terminals)
            {
                if (t == null) continue;
                Terminal terminal = t.GetComponent<Terminal>();
                if (terminal != null)
                    excluded.Add(terminal);
            }
        }

        return excluded;
    }

    /// <summary>
    /// Checks if the visited terminal set contains any terminal
    /// that belongs to the required LED.
    /// </summary>
    private bool PathContainsLED(HashSet<Terminal> visited, LED requiredLED)
    {
        if (requiredLED.terminals == null) return false;

        foreach (Transform t in requiredLED.terminals)
        {
            if (t == null) continue;
            Terminal terminal = t.GetComponent<Terminal>();
            if (terminal != null && visited.Contains(terminal))
                return true;
        }

        return false;
    }
}