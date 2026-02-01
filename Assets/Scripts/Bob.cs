using System;
using System.Collections;
using UnityEngine;

public class BobSideToSide : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    [SerializeField] float distance = 1f;
    
    Vector3 startPos;
    private bool hasStarted;

    private void Awake()
    {
        hasStarted = false;
    }

    IEnumerator Start()
    {
        // Store the initial position
        yield return new WaitForSeconds(0.1f);
        startPos = transform.position;
        hasStarted = true;
    }

    void Update()
    {
        if (!hasStarted) return;
        // Calculate offset using Sine wave
        float offset = Mathf.Sin(Time.time * speed) * distance;
        
        // Apply offset to X axis (horizontal)
        transform.position = startPos + new Vector3(offset, 0, 0);
    }
}