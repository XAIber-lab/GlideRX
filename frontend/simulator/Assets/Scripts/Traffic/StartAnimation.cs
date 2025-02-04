using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Splines;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.Events;

[System.Serializable]
public class WaypointCheckedEvent : UnityEvent<Waypoint> { }

public class StartAnimation : MonoBehaviour, IWaypointEvent
{
    public float triggeringDistance = 500f;
    public Canvas logCanvas;
    public float morphingSpeed = 0.005f;
    public float triggeringHeadingThreashold = 0.5f;

    public WaypointCheckedEvent waypointCheckedEvent { get; set; }

    private GameObject pilotSailplane;
    private Transform waypoint;
    private TextMeshProUGUI waypointLog;
    // Start is called before the first frame update
    void Start()
    {
        pilotSailplane = GameObject.FindWithTag("PilotSailplane");
        waypoint = transform.GetChild(2);
        waypointLog = logCanvas.transform.GetChild(0).gameObject.GetComponent<TextMeshProUGUI>();
        transform.GetChild(1).gameObject.SetActive(false);
        StartCoroutine(CheckPosition());
    }

    private void Awake()
    {
        if (waypointCheckedEvent == null)
        {
            waypointCheckedEvent = new WaypointCheckedEvent();
        }
    }

    IEnumerator CheckPosition()
    {
        for (; ; )
        {
            Vector2 horizontalPosition = new Vector2(pilotSailplane.transform.position.x, pilotSailplane.transform.position.z);
            Vector2 waypointHorizontalPosition = new Vector2(waypoint.position.x, waypoint.position.z);
            float distanceFromWaypoint = (waypointHorizontalPosition - horizontalPosition).magnitude;
            SplineAnimate intruderSplineAnimate = transform.GetChild(1).GetChild(0).gameObject.GetComponent<SplineAnimate>();
            if (distanceFromWaypoint < triggeringDistance && !intruderSplineAnimate.IsPlaying)
            {
                waypointCheckedEvent.Invoke(new Waypoint(
                    timestamp: GetCurrentTime(),
                    waypointID: waypoint.name, 
                    altitude: pilotSailplane.transform.position.y));
                StartCoroutine(FadeInOut(waypointLog, morphingSpeed));
                transform.GetChild(1).gameObject.SetActive(true);
                intruderSplineAnimate.Play();
            }
            else if (!intruderSplineAnimate.IsPlaying)
            {
                transform.GetChild(1).gameObject.SetActive(false);
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

    private int GetCurrentTime()
    {
        return (int)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }
}
