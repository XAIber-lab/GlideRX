using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System;
using System.Runtime.ExceptionServices;

public class RangeLog : MonoBehaviour
{
    private Transform pilotSailplane;
    private AirTrafficController airTrafficController;
    private TextMeshProUGUI rangeText;
    private float closestRange = 100000f;
    private void Awake()
    {
        try
        {
            pilotSailplane = GameObject.FindWithTag("PilotSailplane").transform;
        }
        catch (Exception e)
        {
            Debug.LogError("No Pilot Sailplane defined." + e.Message);
        }
        try
        {
            airTrafficController = GameObject.FindWithTag("AirTrafficController").GetComponent<AirTrafficController>();
            airTrafficController.closestRangeEvent.AddListener(onClosestRangeEvent);
        }
        catch (Exception e)
        {
            Debug.LogError("No Air Traffic Controller defined." + e.Message);
        }
        rangeText = this.GetComponent<TextMeshProUGUI>();
    }

    private void onClosestRangeEvent(float closestRange)
    {
        this.closestRange = closestRange;
    }

    private void Update()
    {
        rangeText.text = "Range: " + closestRange + "m" + "\n" + TrafficString();
    }

    private string TrafficString()
    {
        string res = "";
        foreach (KeyValuePair<GameObject, (int, Vector3, Vector3)> intruder in airTrafficController.GetIntrudersFirstPossibilityAcquisition())
        {
            res += intruder.Key.name + " distance: " + (intruder.Key.transform.position - pilotSailplane.position).magnitude + "\n";
        }
        return res;
    }
}
