using UnityEngine;

public class Resistor : CircuitComponent
{
    [Header("Resistor Settings")]
    public float resistanceValue = 220f;

    public GameObject terminalA;
    public GameObject terminalB;

    private void Awake()
    {
        type = ComponentType.Consumer;
        resistance = resistanceValue; // ← copy into base field so GetResistance() works

        if ((terminals == null || terminals.Length == 0)
            && terminalA != null && terminalB != null)
        {
            terminals = new Transform[] { terminalA.transform, terminalB.transform };
        }
    }
}