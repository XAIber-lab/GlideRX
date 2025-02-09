using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtSailplane : MonoBehaviour
{
    public GameObject sailplane = null;
    public Vector3 localRotationOffset = Vector3.zero;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (sailplane != null)
        {
            transform.LookAt(sailplane.transform);
            transform.Rotate(localRotationOffset, Space.Self);
        }
    }
}
