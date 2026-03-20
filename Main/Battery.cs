using UnityEngine;

public class Battery : CircuitComponent
{
    [Header("Battery Terminals")]
    public GameObject positiveTerminal;
    public GameObject negativeTerminal;

    [Header("Settings")]
    public float voltageOutput = 9f;

    private void Awake()
    {
        type = ComponentType.PowerSource;
        resistance = 0f;

        if ((terminals == null || terminals.Length == 0) && positiveTerminal != null && negativeTerminal != null)
        {
            terminals = new Transform[] { positiveTerminal.transform, negativeTerminal.transform };
        }
    }
}