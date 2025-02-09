using System.Collections;
using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;

public class PointerHeading : MonoBehaviour
{
    public Transform head;

    // relativeTo can either be: "", "wPH"
    public bool withPilotHead = true;
    void Update()
    {
        if (withPilotHead)
        {
            Quaternion orientation = head.transform.localRotation;
            float y_heading = orientation.eulerAngles[1];
            transform.localRotation = Quaternion.Euler(0f, 0f, -y_heading);
        }
    }
}
