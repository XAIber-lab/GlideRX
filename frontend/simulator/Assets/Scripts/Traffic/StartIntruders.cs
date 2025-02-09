using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Splines;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Events;

public class StartIntruders : MonoBehaviour
{
    public List<GameObject> intruders;
    public float triggeringDistance = 500f;
    public Canvas logCanvas;
    public float morphingSpeed = 0.005f;

    private TextMeshProUGUI waypointLog;
    // Start is called before the first frame update
    void Start()
    {
        waypointLog = logCanvas.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        foreach (GameObject intruder in intruders)
        {
            intruder.transform.GetChild(1).gameObject.SetActive(false);
        }
        StartCoroutine(CheckPosition());
    }

    IEnumerator CheckPosition()
    {
        for (; ; )
        {
            int cnt = 1;
            foreach (GameObject intruder in intruders)
            {
                Vector2 horizontalPosition = new Vector2(transform.position.x, transform.position.z);
                Vector2 waypointHorizontalPosition = new Vector2(intruder.transform.GetChild(2).position.x, intruder.transform.GetChild(2).position.z);
                float distanceFromWaypoint = (waypointHorizontalPosition - horizontalPosition).magnitude;
                // Debug.Log("Distance from waypoint " + cnt + ": " + distanceFromWaypoint);
                cnt++;
                SplineAnimate intruderSplineAnimate = intruder.transform.GetChild(1).gameObject.GetComponent<SplineAnimate>();
                if (distanceFromWaypoint < triggeringDistance && !intruderSplineAnimate.IsPlaying)
                {
                    // Debug.Log("Starting Spline Animation for intruder: " + cnt);
                    StartCoroutine(FadeInOut(waypointLog, morphingSpeed));
                    intruder.transform.GetChild(1).gameObject.SetActive(true);
                    intruder.transform.GetChild(1).gameObject.GetComponent<SplineAnimate>().Play();
                }
                if (!intruderSplineAnimate)
                {
                    intruder.transform.GetChild(1).gameObject.SetActive(false);
                }
            }
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator FadeInOut(TextMeshProUGUI textToFade, float morphingSpeed)
    {
        Color color = textToFade.color;
        for (float alpha = 0f; alpha <= 1; alpha += morphingSpeed)
        {
            color.a = alpha;
            textToFade.color = color;
            yield return null;
        }

        yield return new WaitForSeconds(5f);

        for (float alpha = 1f; alpha >= 0; alpha -= morphingSpeed)
        {
            color.a = alpha;
            textToFade.color = color;
            yield return null;
        }
        color.a = 0f;
        textToFade.color = color;
    }
}
