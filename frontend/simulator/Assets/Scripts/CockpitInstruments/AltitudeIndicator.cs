using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AltitudeIndicator : MonoBehaviour
{
    public Transform hundredsHand = null;
    public Transform thousandsHand = null;

    GameObject pilotSailplane = null;
    private float offsetForZeroRotation = 90f;

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
        hundredsHand.localRotation = Quaternion.Euler(0f, ((((int)pilotSailplane.transform.position.y % 1000) * (360f / 1000f)) + offsetForZeroRotation), 0f);
        thousandsHand.localRotation = Quaternion.Euler(0f, ((((int)pilotSailplane.transform.position.y % 10000) * (360f / 10000f)) + offsetForZeroRotation), 0f);
    }
}
