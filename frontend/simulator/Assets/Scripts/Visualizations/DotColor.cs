using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DotColor : MonoBehaviour
{
    // private Color moderateColor = new Color(255f, 69f, 0f, 0f);
    // private Color highColor = Color.red;
    public string shaderColorPropertyName = "_TintColor";
    public Color moderateThreatColor = Color.yellow;
    public Color severeThreatColor = Color.red;

    // private Color currentColor;
    private AirTrafficController airTrafficController;
    private Transform pilotSailplane;
    private Transform sailplane;
    private float distanceRange;
    private float severeThreashold;
    private Renderer rendererDot;
    private Renderer rendererTrajectory;

    // Start is called before the first frame update
    void Start()
    {
        airTrafficController = GameObject.FindWithTag("AirTrafficController").GetComponent<AirTrafficController>();
        // airTrafficController.trafficEvent.AddListener(onTrafficEvent);
        pilotSailplane = airTrafficController.pilotSailplane.transform;
        rendererDot = gameObject.GetComponent<Renderer>();
        rendererTrajectory = transform.GetChild(0).GetComponent<Renderer>();
        severeThreashold = airTrafficController.severeThreashold;
        distanceRange = (airTrafficController.moderateThreashold - severeThreashold);
    }

    public void SetSailplane(Transform sailplane)
    {
        this.sailplane = sailplane;
    }

    /*
    private void OnEnable()
    {
        var risk = airTrafficController.currentStateInt;
        if (risk != (int)RiskSeverity.MODERATE || risk != (int)RiskSeverity.HIGH)
        {
            currentColor = moderateColor;
        }
        else
        {
            if (risk == (int)RiskSeverity.MODERATE)
                currentColor = moderateColor;
            else if (risk == (int)RiskSeverity.HIGH)
                currentColor = highColor;
        }
    }

    private void onTrafficEvent(int riskSeverity)
    {
        if ((RiskSeverity) riskSeverity == RiskSeverity.MODERATE)
        {
            currentColor = moderateColor;
        }
        else if ((RiskSeverity)riskSeverity == RiskSeverity.HIGH)
        {
            currentColor = highColor;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.GetComponent<Renderer>().material.SetColor(shaderColorPropertyName, currentColor);
    }
    */

    private void Update()
    {
        float distance = severeThreashold + distanceRange;
        if (sailplane != null)
        {
            distance = (sailplane.transform.position - pilotSailplane.transform.position).magnitude;
        }
        if (distance <= severeThreashold)
        {
            rendererDot.material.SetColor(shaderColorPropertyName, severeThreatColor);
            rendererTrajectory.material.SetColor(shaderColorPropertyName, severeThreatColor);
        }
        else
        {
            float t = (distance - severeThreashold) / distanceRange;
            if (t > 1)
                t = 1;
            // Debug.Log("DotColor: " + t);
            rendererDot.material.SetColor(shaderColorPropertyName, Color.Lerp(severeThreatColor, moderateThreatColor, t));
            rendererTrajectory.material.SetColor(shaderColorPropertyName, Color.Lerp(severeThreatColor, moderateThreatColor, t));
        }
    }
}
