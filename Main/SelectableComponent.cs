using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectableComponent : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color originalColor;
    private bool isSelected = false;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            sr = GetComponentInChildren<SpriteRenderer>();
        }

        if (sr != null)
        {
            originalColor = sr.color;
        }
    }

    private void OnMouseDown()
    {
        if (BuildManager.Instance == null) return;

        // If currently in build mode, let BuildManager handle spawning instead
        if (BuildManager.Instance.HasSelectedPrefab())
            return;

        BuildManager.Instance.SelectPlacedComponent(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;

        if (sr != null)
        {
            sr.color = isSelected ? Color.yellow : originalColor;
        }
    }
}
