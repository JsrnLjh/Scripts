using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceEffect : MonoBehaviour
{
    public float bounceHeight = 0.2f; // Height of the bounce
    public float bounceDuration = 0.4f; // Speed of the bounce
    public int bounceCount = 2; // Number of bounces

    private Vector3 originalPosition;

    public void StartBounce()
    {
        StartCoroutine(BounceHandler());
    }

    private IEnumerator BounceHandler()
    {
        Vector3 startPosition = transform.position;
        float localHeight = bounceHeight;
        float localDuration = bounceDuration;

        for (int i = 0; i < bounceCount; i++)
        {
            yield return Bounce(transform, startPosition, localHeight, localDuration / 2);
            localHeight *= 0.5f; // Reduce height each bounce
            localDuration *= 0.8f; // Reduce duration each bounce
        }

        transform.localPosition = startPosition; // Reset position
    }
    
    private IEnumerator Bounce(Transform transform, Vector3 start, float height, float duration)
    {
        Vector3 peak = start + Vector3.up * height;
        float elapsed = 0f;

        //move upwards
        while (elapsed < duration)
        {
            transform.localPosition = Vector3.Lerp(start, peak, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        elapsed = 0f;
        
        //move downwards
        while (elapsed < duration)
        {
            transform.localPosition = Vector3.Lerp(peak, start, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        
    }
}
