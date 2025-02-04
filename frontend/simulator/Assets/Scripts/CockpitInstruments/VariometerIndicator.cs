using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Variometer : MonoBehaviour
{
    public float maximumAngleRange = 130f;
    GameObject pilotSailplane = null;

    // Start is called before the first frame update
    void Start()
    {
        GameObject[] pilotSailplanes = GameObject.FindGameObjectsWithTag("PilotSailplane");
        if (pilotSailplanes.Length > 0)
        {
            pilotSailplane = pilotSailplanes[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        float verticalVelocity = pilotSailplane.GetComponent<Rigidbody>().velocity.y;
        if (Mathf.Abs(verticalVelocity) > 5f)
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sign(verticalVelocity) * maximumAngleRange);
        }
        else
        {
            transform.localRotation = Quaternion.Euler(0f, 0f, (verticalVelocity * (maximumAngleRange / 5f)));
        }
    }
}
