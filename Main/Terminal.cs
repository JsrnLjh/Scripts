using UnityEngine;

public class Terminal : MonoBehaviour
{
    public Terminal connectedTerminal;
    public CircuitComponent owner;
    [SerializeField]
    private float autoConnectDistance = 0.2f;

    private void Awake()
    {
        owner = GetComponentInParent<CircuitComponent>();
    }

    public void ConnectTo(Terminal other)
    {
        connectedTerminal = other;
        other.connectedTerminal = this;

        // Debug.Log($"{name} connected to {other.name}");

        CircuitManager.Instance?.EvaluateCircuit();
    }

    public bool IsConnected()
    {
        return connectedTerminal != null;
    }

    public void AutoConnectToNearestTerminal()
    {
        Terminal nearest = null;
        float nearestDistance = autoConnectDistance;
        Terminal[] terminals = FindObjectsOfType<Terminal>();

        foreach (Terminal terminal in terminals)
        {
            if (terminal == null || terminal == this || terminal.owner == owner)
                continue;

            float distance = Vector2.Distance(transform.position, terminal.transform.position);
            if (distance <= nearestDistance)
            {
                nearest = terminal;
                nearestDistance = distance;
            }
        }

        if (nearest != null)
            ConnectTo(nearest);
    }
}
