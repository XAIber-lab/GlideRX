using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR.Interaction.Toolkit.Filtering;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class WeatherEvent : UnityEvent<Vector3> { }

public class WindAtCurrentPosition : MonoBehaviour
{
    public GameObject weather;
    public WeatherEvent weatherEvent;

    public Vector3 windStrengthAtCurrentPosition = Vector3.zero;
    public float windUpdateFrequency = 5f;
    private List<GameObject> weatherPhenomena = new List<GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform weatherPhenomenon in weather.transform)
        {
            Debug.Log("WEATHER PHENOMENON ADDED");
            weatherPhenomena.Add(weatherPhenomenon.gameObject);
        }
        StartCoroutine(updateWind());
    }

    private void Awake()
    {
        if (weatherEvent == null)
        {
            weatherEvent = new WeatherEvent();
        }
    }

    IEnumerator updateWind()
    {
        for (; ;)
        {
            var currentPosition = transform.position;
            Vector3 windSum = Vector3.zero;
            foreach (GameObject weatherPhenomenon in weatherPhenomena)
            {
                float distance = (weatherPhenomenon.transform.position - currentPosition).magnitude;
                // contribution is a value from 0 to 1. When distance is equal or greater than influence, the contribution is zero. It decays linearly with the distance.
                float contribution = 0f;
                var influence = weatherPhenomenon.GetComponent<Wind>().influence;
                if (distance < influence)
                    contribution = (1f / -influence) * (distance - influence);
                windSum += contribution * weatherPhenomenon.GetComponent<Wind>().wind;
            }
            windStrengthAtCurrentPosition = windSum;
            weatherEvent.Invoke(windStrengthAtCurrentPosition);
            yield return new WaitForSeconds(windUpdateFrequency);
        }
    }
}
