using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSpeedIndicator : MonoBehaviour
{
    public float compensation = 30f;

    GameObject pilotSailplane = null;
    // Start is called before the first frame update
    void Start()
    {
        GameObject[] pilotSailplanes = GameObject.FindGameObjectsWithTag("PilotSailplane");
        if (pilotSailplanes.Length > 0 )
        {
            pilotSailplane = pilotSailplanes[0];
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.localRotation = Quaternion.Euler(0f, 0f, -(((pilotSailplane.GetComponent<Rigidbody>().velocity.magnitude * 3.6f) * (360f / 260f)) - 90f));
    }
}
