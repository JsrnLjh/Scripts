using UnityEngine;

public class Terminal : MonoBehaviour
{
    public CircuitComponent owner;
    public Terminal connectedTerminal;

    // Track overlap count to guard against physics jitter
    private int overlapCount = 0;

    private void Awake()
    {
        if (owner == null)
            owner = GetComponentInParent<CircuitComponent>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Terminal")) return;

        Terminal otherTerminal = other.GetComponent<Terminal>();
        if (otherTerminal == null || otherTerminal == this) return;

        // Don't connect two terminals on the same component
        if (otherTerminal.owner == this.owner) return;

        overlapCount++;

        // Only establish connection on first overlap
        if (connectedTerminal == null)
        {
            connectedTerminal = otherTerminal;

            if (otherTerminal.connectedTerminal == null)
                otherTerminal.connectedTerminal = this;

            Debug.Log($"[Terminal] {name} connected to {otherTerminal.name}");

            if (CircuitManager.Instance != null)
                CircuitManager.Instance.EvaluateCircuit();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Terminal")) return;

        Terminal otherTerminal = other.GetComponent<Terminal>();
        if (otherTerminal == null) return;

        overlapCount = Mathf.Max(0, overlapCount - 1);

        // Only disconnect when fully separated (no remaining overlaps)
        if (overlapCount == 0 && connectedTerminal == otherTerminal)
        {
            connectedTerminal = null;

            if (otherTerminal.connectedTerminal == this)
                otherTerminal.connectedTerminal = null;

            Debug.Log($"[Terminal] {name} disconnected from {otherTerminal.name}");

            if (CircuitManager.Instance != null)
                CircuitManager.Instance.EvaluateCircuit();
        }
    }

    // Useful for debugging in Editor
    public bool IsConnected => connectedTerminal != null;
}