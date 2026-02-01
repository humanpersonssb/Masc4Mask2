using UnityEngine;

public class BobSideToSide : MonoBehaviour
{
    [SerializeField] float speed = 2f;
    [SerializeField] float distance = 1f;
    
    Vector3 startPos;

    void Start()
    {
        // Store the initial position
        startPos = transform.position;
    }

    void Update()
    {
        // Calculate offset using Sine wave
        float offset = Mathf.Sin(Time.time * speed) * distance;
        
        // Apply offset to X axis (horizontal)
        transform.position = startPos + new Vector3(offset, 0, 0);
    }
}