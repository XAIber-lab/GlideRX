using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Splines;

[System.Serializable]
public class FlightStatsLogEvent : UnityEvent<FlightStats> { }
public class FlightStatsLog : MonoBehaviour
{
    public FlightStatsLogEvent flightStatsLogEvent;

    private bool isPlaying = false;

    private void Start()
    {
        StartCoroutine(CheckIfIsPlaying());
    }

    private void Awake()
    {
        if (flightStatsLogEvent == null)
        {
            flightStatsLogEvent = new FlightStatsLogEvent();
        }
    }

    IEnumerator CheckIfIsPlaying()
    {
        for (; ; )
        {
            if (!isPlaying && transform.parent.GetComponent<SplineAnimate>().IsPlaying)
            {
                Debug.Log("Sailplane " + transform.name + "is flying");
                isPlaying = true;
                StartCoroutine(GetAverageValue());
            }
            yield return new WaitForSeconds(1);
        }
    }

    IEnumerator GetAverageValue()
    {
        Vector3 initialPosition = transform.position;
        yield return new WaitForSeconds(10f);
        Vector3 finalPosition = transform.position;
        float heading = transform.rotation.eulerAngles.y;
        float verticalSpeed = (finalPosition - initialPosition).y / 10f;
        FlightStats flightStats = new FlightStats(
            intruderID: transform.name,
            heading: heading,
            variometer: verticalSpeed);
        Debug.Log("Invoking flight stats log from " + transform.name);
        flightStatsLogEvent.Invoke(flightStats);
    }
}
