using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour
{
    void Start()
    {
        // transform.rotation = Quaternion.Euler(0f, 0f, 0f);
    }
    void Update()
    {
        Vector3 rL = transform.localRotation.eulerAngles;
        Vector3 rW = transform.parent.eulerAngles;
        transform.localRotation = Quaternion.Euler(rL.x, - rW.y, rL.z);
    }
}
