using UnityEngine;

public class Resistor : CircuitComponent
{
    [Header("Resistor Settings")]
    public float resistanceValue = 220f;

    public override bool CanPassPower()
    {
        return true;
    }
}
