using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

[System.Serializable]
public class ZoomingInEvent : UnityEvent<bool> { }

public class RedCircle : MonoBehaviour, IsVisualizationActivated
{
    private float dynamicScale = -1f;
    public float morphingSpeed = 0.05f;
    public float colorFadingSpeed = 0.05f;
    public string shaderColorPropertyName = "_TintColor";
    public Color moderateThreatColor = Color.yellow;
    public Color severeThreatColor = Color.red;
    public bool severeThreatThreasholdFromAirTrafficController = true;

    public ZoomingInEvent zoomingInEvent;

    private RiskSeverity previousRiskSeverity = RiskSeverity.NONE;
    private RiskSeverity currentRiskSeverity = RiskSeverity.NONE;
    private RiskSeverity updatedRiskSeverity = RiskSeverity.NONE;
    
    private bool isActivated;
    private bool isSevere;

    private float minimumScale = 1500f;
    private float maximumScale = 10000f;

    private GameObject selfSailplane;
    private GameObject pilotSailplane;
    private Renderer renderer;
    private AirTrafficController airTrafficController;
    private float distanceRange;
    private float severeThreashold;
    // Start is called before the first frame update
    void Start()
    {
        isActivated = false;
        isSevere = false;

        airTrafficController = GameObject.FindWithTag("AirTrafficController").GetComponent<AirTrafficController>();
        pilotSailplane = GameObject.FindWithTag("PilotSailplane");
        airTrafficController.moderateThreatEvent.AddListener(onModerateThreat);
        airTrafficController.highThreatEvent.AddListener(onHighThreat);
        airTrafficController.severeThreatEvent.AddListener(onSevereThreat);
        // airTrafficController.trafficEvent.AddListener(onTrafficEvent);

        minimumScale = gameObject.GetComponent<LookAtCamera>().minimumScale;
        maximumScale = gameObject.GetComponent<LookAtCamera>().maximumScale;

        selfSailplane = transform.parent.GetChild(transform.GetSiblingIndex() - 1).gameObject;

        renderer = gameObject.GetComponent<Renderer>();
        if (severeThreatThreasholdFromAirTrafficController)
        {
            severeThreashold = airTrafficController.severeThreashold;
        }
        else
        {
            severeThreashold = 500f;
        }
        distanceRange = (airTrafficController.moderateThreashold - severeThreashold);
    }

    public void Awake()
    {
        if (zoomingInEvent == null)
        {
            zoomingInEvent = new ZoomingInEvent();
        }
    }

    /*
    private void onTrafficEvent(int riskSeverity)
    {
        Debug.Log("onTrafficEvent CALL: " + (RiskSeverity)riskSeverity);
        if(riskSeverity >= (int)RiskSeverity.LOW && riskSeverity != (int)RiskSeverity.SEVERE && !activated)
        {
            Debug.Log("First time in circle");
            activated = true;
            StartCoroutine(Zoom(0f, standardScale, morphingSpeed));
        }
        if(riskSeverity == (int)RiskSeverity.NONE && activated)
        {
            Debug.Log("FUUUCK");
            activated = false;
            StartCoroutine(Zoom(standardScale, 0f, morphingSpeed));
        }
        updatedRiskSeverity = (RiskSeverity)riskSeverity;
        if (updatedRiskSeverity != currentRiskSeverity)
        {
            previousRiskSeverity = currentRiskSeverity;
            currentRiskSeverity = updatedRiskSeverity;
            // morph in
            if (currentRiskSeverity == RiskSeverity.SEVERE)
            {
                Debug.Log("Starting coroutine");
                StartCoroutine(Zoom(maximumScale, standardScale, morphingSpeed));
            }
        }
    }
    */

    private void Update()
    {
        float distance = (selfSailplane.transform.position - pilotSailplane.transform.position).magnitude;
        if (distance <= severeThreashold)
        {
            renderer.material.SetColor(shaderColorPropertyName, severeThreatColor);
        }
        else
        {
            float t = (distance - severeThreashold) / distanceRange;
            if (t > 1)
                t = 1;
            renderer.material.SetColor(shaderColorPropertyName, Color.Lerp(severeThreatColor, moderateThreatColor, t));
        }
    }

    private void onModerateThreat(List<GameObject> moderateThreats)
    {
        isSevere = false;
        // Is this sailplane in the list of moderate threats?
        if (moderateThreats.Contains(selfSailplane))
        {
            if (!isActivated)
            {
                isActivated = true;
                StartCoroutine(Zoom(0f, dynamicScale, morphingSpeed));
            }
        }
        else
        {
            if (isActivated)
            {
                isActivated = false;
                StartCoroutine(Zoom(transform.localScale.x, 0f, morphingSpeed));
            }
        }
    }

    private void onHighThreat(List<GameObject> highThreats)
    {
        isSevere = false;
        // Is this sailplane in the list of moderate threats?
        if (highThreats.Contains(selfSailplane))
        {
            if (!isActivated)
            {
                isActivated = true;
                StartCoroutine(Zoom(0f, dynamicScale, morphingSpeed));
            }
        }
        else
        {
            if (isActivated)
            {
                isActivated = false;
                StartCoroutine(Zoom(transform.localScale.x, 0f, morphingSpeed));
            }
        }
    }

    private void onSevereThreat(GameObject sailplane)
    {
        if (sailplane == selfSailplane)
        {
            isActivated = true;
            if (!isSevere)
            {
                isSevere = true;
                StartCoroutine(Zoom(maximumScale, dynamicScale, morphingSpeed));
            }
        }
        else
        {
            isSevere = false;
            isActivated = false;
            StartCoroutine(Zoom(transform.localScale.x, 0f, morphingSpeed));
        }
    }

    IEnumerator Zoom(float initialScale, float finalScale, float morphingSpeed)
    {
        float scale = finalScale;
        if (finalScale == dynamicScale)
        {
            // Because of DynamicCircleScale
            // Debug.Log("Adjusting radius: " + initialScale.ToString() + " " +  finalScale.ToString());
            float distance = (Camera.main.transform.position - transform.position).magnitude;
            scale = (distance * ((maximumScale - minimumScale) / 2000f)) + minimumScale;
        }
        for (float alpha = 0f; alpha <= 1; alpha += morphingSpeed)
        {
            float tmpScale = (alpha) * scale + (1 - alpha) * initialScale;
            transform.localScale = new Vector3(tmpScale, tmpScale, tmpScale);
            yield return null;
        }
        if (finalScale == 0f)
        {
            transform.localScale = Vector3.zero;
            zoomingInEvent.Invoke(false);
        }
        else
        {
            zoomingInEvent.Invoke(true);
        }
    }

    public bool GetIsActivated()
    {
        return isActivated;
    }
}
