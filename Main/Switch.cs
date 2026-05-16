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
    private bool wasDragged;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        UpdateVisuals();
    }

    public override bool CanPassPower()
    {
        return isClosed;
    }

    public void Toggle()
    {
        SetState(!isClosed);
    }

    public void SetState(bool closed)
    {
        isClosed = closed;
        UpdateVisuals();
        CircuitManager.Instance?.EvaluateCircuit();
    }

    private void UpdateVisuals()
    {
        if (sr == null)
            return;

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
