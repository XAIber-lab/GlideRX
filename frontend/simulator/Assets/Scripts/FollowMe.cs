using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowMe : MonoBehaviour
{
    public Transform target;
    // public float smoothSpeed = 0.125f;
    public Vector3 locationOffset;
    public Vector3 rotationOffset;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    void Update()
    {
        // Update is called once per frame
        Vector3 desiredPosition = target.position + target.rotation * locationOffset;
        // Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = desiredPosition;

        Quaternion desiredRotation = target.rotation * Quaternion.Euler(rotationOffset);
        // Quaternion smoothedRotation = Quaternion.Lerp(transform.rotation, desiredRotation, smoothSpeed);
        transform.rotation = desiredRotation;
    }
}
