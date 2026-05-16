using UnityEngine;

public class DraggableComponent : MonoBehaviour
{
    private Vector3 offset;
    private Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }

    private void OnMouseDown()
    {
        Vector3 mouseWorld =
            cam.ScreenToWorldPoint(Input.mousePosition);

        offset = transform.position - new Vector3(mouseWorld.x, mouseWorld.y, 0f);
    }

    private void OnMouseDrag()
    {
        Vector3 mouseWorld =
            cam.ScreenToWorldPoint(Input.mousePosition);

        transform.position =
            new Vector3(
                mouseWorld.x + offset.x,
                mouseWorld.y + offset.y,
                0f
            );

        CircuitManager.Instance?.EvaluateCircuit();
    }

    private void OnMouseUp()
    {
        // Auto-connect nearby terminals
        Terminal[] terminals =
            GetComponentsInChildren<Terminal>();

        foreach (Terminal terminal in terminals)
        {
            terminal.AutoConnectToNearestTerminal();
        }

        // Debug.Log($"{name} placed.");

        // Re-check power flow
        CircuitManager.Instance?.EvaluateCircuit();
    }
}
