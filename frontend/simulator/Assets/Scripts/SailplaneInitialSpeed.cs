using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SailplaneInitialSpeed : MonoBehaviour
{
    public Vector3 initialVelocity = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {
        gameObject.GetComponent<Rigidbody>().velocity = transform.TransformDirection(initialVelocity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
