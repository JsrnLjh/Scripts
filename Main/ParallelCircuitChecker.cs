using UnityEngine;

public class ParallelCircuitChecker : MonoBehaviour
{
    public bool IsParallelCircuitValid()
    {
        LED[] leds = FindObjectsOfType<LED>();

        if (FindObjectOfType<Battery>() == null)
        {
            // Debug.Log("[ParallelChecker] No battery found.");
            return false;
        }

        if (leds.Length < 2)
        {
            // Debug.Log("[ParallelChecker] Need at least 2 LEDs.");
            return false;
        }

        int poweredLedCount = 0;
        foreach (LED led in leds)
        {
            if (led != null && led.isPowered)
                poweredLedCount++;
        }

        if (poweredLedCount < 2)
        {
            // Debug.Log($"[ParallelChecker] Only {poweredLedCount}/2 LEDs are powered.");
            return false;
        }

        // Debug.Log("[ParallelChecker] Parallel circuit accepted: at least 2 LEDs are powered.");
        return true;
    }
}
