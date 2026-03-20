using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ComponentType { PowerSource, Consumer, Conductor, Control}

public class CircuitComponent : MonoBehaviour
{
    public ComponentType type;
    public float resistance;
    public float voltageDrop;
    public bool isPassed;

    [Header("Connection Points")]
    public Transform[] terminals;

    public virtual bool CanPassCurrent()
    {
        return true;
    }

    public virtual float GetResistance()
    {
        return resistance;
    }

    public virtual void Evaluate(float voltage, float current)
    {
        // Override in child classes
    }

    public virtual void ResetState()
    {
        isPassed = false;
    }
}
