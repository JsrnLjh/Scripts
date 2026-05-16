using UnityEngine;
using System.Collections.Generic;

public class CircuitManager : MonoBehaviour
{
    public static CircuitManager Instance;

    [SerializeField]
    private float contactTolerance = 0.05f;

    private void Awake()
    {
        Instance = this;
    }

    public void EvaluateCircuit()
    {
        Physics2D.SyncTransforms();

        CircuitComponent[] components =
            FindObjectsOfType<CircuitComponent>();

        // Reset all
        foreach (CircuitComponent comp in components)
        {
            comp.SetPower(false);
        }

        // Find batteries
        Battery[] batteries = FindObjectsOfType<Battery>();

        foreach (Battery battery in batteries)
        {
            battery.SetPower(true);
            PropagatePower(battery, components);
        }

        CircuitQuestValidator.Instance?.Validate();
    }

    void PropagatePower(CircuitComponent source, CircuitComponent[] components)
    {
        Queue<CircuitComponent> queue = new Queue<CircuitComponent>();
        HashSet<CircuitComponent> visited = new HashSet<CircuitComponent>();

        queue.Enqueue(source);
        visited.Add(source);

        while (queue.Count > 0)
        {
            CircuitComponent current = queue.Dequeue();

            if (!current.CanPassPower())
                continue;

            foreach (CircuitComponent next in components)
            {
                if (next == null || next == current || visited.Contains(next))
                    continue;

                if (!AreTouching(current, next))
                    continue;

                next.SetPower(true);
                visited.Add(next);
                queue.Enqueue(next);

                // Debug.Log($"{current.name} powered {next.name}");
            }
        }
    }

    private bool AreTouching(CircuitComponent first, CircuitComponent second)
    {
        Collider2D[] firstColliders = first.GetComponentsInChildren<Collider2D>();
        Collider2D[] secondColliders = second.GetComponentsInChildren<Collider2D>();

        foreach (Collider2D firstCollider in firstColliders)
        {
            if (firstCollider == null || !firstCollider.enabled)
                continue;

            foreach (Collider2D secondCollider in secondColliders)
            {
                if (secondCollider == null || !secondCollider.enabled)
                    continue;

                ColliderDistance2D distance = firstCollider.Distance(secondCollider);
                if (distance.isOverlapped || distance.distance <= contactTolerance)
                    return true;
            }
        }

        return false;
    }
}
