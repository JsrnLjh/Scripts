using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BuildManager : MonoBehaviour
{
    public static BuildManager Instance;

    [Header("Spawnable Prefabs")]
    public GameObject batteryPrefab;
    public GameObject wirePrefab;
    public GameObject ledPrefab;
    public GameObject resistorPrefab;
    public GameObject switchPrefab;

    [Header("References")]
    public Camera mainCamera;
    public Transform spawnParent;
    public RectTransform simulatorPanel;

    private GameObject selectedPrefab;
    private SelectableComponent selectedPlacedComponent;

    private List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        Instance = this;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Update()
    {
        if (selectedPrefab == null) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (!IsPointerInsideSimulatorPanel())
                return;

            SpawnSelectedPrefab();
        }
    }

    public bool HasSelectedPrefab()
    {
        return selectedPrefab != null;
    }

    public void SelectBattery()
    {
        selectedPrefab = batteryPrefab;
        DeselectPlacedComponent();
    }

    public void SelectWire()
    {
        selectedPrefab = wirePrefab;
        DeselectPlacedComponent();
    }

    public void SelectLED()
    {
        selectedPrefab = ledPrefab;
        DeselectPlacedComponent();
    }

    public void SelectResistor()
    {
        selectedPrefab = resistorPrefab;
        DeselectPlacedComponent();
    }

    public void SelectSwitch()
    {
        selectedPrefab = switchPrefab;
        DeselectPlacedComponent();
    }

    public void SelectPlacedComponent(SelectableComponent component)
    {
        if (selectedPlacedComponent != null)
        {
            selectedPlacedComponent.SetSelected(false);
        }

        selectedPlacedComponent = component;

        if (selectedPlacedComponent != null)
        {
            selectedPlacedComponent.SetSelected(true);        
        }
    }

    public void DeselectPlacedComponent()
    {
        if (selectedPlacedComponent != null)
        {
            selectedPlacedComponent.SetSelected(false);
            selectedPlacedComponent = null;
        }
    }

    public void RotateSelected()
    {
        selectedPlacedComponent.transform.Rotate(0f, 0f, 90f);

        if (CircuitManager.Instance != null)
        {
            CircuitManager.Instance.EvaluateCircuit();
        }
    }

    public void ResetAllPlacedComponents()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }

        spawnedObjects.Clear();
        selectedPrefab = null;
        DeselectPlacedComponent();

        if (CircuitManager.Instance != null)
        {
            CircuitManager.Instance.EvaluateCircuit();
        }
    }

    private bool IsPointerInsideSimulatorPanel()
    {
        if (simulatorPanel == null) return true;

        return RectTransformUtility.RectangleContainsScreenPoint(
            simulatorPanel,
            Input.mousePosition,
            null
        );
    }

    private void SpawnSelectedPrefab()
    {
        Vector3 mouseScreenPos = Input.mousePosition;
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(mouseScreenPos);
        worldPos.z = 0f;

        if (CircuitGrid.Instance != null)
        {
            worldPos = CircuitGrid.Instance.SnapToGrid(worldPos);
        }

        Vector3 spawnPos = GetSnappedPositionWithOffset(selectedPrefab, worldPos);
        GameObject spawned = Instantiate(selectedPrefab, spawnPos, Quaternion.identity);
        spawnedObjects.Add(spawned);


        selectedPrefab = null;

        if (CircuitManager.Instance != null)
        {
            CircuitManager.Instance.EvaluateCircuit();
        }
    }

    Vector3 GetSnappedPositionWithOffset(GameObject prefab, Vector3 mouseWorldPos)
    {
        float gridSize = 1f;

        // Snap mouse to grid
        float snappedX = Mathf.Round(mouseWorldPos.x / gridSize) * gridSize;
        float snappedY = Mathf.Round(mouseWorldPos.y / gridSize) * gridSize;

        Vector3 snappedPos = new Vector3(snappedX, snappedY, 0);

        // Find first terminal of prefab
        Terminal terminal = prefab.GetComponentInChildren<Terminal>();

        if (terminal != null)
        {
            Vector3 terminalOffset = terminal.transform.localPosition;
            return snappedPos - terminalOffset;
        }

        return snappedPos;
    }
}
