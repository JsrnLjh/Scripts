using UnityEngine;

public class LED : CircuitComponent
{
    public GameObject lightEffect;
    public Sprite litSprite;
    public Sprite unlitSprite;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null && unlitSprite == null)
            unlitSprite = spriteRenderer.sprite;

        UpdateVisuals();
    }

    public override void SetPower(bool powered)
    {
        base.SetPower(powered);
        UpdateVisuals();
    }

    private void Update()
    {
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (lightEffect != null)
            lightEffect.SetActive(isPowered);

        if (spriteRenderer == null)
            return;

        if (isPowered && litSprite != null)
        {
            spriteRenderer.sprite = litSprite;
        }
        else if (!isPowered && unlitSprite != null)
        {
            spriteRenderer.sprite = unlitSprite;
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null && unlitSprite == null)
            unlitSprite = spriteRenderer.sprite;

        UpdateVisuals();
    }
#endif
}
