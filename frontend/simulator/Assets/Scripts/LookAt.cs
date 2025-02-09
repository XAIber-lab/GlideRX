using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAt : MonoBehaviour
{
    public GameObject lookAtObject;
    public float rotationOffsetX;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(lookAtObject.transform);
        transform.Rotate(rotationOffsetX, 0f, 0f, Space.Self);
    }
}
