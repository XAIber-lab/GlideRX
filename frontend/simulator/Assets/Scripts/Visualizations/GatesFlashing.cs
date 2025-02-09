using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GatesFlashing : MonoBehaviour, IsVisualizationActivated
{
    public GameObject sailplane;

    private AirTrafficController airTrafficController;

    private RiskSeverity previousRiskSeverity;
    private RiskSeverity currentRiskSeverity;
    private RiskSeverity updatedRiskSeverity;

    private IEnumerator flashing;
    private bool isActivated;

    List<GameObject> gates = new List<GameObject>();
    // Start is called before the first frame update
    void OnEnable()
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        bool self = true;
        foreach (Transform child in children)
        {
            if (self)
            {
                // skip the first one (this gameObject)
                self = false;
                continue;
            }
            gates.Add(child.gameObject);
            child.gameObject.SetActive(false);
        }
        if (gates.Count == 10)
        {
            try
            {
                AirTrafficController airTrafficController = GameObject.FindWithTag("AirTrafficController").GetComponent<AirTrafficController>();
                airTrafficController.trafficEvent.AddListener(onTrafficEvent);
                airTrafficController.severeThreatEvent.AddListener(onSevereThreat);
            }
            catch (Exception e)
            {
                Debug.LogError("No Air Traffic Controller defined." + e.Message);
            }
            flashing = Flash();
            isActivated = false;
            previousRiskSeverity = RiskSeverity.NONE;
            currentRiskSeverity = RiskSeverity.NONE;
            updatedRiskSeverity = RiskSeverity.NONE;
        }
        else
            Debug.LogError("Provide 10 gates as children of this GameObject");
    }

    private void OnDisable()
    {
        gates.Clear();
    }

    private void onTrafficEvent(int riskSeverity)
    {
        updatedRiskSeverity = (RiskSeverity)riskSeverity;
        if (updatedRiskSeverity != currentRiskSeverity)
        {
            previousRiskSeverity = currentRiskSeverity;
            currentRiskSeverity = updatedRiskSeverity;
            // morph in
            if (currentRiskSeverity == RiskSeverity.SEVERE)
            {
                StartCoroutine(flashing);
            }
            // morph out
            if (previousRiskSeverity == RiskSeverity.SEVERE)
            {
                isActivated = false;
                StopCoroutine(flashing);
                foreach (Transform gate in gameObject.transform)
                {
                    gate.gameObject.SetActive(false);
                }
            }
        }
    }

    private void onSevereThreat(GameObject severeThreat)
    {
        if (sailplane == severeThreat)
        {
            isActivated = true;
        }
        else
        {
            isActivated = false;
        }
    }

    IEnumerator Flash()
    {
        for (; ; )
        {
            GameObject[] previousGates = { gates[7], gates[8], gates[9] };

            Vector3 direction = sailplane.transform.rotation * Vector3.forward;
            transform.position = sailplane.transform.position + 25 * direction;

            foreach (GameObject gate in gates)
            {
                if (isActivated)
                {
                    previousGates[0].SetActive(false);
                    previousGates[0] = previousGates[1];
                    previousGates[1] = previousGates[2];
                    previousGates[2] = gate;
                    gate.SetActive(true);
                    // previousGate = gate;
                }
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public bool GetIsActivated()
    {
        return isActivated;
    }
}
