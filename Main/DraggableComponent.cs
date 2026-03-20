using UnityEngine;

public class DraggableComponent : MonoBehaviour
{
    private Vector3 offset;
    private float zCoord;

    [Header("Grid Settings")]
    public float gridSize = 1.0f;

    private Camera mainCamera;
    private CircuitComponent circuitComponent;

    private void Awake()
    {
        mainCamera = Camera.main;
        circuitComponent = GetComponent<CircuitComponent>();
    }

    void OnMouseDown()
    {
        zCoord = mainCamera.WorldToScreenPoint(transform.position).z;
        offset = transform.position - GetMouseWorldPos();
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = zCoord;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    void OnMouseDrag()
    {
        Vector3 newPos = GetMouseWorldPos() + offset;
        transform.position = new Vector3(newPos.x, newPos.y, 0);
    }

    void OnMouseUp()
    {
        SnapUsingTerminalOffset();

        Debug.Log($"{gameObject.name} placed at {transform.position}");

        if (CircuitManager.Instance != null)
        {
            CircuitManager.Instance.EvaluateCircuit();
        }
    }

    private void SnapUsingTerminalOffset()
    {
        Vector3 snappedPos = transform.position;

        if (circuitComponent != null && circuitComponent.terminals != null && circuitComponent.terminals.Length > 0 && circuitComponent.terminals[0] != null)
        {
            Vector3 terminalWorldPos = circuitComponent.terminals[0].position;
            Vector3 terminalLocalOffset = circuitComponent.terminals[0].localPosition;

            float snappedX = Mathf.Round(terminalWorldPos.x / gridSize) * gridSize;
            float snappedY = Mathf.Round(terminalWorldPos.y / gridSize) * gridSize;

            snappedPos = new Vector3(snappedX, snappedY, 0) - terminalLocalOffset;
        }
        else
        {
            float snappedX = Mathf.Round(transform.position.x / gridSize) * gridSize;
            float snappedY = Mathf.Round(transform.position.y / gridSize) * gridSize;

            snappedPos = new Vector3(snappedX, snappedY, 0);
        }

        transform.position = snappedPos;
    }
}