using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtSevereThreat : MonoBehaviour
{
    public AirTrafficController airTrafficController;
    public float hiddenPosition = -0.7f;
    public float visiblePosition = -0.35f;
    public Vector3 scale = Vector3.zero;
    public Vector3 localRotationOffset = Vector3.zero;

    public float severeMorphingSpeed = 0.025f;
    public float moderateHighMorphingSpeed = 0.01f;

    private RiskSeverity previousRiskSeverity = RiskSeverity.NONE;
    private RiskSeverity currentRiskSeverity = RiskSeverity.NONE;
    private RiskSeverity updatedRiskSeverity = RiskSeverity.NONE;
    private GameObject currentSevereThreat;

    private bool comingFromASevereThreat = false;
    // Start is called before the first frame update
    void Start()
    {
        airTrafficController.trafficEvent.AddListener(onTrafficEvent);
        airTrafficController.severeThreatEvent.AddListener(onSevereThreat);
    }

    public void onSevereThreat(GameObject sailplane)
    {
        currentSevereThreat = sailplane;
    }

    public void onTrafficEvent(int riskSeverity)
    {
        updatedRiskSeverity = (RiskSeverity)riskSeverity;
        if (updatedRiskSeverity != currentRiskSeverity)
        {
            previousRiskSeverity = currentRiskSeverity;
            currentRiskSeverity = updatedRiskSeverity;
            // morph in
            if (currentRiskSeverity == RiskSeverity.SEVERE)
            {
                StartCoroutine(ScaleAndMove(hiddenPosition, visiblePosition, severeMorphingSpeed));
            }
            // morph out
            if (previousRiskSeverity == RiskSeverity.SEVERE)
            {
                StartCoroutine(ScaleAndMove(visiblePosition, hiddenPosition, moderateHighMorphingSpeed));
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (currentRiskSeverity == RiskSeverity.SEVERE && currentSevereThreat != null)
        {
            transform.LookAt(currentSevereThreat.transform);
            transform.Rotate(localRotationOffset, Space.Self);
        }
    }

    IEnumerator ScaleAndMove(float startingVerticalPosition, float endingVerticalPosition, float morphingSpeed)
    {
        /*
        float startingY = -1.8f;
        float endingY = -0.8f;
        */

        for (float alpha = 0f; alpha <= 1; alpha += morphingSpeed)
        {
            float y = (1 - alpha) * startingVerticalPosition + alpha * endingVerticalPosition;
            Vector3 currPosition = transform.localPosition;
            transform.localPosition = new Vector3(currPosition.x, y, currPosition.z);

            float sizeX;
            float sizeY;
            float sizeZ;
            if (startingVerticalPosition < endingVerticalPosition)
            {
                sizeX = alpha * scale.x;
                sizeY = alpha * scale.y;
                sizeZ = alpha * scale.z;
            }
            else
            {
                float inverse_alpha = 1 - alpha;
                sizeX = inverse_alpha * scale.x;
                sizeY = inverse_alpha * scale.y;
                sizeZ = inverse_alpha * scale.z;
            }

            transform.localScale = new Vector3(sizeX, sizeY, sizeZ);

            yield return null;
        }
    }
}
