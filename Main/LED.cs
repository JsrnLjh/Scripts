using UnityEngine;

public class LED : CircuitComponent
{
    [Header("Terminals")]
    public GameObject anode;
    public GameObject cathode;

    [Header("LED Settings")]
    public float forwardVoltage = 2f;
    public float maxVoltage = 12f;

    public Sprite litSprite;
    public Sprite unlitSprite;
    public Sprite brokenSprite;

    [HideInInspector] public bool isLit;
    [HideInInspector] public bool isBroken;

    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponentInChildren<SpriteRenderer>();
        type = ComponentType.Consumer;
        resistance = 50f;

        if ((terminals == null || terminals.Length == 0)
            && anode != null && cathode != null)
            terminals = new Transform[] { anode.transform, cathode.transform };
    }

    public override void Evaluate(float voltageDrop, float current)
    {
        if (isBroken) return;

        Debug.Log($"[LED] {name} — voltageDrop={voltageDrop:F2}V current={current:F4}A");

        // Use current-based detection — more reliable than voltage drop
        // since voltage drop depends on the resistance ratio in the circuit
        float minCurrent = forwardVoltage / Mathf.Max(resistance, 1f);

        // Burnout check — use battery voltage not voltage drop
        Battery battery = FindObjectOfType<Battery>();
        float batteryVoltage = battery != null ? battery.voltageOutput : 9f;

        if (batteryVoltage > maxVoltage)
        {
            isBroken = true;
            isLit = false;
            if (sr != null && brokenSprite != null)
                sr.sprite = brokenSprite;
            Debug.LogWarning($"[LED] {name} burned out!");
            return;
        }

        // Light up if current is sufficient
        isLit = current >= minCurrent;

        if (sr != null)
            sr.sprite = isLit ? litSprite : unlitSprite;

        Debug.Log($"[LED] {name} — minCurrent={minCurrent:F4}A " +
                  $"actual={current:F4}A isLit={isLit}");
    }

    public override void ResetState()
    {
        base.ResetState();
        isLit = false;

        if (!isBroken && sr != null && unlitSprite != null)
            sr.sprite = unlitSprite;
    }

    public void FullReset()
    {
        isBroken = false;
        isLit = false;

        if (sr != null && unlitSprite != null)
            sr.sprite = unlitSprite;
    }
}