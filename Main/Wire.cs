using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wire : CircuitComponent
{
    public Transform terminalA;
    public Transform terminalB;

    private void Awake()
    {
        type = ComponentType.Conductor;
        resistance = 0.01f;

        if (terminals == null || terminals.Length == 0)
        {
            terminals = new Transform[] { terminalA, terminalB };
        }
    }
}
