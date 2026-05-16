using UnityEngine;

public class CircuitComponent : MonoBehaviour
{
    public bool isPowered;

    [Header("Connection Points")]
    public Terminal[] terminals;

    public virtual void SetPower(bool powered)
    {
        isPowered = powered;
    }

    public virtual bool CanPassPower()
    {
        return true;
    }

    public virtual bool CanPassCurrent()
    {
        return CanPassPower();
    }
}
