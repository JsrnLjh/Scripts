using UnityEngine;

public class LED : CircuitComponent
{
    [Header("Terminals")]
    public GameObject anode;
    public GameObject cathode;

    [Header("LED Settings")]
    public float forwardVoltage = 2.0f;   // minimum voltage drop to light up
    public float maxVoltage = 5f;         // above this, LED burns out

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
        resistance = 50f; // LEDs have internal resistance; adjust as needed

        if ((terminals == null || terminals.Length == 0) && anode != null && cathode != null)
            terminals = new Transform[] { anode.transform, cathode.transform };
    }

    // voltage here is the DROP across this LED (current * resistance), not battery voltage
    public override void Evaluate(float voltageDrop, float current)
    {
        if (isBroken) return;

        Debug.Log($"[LED] {name} — voltageDrop={voltageDrop:F2}V, current={current:F4}A");

        if (voltageDrop > maxVoltage)
        {
            isBroken = true;
            isLit = false;
            if (sr != null && brokenSprite != null) sr.sprite = brokenSprite;
            Debug.LogWarning($"[LED] {name} burned out! ({voltageDrop:F2}V > {maxVoltage}V)");
            return;
        }

        isLit = voltageDrop >= forwardVoltage;

        if (sr != null)
            sr.sprite = isLit ? litSprite : unlitSprite;

        Debug.Log($"[LED] {name} isLit = {isLit}");
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