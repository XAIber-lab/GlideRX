using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class ActivateVisualizationsWhenInSight : MonoBehaviour
{
    public List<GameObject> visualizations;
    public float triggeringLookingThreashold = 0.5f;
    public float refreshRate = 0.25f;

    private Camera m_MainCamera;
    private Transform sailplane;
    private bool amIActivated = false;

    void OnEnable()
    {
        m_MainCamera = Camera.main;
        sailplane = transform.GetChild(0);
        foreach (GameObject visualization in visualizations)
        {
            visualization.SetActive(false);
        }
        StartCoroutine(CheckIfInSight());
    }

    IEnumerator CheckIfInSight()
    {
        for (; ; )
        {
            bool nowInSight = InSight();
            if (nowInSight && !amIActivated)
            {
                foreach (GameObject visualization in visualizations)
                {
                    visualization.SetActive(true);
                }
                amIActivated = true;
            }
            else if (!nowInSight && amIActivated)
            {
                foreach (GameObject visualization in visualizations)
                {
                    visualization.SetActive(false);
                }
                amIActivated = false;
            }
            yield return new WaitForSeconds(refreshRate);
        }
    }

    private bool InSight()
    {
        Vector3 sailplanePos = sailplane.position;
        Vector3 cameraPos = m_MainCamera.transform.position;
        Vector3 relativeDirection = (sailplanePos - cameraPos).normalized;
        Vector3 lookingDirection = m_MainCamera.transform.rotation * Vector3.forward;
        float lookingAngularDistance = (float)Math.Acos((double)Vector3.Dot(relativeDirection, lookingDirection));
        if (lookingAngularDistance < triggeringLookingThreashold)
            return true;
        else
            return false;
    }
}
