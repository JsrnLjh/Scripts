using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircuitGrid : MonoBehaviour
{
    public static CircuitGrid Instance;

    [Header("Grid Dimensions")]
    public int width = 10;
    public int height = 10;
    public float cellSize = 1.0f;

    void Awake()
    {
        Instance = this;
    }

    public Vector3 SnapToGrid(Vector3 worldPosition)
    {
        float x = Mathf.Round(worldPosition.x / cellSize) * cellSize;
        float y = Mathf.Round(worldPosition.y / cellSize) * cellSize;
        return new Vector3(x, y, 0);
    }
}
