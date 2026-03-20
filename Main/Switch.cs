using UnityEngine;

public class Switch : CircuitComponent
{
    [Header("Switch Visuals")]
    public Sprite closedSprite;
    public Sprite openSprite;
    private SpriteRenderer sr;

    [Header("State")]
    [SerializeField] private bool isClosed = false;

    [Header("Drag Guard")]
    [SerializeField] private float dragThreshold = 8f;
    private Vector2 mouseDownScreenPos;
    private bool wasDragged = false;

    private void Awake()
    {
        type = ComponentType.Control;
        sr = GetComponentInChildren<SpriteRenderer>();
        UpdateVisuals();
        Debug.Log($"[Switch] {name} Awake: isClosed = {isClosed}");
    }

    public override bool CanPassCurrent()
    {
        Debug.Log($"[Switch] {name} CanPassCurrent = {isClosed}");
        return isClosed;
    }

    public void Toggle()
    {
        isClosed = !isClosed;
        UpdateVisuals();
        Debug.Log($"[Switch] {name} toggled → isClosed = {isClosed}");

        if (CircuitManager.Instance != null)
            CircuitManager.Instance.EvaluateCircuit();
    }

    // Called externally (e.g. from a UI button for mobile)
    public void SetState(bool closed)
    {
        isClosed = closed;
        UpdateVisuals();

        if (CircuitManager.Instance != null)
            CircuitManager.Instance.EvaluateCircuit();
    }

    private void UpdateVisuals()
    {
        if (sr == null) return;
        sr.sprite = isClosed ? closedSprite : openSprite;
    }

    private void OnMouseDown()
    {
        mouseDownScreenPos = Input.mousePosition;
        wasDragged = false;
    }

    private void OnMouseDrag()
    {
        if (Vector2.Distance(Input.mousePosition, mouseDownScreenPos) > dragThreshold)
            wasDragged = true;
    }

    private void OnMouseUp()
    {
        if (!wasDragged)
            Toggle();

        wasDragged = false;
    }

    public bool IsClosed => isClosed;

#if UNITY_EDITOR
    private void OnValidate()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        UpdateVisuals();
    }
#endif
}