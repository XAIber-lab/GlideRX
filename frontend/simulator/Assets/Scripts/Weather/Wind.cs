using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Wind : MonoBehaviour
{
    public Vector3 wind = Vector3.zero;
    public float influence = 500f;
    public Vector3 variability = Vector3.zero;

    private Vector3 notAlteredWind = Vector3.zero;

    void Start()
    {
        notAlteredWind = wind;
        StartCoroutine(alterWind());
    }

    IEnumerator alterWind()
    {
        for (; ; )
        {
            wind = notAlteredWind + new Vector3(Random.Range(-variability.x, variability.x), Random.Range(-variability.y, variability.y), Random.Range(-variability.z, variability.z));
            yield return new WaitForSeconds(5f);
        }
    }
}
